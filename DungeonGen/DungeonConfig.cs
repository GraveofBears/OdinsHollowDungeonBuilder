using BepInEx.Configuration;

namespace OdinsHollow.DungeonGen
{
    internal static class DungeonConfig
    {
        // Prefab name of the buildable loot chest in the asset bundle.
        internal const string BuildableChestPrefab = "OH_Loot_Chest";

        // Location settings
        internal static ConfigEntry<int> DungeonCount = null!;
        internal static ConfigEntry<Heightmap.Biome> DungeonBiome = null!;

        // Generation
        internal static ConfigEntry<bool> AutoGenerate = null!;
        internal static ConfigEntry<int> MinRooms = null!;
        internal static ConfigEntry<int> MaxRooms = null!;
        internal static ConfigEntry<bool> UseCavePieces = null!;
        internal static ConfigEntry<bool> UseFrostPieces = null!;
        internal static ConfigEntry<bool> UseRuinsPieces = null!;
        internal static ConfigEntry<bool> LevelConnectors = null!;
        internal static ConfigEntry<float> OverlapTolerance = null!;

        // Dungeon spawners
        internal static ConfigEntry<float> SpawnerChance = null!;
        internal static ConfigEntry<string> SpawnerPrefabs = null!;
        internal static ConfigEntry<string> DungeonCreatures = null!;
        internal static ConfigEntry<float> SpawnerRespawnMinutes = null!;

        // Dungeon chests
        internal static ConfigEntry<float> ChestChance = null!;
        internal static ConfigEntry<string> ChestPrefabs = null!;
        internal static ConfigEntry<string> DungeonChestLoot = null!;
        internal static ConfigEntry<int> DungeonChestRollsMin = null!;
        internal static ConfigEntry<int> DungeonChestRollsMax = null!;
        internal static ConfigEntry<float> DungeonChestRespawnMinutes = null!;

        // Buildable chest
        internal static ConfigEntry<string> BuildableChestLoot = null!;
        internal static ConfigEntry<int> BuildableChestRollsMin = null!;
        internal static ConfigEntry<int> BuildableChestRollsMax = null!;
        internal static ConfigEntry<float> BuildableChestRespawnMinutes = null!;

        internal static ConfigEntry<bool> OnlyRefillWhenEmpty = null!;

