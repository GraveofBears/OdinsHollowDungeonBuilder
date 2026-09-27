using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OdinsHollow.DungeonGen
{
    // Lives on every loaded OdinsHollowDungeon location. The layout is stored in the location's ZDO: the owner
    // generates it once, and every client builds the (non-networked) rooms from it, like Valheim's own dungeons.
    public class OHDungeon : MonoBehaviour
    {
        internal static readonly List<OHDungeon> Instances = new();

        private ZNetView? m_nview;
        private Location? m_location;
        private string m_builtLayout = "";
        private readonly List<GameObject> m_rooms = new();
        private readonly Dictionary<BoxCollider, Bounds> m_originalZoneBounds = new();
        private float m_originalInteriorRadius = -1f;
        private float m_startTime;
        private float m_nextCheck;
        private Coroutine? m_building;

        private void Start()
        {
            m_nview = FindProxyView();
            if (m_nview == null)
            {
                // Not a real placed location (e.g. a world generation ghost).
                Destroy(this);
                return;
            }

            m_location = GetComponent<Location>();
            m_startTime = Time.time;
            Instances.Add(this);
        }

        private void OnDestroy() => Instances.Remove(this);

        private ZNetView? FindProxyView()
        {
            if (GetComponentInParent<LocationProxy>() is { } parentProxy)
            {
                return parentProxy.GetComponent<ZNetView>();
            }

            foreach (LocationProxy proxy in FindObjectsOfType<LocationProxy>())
            {
                if (proxy.m_instance == gameObject) return proxy.GetComponent<ZNetView>();
            }

            return null;
        }

        private void Update()
        {
            if (Time.time < m_nextCheck) return;
            m_nextCheck = Time.time + 1f;
            if (m_nview == null || !m_nview.IsValid()) return;

            ZDO zdo = m_nview.GetZDO();
            string layout = zdo.GetString(DungeonManager.LayoutKey);

            // Give the zone a few seconds to load player-built pieces before deciding whether to generate.
            if (layout.Length == 0 && DungeonConfig.AutoGenerate.Value && m_nview.IsOwner() && !zdo.GetBool(DungeonManager.ManualKey) && Time.time - m_startTime > 5f)
            {
                if (HasPlayerBuiltPieces())
                {
                    Debug.Log("[OdinsHollow] Dungeon already has player-built pieces; skipping auto-generation. Use OHDungeonGenerate to force it.");
                    zdo.Set(DungeonManager.ManualKey, true);
                    return;
                }

                layout = Generate();
            }

            if (layout != m_builtLayout)
            {
                Rebuild(layout);
            }
        }

        internal bool IsOwner => m_nview != null && m_nview.IsValid() && m_nview.IsOwner();

        internal void ClaimOwnership() => m_nview?.ClaimOwnership();

        // Generates a new layout and its content. The caller must own the ZDO.
        internal string Generate()
        {
            if (m_nview == null || !m_nview.IsValid()) return "";
            ZDO zdo = m_nview.GetZDO();

            Transform? start = FindStartConnector();
            if (start == null)
            {
                Debug.LogError("[OdinsHollow] No start Connector found in the OdinsHollowDungeon location.");
                return "";
            }

            Transform root = transform;
            Vector3 startPosition = Quaternion.Inverse(root.rotation) * (start.position - root.position);
            Quaternion startRotation = Quaternion.Inverse(root.rotation) * start.rotation;

            int seed = Random.Range(1, int.MaxValue);
            List<PlacedRoom> rooms = DungeonManager.GenerateLayout(startPosition, startRotation, StartRoomBounds(start), seed);
            string layout = DungeonManager.Serialize(seed, rooms);

            int dungeonId = Random.Range(1, int.MaxValue);
            zdo.Set(DungeonManager.DungeonIdKey, dungeonId);
            zdo.Set(DungeonManager.ManualKey, false);
            zdo.Set(DungeonManager.LayoutKey, layout);
            // Content is spawned by the owner once the rooms are built (see BuildRooms).
            zdo.Set(DungeonManager.ContentPendingKey, rooms.Count > 0);
            return layout;
        }

        // Removes the generated rooms and content. With 'markManual' the dungeon won't auto-generate again.
        internal int Clear(bool markManual)
        {
            if (m_nview == null || !m_nview.IsValid()) return 0;
            ZDO zdo = m_nview.GetZDO();
            int removed = DungeonManager.DestroyContent(zdo.GetInt(DungeonManager.DungeonIdKey));
            zdo.Set(DungeonManager.DungeonIdKey, 0);
            zdo.Set(DungeonManager.ContentPendingKey, false);
            zdo.Set(DungeonManager.LayoutKey, "");
            zdo.Set(DungeonManager.ManualKey, markManual);
            Rebuild("");
            return removed;
        }

        // The dungeon_start_room's connector if there is one, otherwise the Entrance's, otherwise the first one.
        private Transform? FindStartConnector()
        {
            List<Transform> connectors = DungeonManager.FindConnectors(transform)
                .Where(c => !m_rooms.Any(room => room != null && c.IsChildOf(room.transform)))
                .ToList();
            return connectors.FirstOrDefault(c => HasAncestorNamed(c, "dungeon_start"))
                   ?? connectors.FirstOrDefault(c => HasAncestorNamed(c, "entrance"))
                   ?? connectors.FirstOrDefault();
        }

        private bool HasAncestorNamed(Transform t, string name)
        {
            for (Transform? p = t.parent; p != null && p != transform; p = p.parent)
            {
                if (p.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }

            return false;
        }

        // Bounds of the start room's meshes, so generated rooms don't grow back into it.
        private Obb? StartRoomBounds(Transform start)
        {
            Transform room = start.parent;
            if (room == null || room == transform) return null;

            Bounds? bounds = null;
            foreach (MeshFilter filter in room.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null) continue;
                Bounds b = filter.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = b.center + Vector3.Scale(b.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 point = Quaternion.Inverse(transform.rotation) * (filter.transform.TransformPoint(corner) - transform.position);
                    if (bounds is { } existing)
                    {
                        existing.Encapsulate(point);
                        bounds = existing;
                    }
                    else
                    {
                        bounds = new Bounds(point, Vector3.zero);
                    }
                }
            }

            return bounds is { } result ? new Obb(result.center, Quaternion.identity, result.extents) : null;
        }

        private bool HasPlayerBuiltPieces()
        {
            Transform? start = FindStartConnector();
            if (start == null) return false;
            foreach (Collider collider in Physics.OverlapSphere(start.position, 80f))
            {
                if (collider.GetComponentInParent<Piece>() is { } piece && piece.IsPlacedByPlayer()) return true;
            }

            return false;
        }

        private void Rebuild(string layout)
        {
            if (m_building != null) StopCoroutine(m_building);
            foreach (GameObject room in m_rooms)
            {
                if (room != null) Destroy(room);
            }

            m_rooms.Clear();
            m_builtLayout = layout;
            if (layout.Length == 0)
            {
                RestoreInterior();
                return;
            }

            if (!DungeonManager.TryDeserialize(layout, out int seed, out List<PlacedRoom> rooms))
            {
                Debug.LogError("[OdinsHollow] Stored dungeon layout could not be read. Use OHDungeonGenerate to regenerate it.");
                return;
            }

            m_building = StartCoroutine(BuildRooms(rooms, seed));
        }

        private IEnumerator BuildRooms(List<PlacedRoom> rooms, int seed)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                PlacedRoom room = rooms[i];
                GameObject instance = Instantiate(room.Room.Template, transform.position + transform.rotation * room.Position, transform.rotation * room.Rotation, transform);
                instance.name = room.Room.Name;
                ApplyRandomSpawns(instance, new System.Random(unchecked(seed * 17 + i)));
                m_rooms.Add(instance);

                // Spread the work over a few frames to avoid a long hitch.
                if (i % 3 == 2) yield return null;
            }

            ExpandInterior(rooms);

            // Let the new room colliders settle before spawning chests and spawners onto their floors.
            yield return null;
            Physics.SyncTransforms();
            if (IsOwner && m_nview!.GetZDO() is { } zdo && zdo.GetBool(DungeonManager.ContentPendingKey))
            {
                zdo.Set(DungeonManager.ContentPendingKey, false);
                DungeonManager.SpawnContent(transform, rooms, seed, zdo.GetInt(DungeonManager.DungeonIdKey));
            }

            m_building = null;
        }

        private static void ApplyRandomSpawns(GameObject room, System.Random rng)
        {
            foreach (RandomSpawn randomSpawn in room.GetComponentsInChildren<RandomSpawn>(true))
            {
                bool spawn = rng.NextDouble() * 100.0 < randomSpawn.m_chanceToSpawn;
                if (randomSpawn.m_OffObject != null) randomSpawn.m_OffObject.SetActive(!spawn);
                randomSpawn.gameObject.SetActive(spawn);
            }
        }

        // Grows the interior environment (weather/lighting) and interior radius to cover the generated rooms.
        private void ExpandInterior(List<PlacedRoom> rooms)
        {
            List<Vector3> corners = rooms.SelectMany(r => r.Room.Bounds.Transformed(transform.position + transform.rotation * r.Position, transform.rotation * r.Rotation).Corners()).ToList();
            if (corners.Count == 0) return;

            foreach (EnvZone zone in GetComponentsInChildren<EnvZone>(true))
            {
                if (zone.GetComponent<BoxCollider>() is not { } box) continue;
                if (!m_originalZoneBounds.ContainsKey(box)) m_originalZoneBounds[box] = new Bounds(box.center, box.size);
                Bounds bounds = m_originalZoneBounds[box];
                foreach (Vector3 corner in corners) bounds.Encapsulate(box.transform.InverseTransformPoint(corner));
                box.center = bounds.center;
                box.size = bounds.size;
            }

            if (m_location != null && m_location.m_hasInterior)
            {
                if (m_originalInteriorRadius < 0f) m_originalInteriorRadius = m_location.m_interiorRadius;
                Vector3 center = m_location.m_interiorTransform != null ? m_location.m_interiorTransform.position : transform.position;
                float radius = corners.Max(c => Utils.DistanceXZ(c, center)) + 10f;
                m_location.m_interiorRadius = Mathf.Max(m_originalInteriorRadius, radius);
            }
        }

        private void RestoreInterior()
        {
            foreach (KeyValuePair<BoxCollider, Bounds> kv in m_originalZoneBounds)
            {
                if (kv.Key == null) continue;
                kv.Key.center = kv.Value.center;
                kv.Key.size = kv.Value.size;
            }

            if (m_location != null && m_originalInteriorRadius >= 0f) m_location.m_interiorRadius = m_originalInteriorRadius;
        }

        // Closest dungeon by distance to either the location or its start connector (the interior can sit far from the entrance).
        internal static OHDungeon? FindNearest(Vector3 position, float maxDistance)
        {
            OHDungeon? best = null;
            float bestDistance = maxDistance;
            foreach (OHDungeon dungeon in Instances)
            {
                if (dungeon == null) continue;
                float distance = Utils.DistanceXZ(dungeon.transform.position, position);
                if (dungeon.FindStartConnector() is { } start) distance = Mathf.Min(distance, Vector3.Distance(start.position, position));
                if (distance <= bestDistance)
                {
                    best = dungeon;
                    bestDistance = distance;
                }
            }

            return best;
        }
    }
}
