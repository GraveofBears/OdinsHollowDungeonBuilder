# Dungeon generation

When **Auto-Generate Dungeon** is on, each `OdinsHollowDungeon` location grows a random layout of rooms, halls and ends
from its start connector, then fills it with loot chests and creature spawners. It works like Valheim's own dungeons:

- The layout is saved in the location's ZDO. The first owner generates it once, and every client builds the same rooms
  locally from it. Rooms themselves are not networked.
- Chests and spawners are networked objects. They are spawned once, at generation time, and tagged with the dungeon's
  id so `OHDungeonGenerate` can remove them again.
- Auto-generation skips any dungeon that already has player-built pieces within 80 m of its start, so hand-built
  dungeons in existing worlds are left alone.

## Unity setup

Everything goes in the `odinshollow` asset bundle. Leave `PrefabInstances` as it is; the build pieces keep using it.

1. **Duplicate the room pieces** into `OdinsHollow/DungeonGen` and rename them with an `OH_DG_` prefix, e.g.
   `OH_DG_Cave_Room_1`, `OH_DG_Frost_Cave_Hall_2`, `OH_DG_Ruins_End_1`.
   - The theme comes from the name: `Frost` → frost, `Ruins` → ruins, anything else → cave.
   - A prefab with **one** connector is an end. An end with `Hall_End` in its name is a plug, used only when no
     normal end fits.
   - You can leave `Piece`, `WearNTear` and `ZNetView` on the duplicates; the mod strips them from the rooms.
   - Don't duplicate `OH_OdinsHollow` or the frost door. Any prefab with `OdinsHollow` in its name is ignored.
2. **Check the connectors.** Every child named `Connector…` (but not `ConnectorBox`) is a doorway. Its blue (Z) arrow
   must point **out of** the room, and it should sit on the doorway floor.
   - The connector is at `0,0,0` on `Cave_End_3`, `Frost_Cave_End_4` and `Ruins_End_1`. Check whether that's right.
   - `Cave_Hall_3` and `Frost_Cave_Hall_3` have a 5th connector in the middle of the floor. Delete it in the DG copy.
   - `Ruins_Room_1/2` use the same connector positions as `Cave_Hall_3`. Check that they line up with the doorways.
   - `Hall_End` pieces have no connector. Add one at the opening if you want them used as plugs.
3. **Optional: exact room size.** Add a child named `RoomBounds` with a `BoxCollider` covering the room. Without it, the
   size comes from the room's meshes, which can be generous for caves.
4. **Chest and spawner spots.** Add empty children named `OH_ChestPoint` or `OH_SpawnerPoint` where a chest or spawner
   may appear. The prefab is picked from the config lists. You can also nest a real chest or spawner prefab in a room;
   it's spawned as-is, and the chest/spawner chance still applies.
5. **Dungeon chests and spawners.** Duplicate the chest as `OH_DG_Loot_Chest` and the shroom spawners as
   `OH_DG_Spawner_Shroom_1..4` in `DungeonGen`. If an `OH_DG_` prefab is missing, the matching `OH_` build piece is
   used instead.
6. **Buildable chest.** Name the chest prefab `OH_Loot_Chest`, with `Piece`, `ZNetView` and `Container`. Set the
   Piece name to `$OH_Loot_Chest`.
7. **Start room.** The generator starts from the `Connector` under a `dungeon_start…` object in the
   `OdinsHollowDungeon` location. If there isn't one, it uses the `Connector` under `Entrance`.

## Config

| Section | Setting | Default |
|---|---|---|
| 3 - Dungeon Location | Dungeon Count, Dungeon Biome | 1, Meadows |
| 4 - Dungeon Generation | Auto-Generate Dungeon | off |
| | Min / Max Rooms | 15 / 30 |
| | Use Cave / Frost / Ruins Pieces | on / off / off |
| | Level Connectors, Room Overlap Tolerance | on, 2 m |
| 5 - Dungeon Spawners | Spawner Chance, Spawner Prefabs, Dungeon Creatures | 0.5, OH_DG_Spawner_Shroom_1..4, `Skeleton:3,Greyling:2,Bat:2,Neck:1` |
| | Spawner Respawn Time (minutes, 0 = never) | 120 |
| 6 - Dungeon Chests | Chest Chance, Chest Prefabs, Chest Loot, Chest Rolls Min/Max | 0.5, OH_DG_Loot_Chest, … , 2/4 |
| | Chest Respawn Time (minutes, 0 = never), Only Refill When Empty | 120, on |
| 7 - Buildable Loot Chest | Chest Loot, Chest Rolls Min/Max, Chest Respawn Time | … , 2/4, 120 |

Loot is written as `Item:min:max:weight`, separated by commas. Timers use in-game world time, which only advances while
the world is running. A chest's timer starts the first time something is taken out of it.

Dungeon Count and Dungeon Biome only apply after a restart, and only to parts of the world that haven't been generated
yet. Valheim places locations when zones are first created. On an existing world, the vanilla `genloc` command places
missing locations in unexplored areas.

## Admin command

- `OHDungeonGenerate` regenerates the nearest dungeon with the current config. It removes that dungeon's old generated
  chests, spawners and props first. It works even when Auto-Generate is off, and on dungeons that auto-generation
  skipped.
- `OHDungeonGenerate clear` removes the generated rooms and content and stops that dungeon from auto-generating again.

Stand at the entrance or inside the dungeon when you run it. Players inside should leave first, because the rooms
around them are replaced.
