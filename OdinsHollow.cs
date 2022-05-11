﻿using BepInEx;
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
		private const string ModVersion = "1.0.4";
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

			BuildPiece OH_Spawner_Shroom = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom", true, "OdinsHollowWand");
			OH_Spawner_Shroom.Name.English("OH Spawner Shroom");
			OH_Spawner_Shroom.Description.English("A Spawner Shroom");
			OH_Spawner_Shroom.RequiredItems.Add("SwordCheat", 1, false);


			{
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

		new Harmony(ModName).PatchAll();


		}
	}

}
