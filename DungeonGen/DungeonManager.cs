using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PieceManager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OdinsHollow.DungeonGen
{
    internal enum RoomTheme
    {
        Cave,
        Frost,
        Ruins
    }

    internal enum ContentKind
    {
        Chest,
        Spawner,
        Other
    }

    internal class ConnectorInfo
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    internal class ContentPoint
    {
        public string? Prefab; // null = picked from config when the dungeon is generated
        public ContentKind Kind;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    internal class RoomTemplate
    {
        public string Name = "";
        public GameObject Template = null!;
        public RoomTheme Theme;
        public bool IsEnd;
        public bool IsPlug;
        public List<ConnectorInfo> Connectors = new();
        public Obb Bounds;
        public List<ContentPoint> Content = new();
    }

    internal class PlacedRoom
    {
        public RoomTemplate Room = null!;
        public Vector3 Position; // relative to the location root
        public Quaternion Rotation; // relative to the location root
    }

    internal static class DungeonManager
    {
        internal const string LayoutKey = "OH_DG_Layout";
        internal const string DungeonIdKey = "OH_DG_Id";
        internal const string ManualKey = "OH_DG_Manual";
        internal const string CreatureKey = "OH_DG_Creature";

        private const string LayoutVersion = "OHDG1";

        private static readonly List<GameObject> DungeonGenPrefabs = new();
        private static readonly Dictionary<string, RoomTemplate> Templates = new();
        private static GameObject? templateHolder;

        internal static void RegisterLocationPrefab(GameObject? locationPrefab)
        {
            if (locationPrefab == null)
            {
                Debug.LogError("[OdinsHollow] OdinsHollowDungeon location prefab not found; dungeon generation is disabled.");
                return;
            }

            if (locationPrefab.GetComponent<OHDungeon>() == null)
            {
                locationPrefab.AddComponent<OHDungeon>();
            }
        }

        // Finds every prefab in the bundle's DungeonGen folder. Rooms are kept as templates; networked prefabs
        // (chests, spawners) are added to ZNetScene so they can be spawned into dungeons.
        internal static void LoadDungeonGenAssets(AssetBundle bundle)
        {
            if (bundle == null) return;

            foreach (string path in bundle.GetAllAssetNames())
            {
                if (!path.EndsWith(".prefab", StringComparison.Ordinal)) continue;
                bool inFolder = path.Contains("/dungeongen/");
                bool dgName = System.IO.Path.GetFileName(path).StartsWith("oh_dg_", StringComparison.Ordinal);
                if (!inFolder && !dgName) continue;

                GameObject prefab = bundle.LoadAsset<GameObject>(path);
                if (prefab == null) continue;
                DungeonGenPrefabs.Add(prefab);

                if (FindConnectors(prefab.transform).Count == 0 && prefab.GetComponent<ZNetView>() != null)
                {
                    PiecePrefabManager.RegisterPrefab(bundle, prefab.name);
                }
            }

            Debug.Log($"[OdinsHollow] Found {DungeonGenPrefabs.Count} DungeonGen prefabs.");
        }

        internal static bool IsRoomPrefab(GameObject prefab) => FindConnectors(prefab.transform).Count > 0 && !prefab.name.ToLowerInvariant().Contains("odinshollow");

        #region Templates

        private static void EnsureTemplates()
        {
            if (templateHolder != null) return;

            templateHolder = new GameObject("OH_DungeonGenTemplates");
            templateHolder.SetActive(false);
            Object.DontDestroyOnLoad(templateHolder);

            foreach (GameObject prefab in DungeonGenPrefabs)
            {
                if (!IsRoomPrefab(prefab) || Templates.ContainsKey(prefab.name)) continue;
                try
                {
                    Templates[prefab.name] = BuildTemplate(prefab);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[OdinsHollow] Failed to prepare dungeon room {prefab.name}: {e}");
                }
            }

            Debug.Log($"[OdinsHollow] Prepared {Templates.Count} dungeon rooms: {string.Join(", ", Templates.Values.Select(t => $"{t.Name} ({t.Theme}, {t.Connectors.Count} connectors{(t.IsEnd ? ", end" : "")})"))}");
        }

        // Copies the room under an inactive holder (so nothing on it wakes up) and strips everything networked from
        // it. Networked children are remembered as content and spawned for real once, by the dungeon's owner.
        private static RoomTemplate BuildTemplate(GameObject prefab)
        {
            GameObject copy = Object.Instantiate(prefab, templateHolder!.transform);
            copy.name = prefab.name;
            copy.transform.localPosition = Vector3.zero;
            copy.transform.localRotation = Quaternion.identity;
            Transform root = copy.transform;

            RoomTemplate room = new() { Name = prefab.name, Template = copy };
            string lowerName = prefab.name.ToLowerInvariant();
            room.Theme = lowerName.Contains("frost") ? RoomTheme.Frost : lowerName.Contains("ruin") ? RoomTheme.Ruins : RoomTheme.Cave;

            foreach (ZNetView nview in copy.GetComponentsInChildren<ZNetView>(true))
            {
                // Children of content that was already removed are gone too.
                if (nview == null || nview.gameObject == copy) continue;
                GameObject content = nview.gameObject;
                room.Content.Add(new ContentPoint
                {
                    Prefab = Utils.GetPrefabName(content.name),
                    Kind = content.GetComponentInChildren<Container>(true) != null ? ContentKind.Chest : content.GetComponentInChildren<CreatureSpawner>(true) != null ? ContentKind.Spawner : ContentKind.Other,
                    Position = RelativePosition(root, content.transform),
                    Rotation = RelativeRotation(root, content.transform)
                });
                Object.DestroyImmediate(content);
            }

            foreach (Transform t in copy.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("OH_ChestPoint", StringComparison.OrdinalIgnoreCase) || t.name.StartsWith("OH_SpawnerPoint", StringComparison.OrdinalIgnoreCase))
                {
                    room.Content.Add(new ContentPoint
                    {
                        Prefab = null,
                        Kind = t.name.StartsWith("OH_ChestPoint", StringComparison.OrdinalIgnoreCase) ? ContentKind.Chest : ContentKind.Spawner,
                        Position = RelativePosition(root, t),
                        Rotation = RelativeRotation(root, t)
                    });
                }
            }

            // Rooms are local, non-networked objects: remove everything that expects a ZNetView.
            StripComponents<WearNTear>(copy);
            StripComponents<Piece>(copy);
            StripComponents<TerrainModifier>(copy);
            StripComponents<TimedDestruction>(copy);
            StripComponents<ZSyncTransform>(copy);
            StripComponents<ZNetView>(copy);

            foreach (Transform connector in FindConnectors(root))
            {
                room.Connectors.Add(new ConnectorInfo { Position = RelativePosition(root, connector), Rotation = RelativeRotation(root, connector) });
            }

            room.IsEnd = room.Connectors.Count == 1;
            room.IsPlug = room.IsEnd && lowerName.Contains("hall_end");
            room.Bounds = ComputeBounds(copy, room.Connectors);
            return room;
        }

        private static void StripComponents<T>(GameObject go) where T : Component
        {
            foreach (T component in go.GetComponentsInChildren<T>(true))
            {
                if (component != null) Object.DestroyImmediate(component);
            }
        }

        // A child named "RoomBounds" with a BoxCollider sets the room's size exactly; otherwise the meshes are used.
        private static Obb ComputeBounds(GameObject room, List<ConnectorInfo> connectors)
        {
            Transform root = room.transform;
            Transform? boundsOverride = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.Equals("RoomBounds", StringComparison.OrdinalIgnoreCase));
            if (boundsOverride != null && boundsOverride.GetComponent<BoxCollider>() is { } box)
            {
                Obb result = new(RelativePosition(root, boundsOverride, box.center), RelativeRotation(root, boundsOverride), Vector3.Scale(box.size, boundsOverride.lossyScale) * 0.5f);
                Object.DestroyImmediate(boundsOverride.gameObject);
                return result;
            }

            bool any = false;
            Bounds bounds = default;
            foreach (MeshFilter filter in room.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null) continue;
                Bounds meshBounds = filter.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = meshBounds.center + Vector3.Scale(meshBounds.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 point = Quaternion.Inverse(root.rotation) * (filter.transform.TransformPoint(corner) - root.position);
                    if (!any)
                    {
                        bounds = new Bounds(point, Vector3.zero);
                        any = true;
                    }
                    else
                    {
                        bounds.Encapsulate(point);
                    }
                }
            }

            if (!any)
            {
                bounds = new Bounds(connectors.Count > 0 ? connectors[0].Position : Vector3.zero, Vector3.one * 10f);
                foreach (ConnectorInfo connector in connectors) bounds.Encapsulate(connector.Position);
            }

            return new Obb(bounds.center, Quaternion.identity, bounds.extents);
        }

        internal static List<Transform> FindConnectors(Transform root)
        {
            List<Transform> result = new();
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t != root && t.name.StartsWith("Connector", StringComparison.OrdinalIgnoreCase) && t.name.IndexOf("Box", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    result.Add(t);
                }
            }

            return result;
        }

        private static Vector3 RelativePosition(Transform root, Transform t, Vector3 localOffset = default) => Quaternion.Inverse(root.rotation) * (t.TransformPoint(localOffset) - root.position);

        private static Quaternion RelativeRotation(Transform root, Transform t) => Quaternion.Inverse(root.rotation) * t.rotation;

        #endregion

        #region Generation

        private struct OpenConnection
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public int ParentBox;
        }

        private static Quaternion Level(Quaternion rotation)
        {
            if (!DungeonConfig.LevelConnectors.Value) return rotation;
            Vector3 forward = Vector3.ProjectOnPlane(rotation * Vector3.forward, Vector3.up);
            return forward.sqrMagnitude < 1e-4f ? rotation : Quaternion.LookRotation(forward.normalized, Vector3.up);
        }

        private static List<RoomTemplate> EnabledRooms()
        {
            EnsureTemplates();
            return Templates.Values.Where(t =>
                (t.Theme == RoomTheme.Cave && DungeonConfig.UseCavePieces.Value) ||
                (t.Theme == RoomTheme.Frost && DungeonConfig.UseFrostPieces.Value) ||
                (t.Theme == RoomTheme.Ruins && DungeonConfig.UseRuinsPieces.Value)).OrderBy(t => t.Name, StringComparer.Ordinal).ToList();
        }

        // Grows a layout outward from the start connector: rooms and halls until the room count is reached, then an
        // end on every connector that is still open. All positions are relative to the location root.
        internal static List<PlacedRoom> GenerateLayout(Vector3 startPosition, Quaternion startRotation, Obb? startBounds, int seed)
        {
            List<RoomTemplate> rooms = EnabledRooms();
            List<RoomTemplate> normal = rooms.Where(r => r.Connectors.Count >= 2).ToList();
            List<RoomTemplate> ends = rooms.Where(r => r.IsEnd && !r.IsPlug).ToList();
            List<RoomTemplate> plugs = rooms.Where(r => r.IsPlug).ToList();
            if (normal.Count == 0)
            {
                Debug.LogWarning("[OdinsHollow] No DungeonGen rooms with 2+ connectors are enabled; check the Use Cave/Frost/Ruins Pieces settings and the DungeonGen prefabs.");
                return new List<PlacedRoom>();
            }

            int min = Math.Min(DungeonConfig.MinRooms.Value, DungeonConfig.MaxRooms.Value);
            int max = Math.Max(DungeonConfig.MinRooms.Value, DungeonConfig.MaxRooms.Value);
            float tolerance = DungeonConfig.OverlapTolerance.Value;

            List<PlacedRoom> best = new();
            int bestCount = -1;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                System.Random rng = new(seed + attempt * 7919);
                int target = rng.Next(min, max + 1);
                List<PlacedRoom> placed = new();
                List<Obb> boxes = new();
                if (startBounds.HasValue) boxes.Add(startBounds.Value);

                List<OpenConnection> open = new() { new OpenConnection { Position = startPosition, Rotation = Level(startRotation), ParentBox = startBounds.HasValue ? 0 : -1 } };
                List<OpenConnection> deadEnds = new();
                int roomCount = 0;

                while (open.Count > 0 && roomCount < target)
                {
                    int index = rng.Next(open.Count);
                    OpenConnection connection = open[index];
                    open.RemoveAt(index);
                    if (TryPlace(connection, normal, rng, placed, boxes, open, tolerance))
                    {
                        roomCount++;
                    }
                    else
                    {
                        deadEnds.Add(connection);
                    }
                }

                deadEnds.AddRange(open);
                open.Clear();
                int unclosed = 0;
                foreach (OpenConnection connection in deadEnds)
                {
                    if (!TryPlace(connection, ends, rng, placed, boxes, open, tolerance) && !TryPlace(connection, plugs, rng, placed, boxes, open, tolerance))
                    {
                        unclosed++;
                    }
                }

                if (unclosed > 0)
                {
                    Debug.LogWarning($"[OdinsHollow] Dungeon attempt {attempt + 1}: {unclosed} connector(s) could not be closed with an end piece.");
                }

                if (roomCount > bestCount)
                {
                    best = placed;
                    bestCount = roomCount;
                }

                if (roomCount >= min) break;
            }

            Debug.Log($"[OdinsHollow] Generated dungeon with {bestCount} rooms and {best.Count - bestCount} ends.");
            return best;
        }

        private static bool TryPlace(OpenConnection connection, List<RoomTemplate> pool, System.Random rng, List<PlacedRoom> placed, List<Obb> boxes, List<OpenConnection> open, float tolerance)
        {
            if (pool.Count == 0) return false;

            Quaternion exitRotation = connection.Rotation * Quaternion.Euler(0f, 180f, 0f);
            foreach (RoomTemplate room in Shuffle(pool, rng).Take(12))
            {
                foreach (int entryIndex in Shuffle(Enumerable.Range(0, room.Connectors.Count).ToList(), rng))
                {
                    ConnectorInfo entry = room.Connectors[entryIndex];
                    Quaternion roomRotation = exitRotation * Quaternion.Inverse(Level(entry.Rotation));
                    Vector3 roomPosition = connection.Position - roomRotation * entry.Position;
                    Obb box = room.Bounds.Transformed(roomPosition, roomRotation);

                    bool blocked = false;
                    for (int i = 0; i < boxes.Count; i++)
                    {
                        if (i != connection.ParentBox && Obb.Intersects(box, boxes[i], tolerance))
                        {
                            blocked = true;
                            break;
                        }
                    }

                    if (blocked) continue;

                    placed.Add(new PlacedRoom { Room = room, Position = roomPosition, Rotation = roomRotation });
                    boxes.Add(box);
                    for (int i = 0; i < room.Connectors.Count; i++)
                    {
                        if (i == entryIndex) continue;
                        ConnectorInfo exit = room.Connectors[i];
                        open.Add(new OpenConnection
                        {
                            Position = roomPosition + roomRotation * exit.Position,
                            Rotation = Level(roomRotation * exit.Rotation),
                            ParentBox = boxes.Count - 1
                        });
                    }

                    return true;
                }
            }

            return false;
        }

        private static List<T> Shuffle<T>(List<T> list, System.Random rng)
        {
            List<T> copy = new(list);
            for (int i = copy.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (copy[i], copy[j]) = (copy[j], copy[i]);
            }

            return copy;
        }

        #endregion

        #region Layout serialization

        internal static string Serialize(int seed, List<PlacedRoom> rooms)
        {
            StringBuilder sb = new();
            sb.Append(LayoutVersion).Append(';').Append(seed.ToString(CultureInfo.InvariantCulture));
            foreach (PlacedRoom room in rooms)
            {
                sb.Append('|').Append(room.Room.Name);
                foreach (float f in new[] { room.Position.x, room.Position.y, room.Position.z, room.Rotation.x, room.Rotation.y, room.Rotation.z, room.Rotation.w })
                {
                    sb.Append(';').Append(f.ToString("R", CultureInfo.InvariantCulture));
                }
            }

            return sb.ToString();
        }

        internal static bool TryDeserialize(string layout, out int seed, out List<PlacedRoom> rooms)
        {
            seed = 0;
            rooms = new List<PlacedRoom>();
            string[] entries = layout.Split('|');
            string[] header = entries[0].Split(';');
            if (header.Length < 2 || header[0] != LayoutVersion || !int.TryParse(header[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out seed)) return false;

            EnsureTemplates();
            for (int i = 1; i < entries.Length; i++)
            {
                string[] parts = entries[i].Split(';');
                if (parts.Length != 8) return false;
                if (!Templates.TryGetValue(parts[0], out RoomTemplate room))
                {
                    Debug.LogWarning($"[OdinsHollow] Dungeon room {parts[0]} no longer exists; skipping it.");
                    continue;
                }

                float[] v = new float[7];
                for (int j = 0; j < 7; j++)
                {
                    if (!float.TryParse(parts[j + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out v[j])) return false;
                }

                rooms.Add(new PlacedRoom { Room = room, Position = new Vector3(v[0], v[1], v[2]), Rotation = new Quaternion(v[3], v[4], v[5], v[6]) });
            }

            return true;
        }

        #endregion

        #region Content

        // Spawns the networked chests, spawners and props for a freshly generated layout. Runs once, on the owner.
        internal static void SpawnContent(Transform locationRoot, List<PlacedRoom> rooms, int seed, int dungeonId)
        {
            List<string> chestPrefabs = LootTables.ParseWeighted(DungeonConfig.ChestPrefabs.Value).Select(kv => kv.Key).ToList();
            List<string> spawnerPrefabs = LootTables.ParseWeighted(DungeonConfig.SpawnerPrefabs.Value).Select(kv => kv.Key).ToList();
            List<KeyValuePair<string, float>> creatures = LootTables.ParseWeighted(DungeonConfig.DungeonCreatures.Value);
            int chests = 0, spawners = 0;

            for (int roomIndex = 0; roomIndex < rooms.Count; roomIndex++)
            {
                PlacedRoom room = rooms[roomIndex];
                System.Random rng = new(unchecked(seed * 31 + roomIndex));
                foreach (ContentPoint point in room.Room.Content)
                {
                    string? prefabName = point.Prefab;
                    switch (point.Kind)
                    {
                        case ContentKind.Chest:
                            if (rng.NextDouble() >= DungeonConfig.ChestChance.Value) continue;
                            prefabName ??= chestPrefabs.Count > 0 ? chestPrefabs[rng.Next(chestPrefabs.Count)] : null;
                            break;
                        case ContentKind.Spawner:
                            if (rng.NextDouble() >= DungeonConfig.SpawnerChance.Value) continue;
                            prefabName ??= spawnerPrefabs.Count > 0 ? spawnerPrefabs[rng.Next(spawnerPrefabs.Count)] : null;
                            break;
                    }

                    GameObject? prefab = prefabName == null ? null : LootTables.FindNetworkedPrefab(prefabName);
                    if (prefab == null)
                    {
                        Debug.LogWarning($"[OdinsHollow] Dungeon content prefab '{prefabName}' not found in {room.Room.Name}.");
                        continue;
                    }

                    Vector3 position = locationRoot.position + locationRoot.rotation * (room.Position + room.Rotation * point.Position);
                    Quaternion rotation = locationRoot.rotation * room.Rotation * point.Rotation;
                    GameObject spawned = Object.Instantiate(prefab, position, rotation);
                    ZNetView nview = spawned.GetComponent<ZNetView>();
                    if (nview == null || nview.GetZDO() == null) continue;

                    ZDO zdo = nview.GetZDO();
                    zdo.Set(DungeonIdKey, dungeonId);

                    CreatureSpawner[] creatureSpawners = spawned.GetComponentsInChildren<CreatureSpawner>();
                    if (creatureSpawners.Length > 0)
                    {
                        GameObject? creature = null;
                        if (LootTables.PickWeighted(creatures, rng) is { } creatureName && ZNetScene.instance.GetPrefab(creatureName) is { } found)
                        {
                            zdo.Set(CreatureKey, creatureName);
                            creature = found;
                        }

                        // CreatureSpawner.Awake already ran, so apply to this instance directly; the Awake patch
                        // re-applies it from the ZDO whenever the spawner is loaded again.
                        foreach (CreatureSpawner spawner in creatureSpawners)
                        {
                            if (creature != null) spawner.m_creaturePrefab = creature;
                            spawner.m_respawnTimeMinuts = DungeonConfig.SpawnerRespawnMinutes.Value;
                        }

                        spawners++;
                    }

                    if (spawned.GetComponent<Container>() != null)
                    {
                        if (spawned.GetComponent<OHLootChest>() == null) spawned.AddComponent<OHLootChest>();
                        chests++;
                    }
                }
            }

            Debug.Log($"[OdinsHollow] Spawned {chests} chests and {spawners} spawners in dungeon {dungeonId}.");
        }

        // Removes every networked object this peer knows about that was spawned for the given dungeon.
        internal static int DestroyContent(int dungeonId)
        {
            if (dungeonId == 0 || ZDOMan.instance == null) return 0;

            List<ZDO> toDestroy = ZDOMan.instance.m_objectsByID.Values.Where(zdo => zdo.GetInt(DungeonIdKey) == dungeonId).ToList();
            foreach (ZDO zdo in toDestroy)
            {
                zdo.SetOwner(ZDOMan.GetSessionID());
                ZDOMan.instance.DestroyZDO(zdo);
            }

            return toDestroy.Count;
        }

        #endregion
    }
}
