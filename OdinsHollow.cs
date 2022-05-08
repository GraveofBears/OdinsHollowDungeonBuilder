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
			GameObject OH_Cave_End_1 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_End_1");
			GameObject OH_Cave_End_2 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_End_2");
			GameObject OH_Cave_End_3 = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_Cave_End_3");
			//GameObject OH_OdinsHollow = ItemManager.PrefabManager.RegisterPrefab("odinshollow", "OH_OdinsHollow");

			
			{

				LocationManager.Location location = new("odinshollow", "OdinsHollowDungeon")
				{
					MapIcon = "ohcave.png",
					ShowMapIcon = ShowIcon.Always,
					Biome = Heightmap.Biome.Meadows,
					SpawnDistance = new Range(100, 1500),
					SpawnAltitude = new Range(5, 150),
					MinimumDistanceFromGroup = 100,
					Unique = true,
					Count = 15
					
				};

				location.CreatureSpawner.Add("Spawner_1", "Greydwarf");

			}
		

			

		new Harmony(ModName).PatchAll();


		}
	}

}
