using System.Collections.Generic;
using BepInEx;
using ItemManager;
using UnityEngine;
using LocationManager;
using PieceManager;
using ServerSync;
using HarmonyLib;

namespace OdinsHollow
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class OdinsHollow : BaseUnityPlugin
	{
		private const string ModName = "OdinsHollow";
		private const string ModVersion = "1.0.2";
		private const string ModGUID = "org.bepinex.plugins.odinshollow";


        public void Awake()
		{


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
			OH_Spawner_Shroom_1.Description.English("A Dungeon Spawner1 Shroom");
			OH_Spawner_Shroom_1.RequiredItems.Add("SwordCheat", 1, false);

			BuildPiece OH_Spawner_Shroom_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_2", true, "OdinsHollowWand");
			OH_Spawner_Shroom_2.Name.English("OH Spawner Shroom 2");
			OH_Spawner_Shroom_2.Description.English("A Dungeon Spawner2 Shroom");
			OH_Spawner_Shroom_2.RequiredItems.Add("SwordCheat", 1, false);

			BuildPiece OH_Spawner_Shroom_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_3", true, "OdinsHollowWand");
			OH_Spawner_Shroom_3.Name.English("OH Spawner Shroom 3");
			OH_Spawner_Shroom_3.Description.English("A Dungeon Spawner3 Shroom");
			OH_Spawner_Shroom_3.RequiredItems.Add("SwordCheat", 1, false);

			BuildPiece OH_Spawner_Shroom_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_4", true, "OdinsHollowWand");
			OH_Spawner_Shroom_4.Name.English("OH Spawner Shroom 4");
			OH_Spawner_Shroom_4.Description.English("A Dungeon Spawner4 Shroom");
			OH_Spawner_Shroom_4.RequiredItems.Add("SwordCheat", 1, false);

			BuildPiece OH_Spawner_Shroom_5 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_5", true, "OdinsHollowWand");
			OH_Spawner_Shroom_5.Name.English("OH Spawner Shroom 5");
			OH_Spawner_Shroom_5.Description.English("A Dungeon Spawner5 Shroom");
			OH_Spawner_Shroom_5.RequiredItems.Add("SwordCheat", 1, false);

			CreaturesInSpawners.Add(OH_Spawner_Shroom_1, "Boar");
			CreaturesInSpawners.Add(OH_Spawner_Shroom_2, "Boar");
			CreaturesInSpawners.Add(OH_Spawner_Shroom_3, "Greyling");
			CreaturesInSpawners.Add(OH_Spawner_Shroom_4, "Bat");
			CreaturesInSpawners.Add(OH_Spawner_Shroom_5, "Skeleton");



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

			new Harmony(ModName).PatchAll();
		}

		private static readonly Dictionary<BuildPiece, string> CreaturesInSpawners = new();

		[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
		private class AddCreatureToSpawner
		{
			private static void Postfix(ZNetScene __instance)
			{
				foreach (KeyValuePair<BuildPiece, string> kv in CreaturesInSpawners)
				{
					foreach (CreatureSpawner spawner in kv.Key.Prefab.transform.GetComponentsInChildren<CreatureSpawner>())
					{
						spawner.m_creaturePrefab = __instance.GetPrefab(kv.Value);
					}
				}
			}
		}
	}
}