        internal static void Bind(OdinsHollow plugin)
        {
            const string location = "3 - Dungeon Location";
            DungeonCount = plugin.config(location, "Dungeon Count", 1, new ConfigDescription("How many Odins Hollow dungeons are placed in a world. 0 disables placement. Only affects areas of the world that haven't been generated yet; requires a restart.", new AcceptableValueRange<int>(0, 100)));
            DungeonBiome = plugin.config(location, "Dungeon Biome", Heightmap.Biome.Meadows, "Biome(s) the dungeon can be placed in. Several can be combined, e.g. \"Meadows, BlackForest\". Only affects areas of the world that haven't been generated yet; requires a restart.");

            const string generation = "4 - Dungeon Generation";
            AutoGenerate = plugin.config(generation, "Auto-Generate Dungeon", false, "If on, each Odins Hollow dungeon generates a random layout of DungeonGen rooms, halls and ends from its start room, with loot chests and spawners. Dungeons someone has already built in are left alone.");
            MinRooms = plugin.config(generation, "Min Rooms", 15, new ConfigDescription("Minimum number of rooms and halls (not counting ends) a generated dungeon aims for.", new AcceptableValueRange<int>(1, 200)));
            MaxRooms = plugin.config(generation, "Max Rooms", 30, new ConfigDescription("Maximum number of rooms and halls (not counting ends) in a generated dungeon.", new AcceptableValueRange<int>(1, 200)));
            UseCavePieces = plugin.config(generation, "Use Cave Pieces", true, "Use the cave rooms, halls and ends.");
            UseFrostPieces = plugin.config(generation, "Use Frost Pieces", false, "Use the frost cave rooms, halls and ends.");
            UseRuinsPieces = plugin.config(generation, "Use Ruins Pieces", false, "Use the ruins rooms, halls and ends.");
            LevelConnectors = plugin.config(generation, "Level Connectors", true, "Ignore the tilt of room connectors so every room stays level. Turn off if a room is meant to attach at an angle.");
            OverlapTolerance = plugin.config(generation, "Room Overlap Tolerance", 2f, new ConfigDescription("How far (in meters) rooms may overlap each other. Raise it if the dungeon comes out too small, lower it if rooms clip into each other.", new AcceptableValueRange<float>(0f, 20f)));

            const string spawners = "5 - Dungeon Spawners";
            SpawnerChance = plugin.config(spawners, "Spawner Chance", 0.5f, new ConfigDescription("Chance (0-1) for each spawner spot in a generated dungeon to get a spawner.", new AcceptableValueRange<float>(0f, 1f)));
            SpawnerPrefabs = plugin.config(spawners, "Spawner Prefabs", "OH_DG_Spawner_Shroom_1,OH_DG_Spawner_Shroom_2,OH_DG_Spawner_Shroom_3,OH_DG_Spawner_Shroom_4", "Spawner prefabs picked at random for spawner spots. Falls back to the OH_ build piece if an OH_DG_ prefab doesn't exist.");
            DungeonCreatures = plugin.config(spawners, "Dungeon Creatures", "Skeleton:3,Greyling:2,Bat:2,Neck:1", "Creatures for generated dungeon spawners, as Prefab:weight. Higher weight is more common.");
            SpawnerRespawnMinutes = plugin.config(spawners, "Spawner Respawn Time", 120f, new ConfigDescription("Minutes of in-game time after a dungeon spawner's creature dies before it spawns a new one. 0 = never respawn.", new AcceptableValueRange<float>(0f, 10080f)));

            const string chests = "6 - Dungeon Chests";
            ChestChance = plugin.config(chests, "Chest Chance", 0.5f, new ConfigDescription("Chance (0-1) for each chest spot in a generated dungeon to get a loot chest.", new AcceptableValueRange<float>(0f, 1f)));
            ChestPrefabs = plugin.config(chests, "Chest Prefabs", "OH_DG_Loot_Chest", "Chest prefabs picked at random for chest spots. Falls back to the OH_ build piece if an OH_DG_ prefab doesn't exist.");
            DungeonChestLoot = plugin.config(chests, "Chest Loot", "Coins:20:60:10,Amber:1:3:4,AmberPearl:1:2:2,Ruby:1:2:2,SilverNecklace:1:1:1", "Loot table for generated dungeon chests, as Item:min:max:weight separated by commas.");
            DungeonChestRollsMin = plugin.config(chests, "Chest Rolls Min", 2, new ConfigDescription("Minimum number of loot table rolls per dungeon chest.", new AcceptableValueRange<int>(0, 32)));
            DungeonChestRollsMax = plugin.config(chests, "Chest Rolls Max", 4, new ConfigDescription("Maximum number of loot table rolls per dungeon chest.", new AcceptableValueRange<int>(0, 32)));
            DungeonChestRespawnMinutes = plugin.config(chests, "Chest Respawn Time", 120f, new ConfigDescription("Minutes of in-game time after a dungeon chest is looted before it refills. 0 = never refill.", new AcceptableValueRange<float>(0f, 10080f)));
            OnlyRefillWhenEmpty = plugin.config(chests, "Only Refill When Empty", true, "If on, a looted chest only refills once it's completely empty, so items players put in it are never replaced. If off, the chest's contents are replaced with fresh loot when the timer runs out. Applies to dungeon and buildable chests.");

            const string buildable = "7 - Buildable Loot Chest";
            BuildableChestLoot = plugin.config(buildable, "Chest Loot", "Coins:20:60:10,Amber:1:3:4,AmberPearl:1:2:2,Ruby:1:2:2", "Loot table for the buildable loot chest, as Item:min:max:weight separated by commas.");
            BuildableChestRollsMin = plugin.config(buildable, "Chest Rolls Min", 2, new ConfigDescription("Minimum number of loot table rolls per buildable chest.", new AcceptableValueRange<int>(0, 32)));
            BuildableChestRollsMax = plugin.config(buildable, "Chest Rolls Max", 4, new ConfigDescription("Maximum number of loot table rolls per buildable chest.", new AcceptableValueRange<int>(0, 32)));
            BuildableChestRespawnMinutes = plugin.config(buildable, "Chest Respawn Time", 120f, new ConfigDescription("Minutes of in-game time after a buildable loot chest is looted before it refills. 0 = never refill.", new AcceptableValueRange<float>(0f, 10080f)));
        }
    }
}
