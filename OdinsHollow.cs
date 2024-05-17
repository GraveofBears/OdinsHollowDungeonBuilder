using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using ItemManager;
using CreatureManager;
using UnityEngine;
using LocationManager;
using PieceManager;
using ServerSync;
using HarmonyLib;
using System.Reflection;
using LocalizationManager;

namespace OdinsHollow
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	public class OdinsHollow : BaseUnityPlugin
	{
		private const string ModName = "OdinsHollow";
		private const string ModVersion = "2.0.7";
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

        public static List<GameObject> ObjectsToAddToMaterialReplacer = new();

        public static void AddToMaterialReplacer()
        {
            foreach (var go in ObjectsToAddToMaterialReplacer)
            {
                Transform child = Utils.FindChild(go.transform, "Rocks");
                if (!child) continue;
                MaterialReplacer.RegisterGameObjectForMatSwap(child.gameObject);
            }
        }

        public void Awake()
		{
            Localizer.Load();
            Assembly assembly = Assembly.GetExecutingAssembly();
			harmony = new Harmony(ModGUID);
			harmony.PatchAll(assembly);

			ServerConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");
			configSync.AddLockingConfigEntry(ServerConfigLocked);


            #region Items

            Item OdinsHollowWand = new("odinshollow", "OdinsHollowWand");
            OdinsHollowWand.Crafting.Add(ItemManager.CraftingTable.StoneCutter, 20);
            OdinsHollowWand.RequiredItems.Add("SwordCheat", 1);
            OdinsHollowWand.CraftAmount = 1;

            #endregion



            #region HollowPieces

            BuildPiece OdinsHollowMush = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OdinsHollowMush");
			OdinsHollowMush.RequiredItems.Add("SwordCheat", 1, false);
			OdinsHollowMush.Category.Set("Hollow Pieces");
            OdinsHollowMush.Tool.Add("OdinsHollowWand");

            BuildPiece OH_OdinsHollow = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_OdinsHollow");
			OH_OdinsHollow.RequiredItems.Add("SwordCheat", 1, false);
			OH_OdinsHollow.Category.Set("Hollow Pieces");
			OH_OdinsHollow.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_Hall_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_Hall_1");
			OH_Cave_Hall_1.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_Hall_1.Category.Set("Hollow Pieces");
			OH_Cave_Hall_1.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_Hall_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_Hall_2");
			OH_Cave_Hall_2.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_Hall_2.Category.Set("Hollow Pieces");
            OH_Cave_Hall_2.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_Hall_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_Hall_3");
			OH_Cave_Hall_3.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_Hall_3.Category.Set("Hollow Pieces");
            OH_Cave_Hall_3.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_Hall_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_Hall_4");
			OH_Cave_Hall_4.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_Hall_4.Category.Set("Hollow Pieces");
            OH_Cave_Hall_4.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_Bridge = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_Bridge");
			OH_Cave_Bridge.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_Bridge.Category.Set("Hollow Pieces");
            OH_Cave_Bridge.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_Room_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_Room_1");
			OH_Cave_Room_1.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_Room_1.Category.Set("Hollow Pieces");
            OH_Cave_Room_1.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_Room_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_Room_2");
			OH_Cave_Room_2.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_Room_2.Category.Set("Hollow Pieces");
            OH_Cave_Room_2.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_Room_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_Room_3");
			OH_Cave_Room_3.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_Room_3.Category.Set("Hollow Pieces");
            OH_Cave_Room_3.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_Room_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_Room_4");
			OH_Cave_Room_4.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_Room_4.Category.Set("Hollow Pieces");
            OH_Cave_Room_4.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_End_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_End_1");
			OH_Cave_End_1.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_End_1.Category.Set("Hollow Pieces");
            OH_Cave_End_1.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_End_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_End_2");
			OH_Cave_End_2.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_End_2.Category.Set("Hollow Pieces");
            OH_Cave_End_2.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_End_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_End_3");
			OH_Cave_End_3.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_End_3.Category.Set("Hollow Pieces");
            OH_Cave_End_3.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Cave_End_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Cave_End_4");
			OH_Cave_End_4.RequiredItems.Add("SwordCheat", 1, false);
			OH_Cave_End_4.Category.Set("Hollow Pieces");
            OH_Cave_End_4.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Pillar_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Pillar_1");
			OH_Pillar_1.RequiredItems.Add("SwordCheat", 1, false);
			OH_Pillar_1.Category.Set("Hollow Pieces");
            OH_Pillar_1.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Pillar_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Pillar_2");
			OH_Pillar_2.RequiredItems.Add("SwordCheat", 1, false);
			OH_Pillar_2.Category.Set("Hollow Pieces");
            OH_Pillar_2.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Rock_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Rock_1");
			OH_Rock_1.RequiredItems.Add("SwordCheat", 1, false);
			OH_Rock_1.Category.Set("Hollow Pieces");
            OH_Rock_1.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Rock_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Rock_2");
			OH_Rock_2.RequiredItems.Add("SwordCheat", 1, false);
			OH_Rock_2.Category.Set("Hollow Pieces");
            OH_Rock_2.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Rock_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Rock_3");
			OH_Rock_3.RequiredItems.Add("SwordCheat", 1, false);
			OH_Rock_3.Category.Set("Hollow Pieces");
            OH_Rock_3.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Rock_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Rock_4");
            OH_Rock_4.RequiredItems.Add("SwordCheat", 1, false);
            OH_Rock_4.Category.Set("Hollow Pieces");
            OH_Rock_4.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Stalagmite_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Stalagmite_1");
			OH_Stalagmite_1.RequiredItems.Add("SwordCheat", 1, false);
			OH_Stalagmite_1.Category.Set("Hollow Pieces");
            OH_Stalagmite_1.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Stalagmite_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Stalagmite_2");
			OH_Stalagmite_2.RequiredItems.Add("SwordCheat", 1, false);
			OH_Stalagmite_2.Category.Set("Hollow Pieces");
            OH_Stalagmite_2.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Stalagmite_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Stalagmite_3");
			OH_Stalagmite_3.RequiredItems.Add("SwordCheat", 1, false);
			OH_Stalagmite_3.Category.Set("Hollow Pieces");
            OH_Stalagmite_3.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Wall_Torch = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Wall_Torch");
			OH_Wall_Torch.RequiredItems.Add("SwordCheat", 1, false);
			OH_Wall_Torch.Category.Set("Hollow Pieces");
            OH_Wall_Torch.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Floor_Torch = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Floor_Torch");
			OH_Floor_Torch.RequiredItems.Add("SwordCheat", 1, false);
			OH_Floor_Torch.Category.Set("Hollow Pieces");
            OH_Floor_Torch.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Wall_Patch = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Wall_Patch");
			OH_Wall_Patch.RequiredItems.Add("SwordCheat", 1, false);
			OH_Wall_Patch.Category.Set("Hollow Pieces");
            OH_Wall_Patch.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Floor_Patch = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Floor_Patch");
			OH_Floor_Patch.RequiredItems.Add("SwordCheat", 1, false);
			OH_Floor_Patch.Category.Set("Hollow Pieces");
            OH_Floor_Patch.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Hall_End = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Hall_End");
			OH_Hall_End.RequiredItems.Add("SwordCheat", 1, false);
			OH_Hall_End.Category.Set("Hollow Pieces");
            OH_Hall_End.Tool.Add("OdinsHollowWand");

            #endregion
            #region FrostPieces

            BuildPiece OH_Frost_Cave_Bridge = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_Bridge");
            OH_Frost_Cave_Bridge.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_Bridge.Category.Set("Hollow Pieces");
            OH_Frost_Cave_Bridge.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_Bridge.Prefab);

            BuildPiece OH_Frost_Cave_End_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_End_1");
            OH_Frost_Cave_End_1.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_End_1.Category.Set("Hollow Pieces");
            OH_Frost_Cave_End_1.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_End_1.Prefab);

            BuildPiece OH_Frost_Cave_End_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_End_2");
            OH_Frost_Cave_End_2.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_End_2.Category.Set("Hollow Pieces");
            OH_Frost_Cave_End_2.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_End_2.Prefab);
            //
            BuildPiece OH_Frost_Cave_End_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_End_3");
            OH_Frost_Cave_End_3.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_End_3.Category.Set("Hollow Pieces");
            OH_Frost_Cave_End_3.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_End_3.Prefab);

            BuildPiece OH_Frost_Cave_End_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_End_4");
            OH_Frost_Cave_End_4.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_End_4.Category.Set("Hollow Pieces");
            OH_Frost_Cave_End_4.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_End_4.Prefab);

            BuildPiece OH_Frost_Cave_End_5 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_End_5");
            OH_Frost_Cave_End_5.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_End_5.Category.Set("Hollow Pieces");
            OH_Frost_Cave_End_5.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_End_5.Prefab);

            BuildPiece OH_Frost_Cave_Hall_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_Hall_1");
            OH_Frost_Cave_Hall_1.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_Hall_1.Category.Set("Hollow Pieces");
            OH_Frost_Cave_Hall_1.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_End_5.Prefab);

            BuildPiece OH_Frost_Cave_Hall_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_Hall_2");
            OH_Frost_Cave_Hall_2.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_Hall_2.Category.Set("Hollow Pieces");
            OH_Frost_Cave_Hall_2.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_Hall_2.Prefab);

            BuildPiece OH_Frost_Cave_Hall_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_Hall_3");
            OH_Frost_Cave_Hall_3.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_Hall_3.Category.Set("Hollow Pieces");
            OH_Frost_Cave_Hall_3.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_Hall_3.Prefab);

            BuildPiece OH_Frost_Cave_Hall_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_Hall_4");
            OH_Frost_Cave_Hall_4.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_Hall_4.Category.Set("Hollow Pieces");
            OH_Frost_Cave_Hall_4.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_Hall_4.Prefab);

            BuildPiece OH_Frost_Cave_Room_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_Room_1");
            OH_Frost_Cave_Room_1.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_Room_1.Category.Set("Hollow Pieces");
            OH_Frost_Cave_Room_1.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_Room_1.Prefab);

            BuildPiece OH_Frost_Cave_Room_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_Room_2");
            OH_Frost_Cave_Room_2.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_Room_2.Category.Set("Hollow Pieces");
            OH_Frost_Cave_Room_2.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_Room_2.Prefab);

            BuildPiece OH_Frost_Cave_Room_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_Room_3");
            OH_Frost_Cave_Room_3.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_Room_3.Category.Set("Hollow Pieces");
            OH_Frost_Cave_Room_3.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_Room_3.Prefab);

            BuildPiece OH_Frost_Cave_Room_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Cave_Room_4");
            OH_Frost_Cave_Room_4.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Cave_Room_4.Category.Set("Hollow Pieces");
            OH_Frost_Cave_Room_4.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Cave_Room_4.Prefab);

            BuildPiece OH_Frost_Hall_End = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Hall_End");
            OH_Frost_Hall_End.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Hall_End.Category.Set("Hollow Pieces");
            OH_Frost_Hall_End.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Hall_End.Prefab);

            BuildPiece OH_Frost_Rock_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Rock_1");
            OH_Frost_Rock_1.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Rock_1.Category.Set("Hollow Pieces");
            OH_Frost_Rock_1.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Frost_Rock_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Rock_2");
            OH_Frost_Rock_2.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Rock_2.Category.Set("Hollow Pieces");
            OH_Frost_Rock_2.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Rock_2.Prefab);

            BuildPiece OH_Frost_Rock_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Rock_3");
            OH_Frost_Rock_3.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Rock_3.Category.Set("Hollow Pieces");
            OH_Frost_Rock_3.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Frost_Rock_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Rock_4");
            OH_Frost_Rock_4.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Rock_4.Category.Set("Hollow Pieces");
            OH_Frost_Rock_4.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Rock_4.Prefab);

            BuildPiece OH_Frost_Stalagmite_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Stalagmite_1");
            OH_Frost_Stalagmite_1.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Stalagmite_1.Category.Set("Hollow Pieces");
            OH_Frost_Stalagmite_1.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Frost_Stalagmite_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Stalagmite_2");
            OH_Frost_Stalagmite_2.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Stalagmite_2.Category.Set("Hollow Pieces");
            OH_Frost_Stalagmite_2.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Frost_Stalagmite_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Stalagmite_3");
            OH_Frost_Stalagmite_3.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Stalagmite_3.Category.Set("Hollow Pieces");
            OH_Frost_Stalagmite_3.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Frost_Wall_Patch = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Frost_Wall_Patch");
            OH_Frost_Wall_Patch.RequiredItems.Add("SwordCheat", 1, false);
            OH_Frost_Wall_Patch.Category.Set("Hollow Pieces");
            OH_Frost_Wall_Patch.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_Frost_Wall_Patch.Prefab);

            BuildPiece OH_OdinsHollow_Frost_Door = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_OdinsHollow_Frost_Door");
            OH_OdinsHollow_Frost_Door.RequiredItems.Add("SwordCheat", 1, false);
            OH_OdinsHollow_Frost_Door.Category.Set("Hollow Pieces");
            OH_OdinsHollow_Frost_Door.Tool.Add("OdinsHollowWand");
            ObjectsToAddToMaterialReplacer.Add(OH_OdinsHollow_Frost_Door.Prefab);

            AddToMaterialReplacer();

            #endregion
            #region shroomsspawners

            BuildPiece OH_Spawner_Shroom_1 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_1");
			OH_Spawner_Shroom_1.RequiredItems.Add("SwordCheat", 1, false);
			OH_Spawner_Shroom_1.Category.Set("Hollow Pieces");
            OH_Spawner_Shroom_1.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Spawner_Shroom_2 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_2");
			OH_Spawner_Shroom_2.RequiredItems.Add("SwordCheat", 1, false);
			OH_Spawner_Shroom_2.Category.Set("Hollow Pieces");
            OH_Spawner_Shroom_2.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Spawner_Shroom_3 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_3");
			OH_Spawner_Shroom_3.RequiredItems.Add("SwordCheat", 1, false);
			OH_Spawner_Shroom_3.Category.Set("Hollow Pieces");
            OH_Spawner_Shroom_3.Tool.Add("OdinsHollowWand");

            BuildPiece OH_Spawner_Shroom_4 = new(PiecePrefabManager.RegisterAssetBundle("odinshollow"), "OH_Spawner_Shroom_4");
			OH_Spawner_Shroom_4.RequiredItems.Add("SwordCheat", 1, false);
			OH_Spawner_Shroom_4.Category.Set("Hollow Pieces");
            OH_Spawner_Shroom_4.Tool.Add("OdinsHollowWand");

            OH_Spawner_Shroom_1_Prefab = config("2 - Spawner", "Creature for Shroom spawner 1", "Neck", "Prefab name for the creature that should spawn at the first shroom spawner.");
			OH_Spawner_Shroom_1_Prefab.SettingChanged += (_, _) => UpdateSpawner(OH_Spawner_Shroom_1, OH_Spawner_Shroom_1_Prefab);
			OH_Spawner_Shroom_2_Prefab = config("2 - Spawner", "Creature for Shroom spawner 2", "Skeleton", "Prefab name for the creature that should spawn at the second shroom spawner.");
			OH_Spawner_Shroom_2_Prefab.SettingChanged += (_, _) => UpdateSpawner(OH_Spawner_Shroom_2, OH_Spawner_Shroom_2_Prefab);
			OH_Spawner_Shroom_3_Prefab = config("2 - Spawner", "Creature for Shroom spawner 3", "Greyling", "Prefab name for the creature that should spawn at the third shroom spawner.");
			OH_Spawner_Shroom_3_Prefab.SettingChanged += (_, _) => UpdateSpawner(OH_Spawner_Shroom_3, OH_Spawner_Shroom_3_Prefab);
			OH_Spawner_Shroom_4_Prefab = config("2 - Spawner", "Creature for Shroom spawner 4", "Bat", "Prefab name for the creature that should spawn at the fourth shroom spawner.");
			OH_Spawner_Shroom_4_Prefab.SettingChanged += (_, _) => UpdateSpawner(OH_Spawner_Shroom_4, OH_Spawner_Shroom_4_Prefab);

			CreaturesInSpawners.Add(OH_Spawner_Shroom_1, OH_Spawner_Shroom_1_Prefab);
			CreaturesInSpawners.Add(OH_Spawner_Shroom_2, OH_Spawner_Shroom_2_Prefab);
			CreaturesInSpawners.Add(OH_Spawner_Shroom_3, OH_Spawner_Shroom_3_Prefab);
			CreaturesInSpawners.Add(OH_Spawner_Shroom_4, OH_Spawner_Shroom_4_Prefab);

            #endregion

            #region Location

            _ = new LocationManager.Location("odinshollow", "OdinsHollowDungeon")
			{
				MapIcon = "ohcave.png",
				CanSpawn = true,
				ShowMapIcon = ShowIcon.Explored,
				Biome = Heightmap.Biome.Meadows,
				SpawnDistance = new LocationManager.Range(500, 1500),
				SpawnAltitude = new LocationManager.Range(10, 100),
				MinimumDistanceFromGroup = 100,
				Count = 15,
				Unique = true
			};

		}
        #endregion
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
