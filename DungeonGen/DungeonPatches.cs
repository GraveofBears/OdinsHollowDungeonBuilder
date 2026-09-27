using System;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OdinsHollow.DungeonGen
{
    internal static class DungeonPatches
    {
        // Buildable loot chests and chests spawned into generated dungeons get the loot/respawn behaviour.
        [HarmonyPatch(typeof(Container), nameof(Container.Awake))]
        private static class AddLootChest
        {
            private static void Postfix(Container __instance)
            {
                if (__instance.GetComponent<OHLootChest>() != null) return;
                ZNetView nview = __instance.GetComponentInParent<ZNetView>();
                if (nview == null || nview.GetZDO() == null) return;

                bool buildable = Utils.GetPrefabName(nview.gameObject) == DungeonConfig.BuildableChestPrefab;
                bool dungeon = nview.GetZDO().GetInt(DungeonManager.DungeonIdKey) != 0;
                if (buildable || dungeon)
                {
                    __instance.gameObject.AddComponent<OHLootChest>();
                }
            }
        }

        // Spawners in generated dungeons keep the creature they rolled and use the dungeon respawn time; buildable
        // shroom spawners use their own respawn time.
        [HarmonyPatch(typeof(CreatureSpawner), nameof(CreatureSpawner.Awake))]
        private static class ApplySpawnerSettings
        {
            private static void Postfix(CreatureSpawner __instance) => Apply(__instance);
        }

        private static void Apply(CreatureSpawner spawner)
        {
            ZNetView nview = spawner.GetComponentInParent<ZNetView>();
            if (nview == null || nview.GetZDO() is not { } zdo) return;

            if (zdo.GetInt(DungeonManager.DungeonIdKey) != 0)
            {
                string creature = zdo.GetString(DungeonManager.CreatureKey);
                if (creature.Length > 0 && ZNetScene.instance != null && ZNetScene.instance.GetPrefab(creature) is { } prefab)
                {
                    spawner.m_creaturePrefab = prefab;
                }

                spawner.m_respawnTimeMinuts = DungeonConfig.SpawnerRespawnMinutes.Value;
            }
            else if (Utils.GetPrefabName(nview.gameObject).StartsWith("OH_Spawner_", StringComparison.Ordinal))
            {
                spawner.m_respawnTimeMinuts = DungeonConfig.BuildableSpawnerRespawnMinutes.Value;
            }
        }

        // Respawn times are read from the spawner component, so push config changes to the ones already loaded.
        internal static void ApplySpawnerSettingsToLoaded()
        {
            foreach (CreatureSpawner spawner in Object.FindObjectsOfType<CreatureSpawner>())
            {
                Apply(spawner);
            }
        }

        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
        private static class RegisterCommands
        {
            private static void Postfix()
            {
                _ = new Terminal.ConsoleCommand("OHDungeonGenerate", "[clear] - (Admin) Regenerate the nearest Odins Hollow dungeon with the current config. 'clear' removes its generated rooms, chests and spawners instead.", args =>
                {
                    Terminal? terminal = args.Context;
                    if (!OdinsHollow.IsAdmin)
                    {
                        terminal?.AddString("OHDungeonGenerate: only admins can use this command.");
                        return;
                    }

                    if (Player.m_localPlayer == null)
                    {
                        terminal?.AddString("OHDungeonGenerate: you need to be in a world.");
                        return;
                    }

                    OHDungeon? dungeon = OHDungeon.FindNearest(Player.m_localPlayer.transform.position, 500f);
                    if (dungeon == null)
                    {
                        terminal?.AddString("OHDungeonGenerate: no Odins Hollow dungeon is loaded nearby. Stand at the entrance or inside the dungeon.");
                        return;
                    }

                    bool clear = args.Args.Length > 1 && args.Args[1].Equals("clear", StringComparison.OrdinalIgnoreCase);
                    dungeon.ClaimOwnership();
                    int removed = dungeon.Clear(markManual: clear);
                    if (clear)
                    {
                        terminal?.AddString($"OHDungeonGenerate: removed the generated dungeon ({removed} chests/spawners/props). It won't auto-generate again until you run OHDungeonGenerate.");
                        return;
                    }

                    string layout = dungeon.Generate();
                    terminal?.AddString(layout.Length > 0
                        ? $"OHDungeonGenerate: dungeon regenerated (removed {removed} old chests/spawners/props). Players inside should leave and re-enter."
                        : "OHDungeonGenerate: generation failed, check the BepInEx log.");
                });
            }
        }
    }
}
