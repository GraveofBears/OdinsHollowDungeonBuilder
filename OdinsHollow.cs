using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using ItemManager;
using UnityEngine;
using LocationManager;
using PieceManager;
using ServerSync;
using HarmonyLib;
using System.Reflection;

namespace OdinsHollow
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class OdinsHollow : BaseUnityPlugin
	{
		private const string ModName = "OdinsHollow";
		private const string ModVersion = "1.0.2";
		private const string ModGUID = "org.bepinex.plugins.odinshollow";
		private static Harmony harmony = null!;

		private static readonly Dictionary<BuildPiece, ConfigEntry<string>> CreaturesInSpawners = new();

		private readonly ConfigSync configSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

		private static ConfigEntry<bool> ServerConfigLocked = null!;
		private static ConfigEntry<string> OH_Spawner_Shroom_1_Prefab = null!;
		private static ConfigEntry<string> OH_Spawner_Shroom_2_Prefab = null!;
		private static ConfigEntry<string> OH_Spawner_Shroom_3_Prefab = null!;
		private static ConfigEntry<string> OH_Spawner_Shroom_4_Prefab = null!;

		private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
		{
			ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

			SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

			return configEntry;
		}

		private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

		public void Awake()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			harmony = new Harmony(ModGUID);
			harmony.PatchAll(assembly);

			ServerConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");
			configSync.AddLockingConfigEntry(ServerConfigLocked);

			Item OdinsHollowWand = new("odinshollow", "OdinsHollowWand");
			OdinsHollowWand.Crafting.Add(CraftingTable.StoneCutter, 15);
			OdinsHollowWand.RequiredItems.Add("SwordCheat", 1);
			OdinsHollowWand.CraftAmount = 1;

			GameObject OH_Cave_Hall_1 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_Hall_1");
			GameObject OH_Cave_Hall_2 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_Hall_2");
			GameObject OH_Cave_Hall_3 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_Hall_3");
			GameObject OH_Cave_Bridge = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_Bridge");
			GameObject OH_Cave_Room_1 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_Room_1");
			GameObject OH_Cave_Room_2 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_Room_2");
			GameObject OH_Cave_Room_3 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_Room_3");
			GameObject OH_Cave_Room_4 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_Room_4");
			GameObject OH_Cave_End_1 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_End_1");
			GameObject OH_Cave_End_2 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_End_2");
			GameObject OH_Cave_End_3 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_End_3");
			GameObject OH_Cave_End_4 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_End_4");
			GameObject OdinsHollowMush = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OdinsHollowMush");
			GameObject OH_OdinsHollow = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_OdinsHollow");

			BuildPiece OH_Spawner_Shroom_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_1", true, "OdinsHollowWand");
			OH_Spawner_Shroom_1.Name.English("OH Spawner Shroom 1");
			OH_Spawner_Shroom_1.Description.English("A Dungeon Spawner Shroom");
			OH_Spawner_Shroom_1.RequiredItems.Add("SwordCheat", 1, false);

			BuildPiece OH_Spawner_Shroom_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_2", true, "OdinsHollowWand");
			OH_Spawner_Shroom_2.Name.English("OH Spawner Shroom 2");
			OH_Spawner_Shroom_2.Description.English("A Dungeon Spawner Shroom");
			OH_Spawner_Shroom_2.RequiredItems.Add("SwordCheat", 1, false);

			BuildPiece OH_Spawner_Shroom_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_3", true, "OdinsHollowWand");
			OH_Spawner_Shroom_3.Name.English("OH Spawner Shroom 3");
			OH_Spawner_Shroom_3.Description.English("A Dungeon Spawner Shroom");
			OH_Spawner_Shroom_3.RequiredItems.Add("SwordCheat", 1, false);

			BuildPiece OH_Spawner_Shroom_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_4", true, "OdinsHollowWand");
			OH_Spawner_Shroom_4.Name.English("OH Spawner Shroom 4");
			OH_Spawner_Shroom_4.Description.English("A Dungeon Spawner Shroom");
			OH_Spawner_Shroom_4.RequiredItems.Add("SwordCheat", 1, false);

			OH_Spawner_Shroom_1_Prefab = config("2 - Spawner", "Creature for Shroom spawner 1", "Neck", "Prefab name for the creature that should spawn at the first shroom spawner.");
			OH_Spawner_Shroom_1_Prefab.SettingChanged += (_, _) => UpdateSpawner(OH_Spawner_Shroom_1, OH_Spawner_Shroom_1_Prefab);
			OH_Spawner_Shroom_2_Prefab = config("2 - Spawner", "Creature for Shroom spawner 2", "Boar", "Prefab name for the creature that should spawn at the second shroom spawner.");
			OH_Spawner_Shroom_2_Prefab.SettingChanged += (_, _) => UpdateSpawner(OH_Spawner_Shroom_2, OH_Spawner_Shroom_2_Prefab);
			OH_Spawner_Shroom_3_Prefab = config("2 - Spawner", "Creature for Shroom spawner 3", "Greyling", "Prefab name for the creature that should spawn at the third shroom spawner.");
			OH_Spawner_Shroom_3_Prefab.SettingChanged += (_, _) => UpdateSpawner(OH_Spawner_Shroom_3, OH_Spawner_Shroom_3_Prefab);
			OH_Spawner_Shroom_4_Prefab = config("2 - Spawner", "Creature for Shroom spawner 4", "Bat", "Prefab name for the creature that should spawn at the fourth shroom spawner.");
			OH_Spawner_Shroom_4_Prefab.SettingChanged += (_, _) => UpdateSpawner(OH_Spawner_Shroom_4, OH_Spawner_Shroom_4_Prefab);

			CreaturesInSpawners.Add(OH_Spawner_Shroom_1, OH_Spawner_Shroom_1_Prefab);
			CreaturesInSpawners.Add(OH_Spawner_Shroom_2, OH_Spawner_Shroom_2_Prefab);
			CreaturesInSpawners.Add(OH_Spawner_Shroom_3, OH_Spawner_Shroom_3_Prefab);
			CreaturesInSpawners.Add(OH_Spawner_Shroom_4, OH_Spawner_Shroom_4_Prefab);

			_ = new LocationManager.Location("odinshollow", "OdinsHollowDungeon")
			{
				MapIcon = "ohcave.png",
				CanSpawn = true,
				ShowMapIcon = ShowIcon.Explored,
				Biome = Heightmap.Biome.Meadows,
				SpawnDistance = new Range(500, 1500),
				SpawnAltitude = new Range(10, 100),
				MinimumDistanceFromGroup = 100,
				Count = 15,
				Unique = true
			};

		}

		private static void UpdateSpawner(BuildPiece piece, ConfigEntry<string> configEntry)
		{
			if (ZNetScene.instance.GetPrefab(configEntry.Value) is { } character)
			{
				if (Resources.FindObjectsOfTypeAll<CreatureSpawner>().FirstOrDefault(c => piece.Prefab.GetComponentInChildren<CreatureSpawner>().name.StartsWith(c.name, StringComparison.Ordinal)) is { } creatureSpawner)
				{
					creatureSpawner.m_creaturePrefab = character;
				}
				piece.Prefab.transform.GetComponentInChildren<CreatureSpawner>().m_creaturePrefab = character;
			}
		}

		[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
		private class AddCreatureToSpawner
		{
			private static void Postfix(ZNetScene __instance)
			{
				foreach (KeyValuePair<BuildPiece, ConfigEntry<string>> kv in CreaturesInSpawners)
				{
					if (__instance.GetPrefab(kv.Value.Value) is { } character)
					{
						foreach (CreatureSpawner spawner in kv.Key.Prefab.transform.GetComponentsInChildren<CreatureSpawner>())
						{
							spawner.m_creaturePrefab = character;
						}
					}
				}
			}
		}
	}
}
