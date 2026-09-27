using UnityEngine;

namespace OdinsHollow.DungeonGen
{
    // Fills a chest from a loot table and refills it after a configurable time once it has been looted.
    // Added to buildable loot chests and to every chest spawned in a generated dungeon (see DungeonPatches).
    public class OHLootChest : MonoBehaviour
    {
        private const string FilledKey = "OH_LootFilled";
        private const string FilledCountKey = "OH_LootFilledCount";
        private const string NextRefillKey = "OH_LootNextRefill";

        private ZNetView m_nview = null!;
        private Container m_container = null!;

        private void Awake()
        {
            m_nview = GetComponentInParent<ZNetView>();
            m_container = GetComponent<Container>();
            // Build ghosts have no ZDO.
            if (m_nview == null || m_container == null || m_nview.GetZDO() == null)
            {
                enabled = false;
                return;
            }

            InvokeRepeating(nameof(UpdateLoot), Random.Range(1f, 3f), 5f);
        }

        private bool IsDungeonChest => m_nview.GetZDO().GetInt(DungeonManager.DungeonIdKey) != 0;

        private float RespawnMinutes => IsDungeonChest ? DungeonConfig.DungeonChestRespawnMinutes.Value : DungeonConfig.BuildableChestRespawnMinutes.Value;

        private void UpdateLoot()
        {
            if (!m_nview.IsValid() || !m_nview.IsOwner() || ZNet.instance == null) return;

            ZDO zdo = m_nview.GetZDO();
            Inventory inventory = m_container.GetInventory();
            if (inventory == null) return;

            if (!zdo.GetBool(FilledKey))
            {
                Fill(zdo, inventory);
                return;
            }

            int count = CountItems(inventory);
            long nextRefill = zdo.GetLong(NextRefillKey);
            if (nextRefill == 0)
            {
                // The timer starts the first time something is taken out.
                float minutes = RespawnMinutes;
                if (minutes > 0f && count < zdo.GetInt(FilledCountKey))
                {
                    zdo.Set(NextRefillKey, ZNet.instance.GetTime().AddMinutes(minutes).Ticks);
                }

                return;
            }

            if (ZNet.instance.GetTime().Ticks < nextRefill) return;
            if (DungeonConfig.OnlyRefillWhenEmpty.Value && count > 0) return;
            // Don't swap the contents while someone is looking.
            if (m_container.IsInUse() || Player.IsPlayerInRange(transform.position, 4f)) return;

            Fill(zdo, inventory);
        }

        private void Fill(ZDO zdo, Inventory inventory)
        {
            DropTable table = IsDungeonChest
                ? LootTables.BuildDropTable(DungeonConfig.DungeonChestLoot.Value, DungeonConfig.DungeonChestRollsMin.Value, DungeonConfig.DungeonChestRollsMax.Value)
                : LootTables.BuildDropTable(DungeonConfig.BuildableChestLoot.Value, DungeonConfig.BuildableChestRollsMin.Value, DungeonConfig.BuildableChestRollsMax.Value);

            inventory.RemoveAll();
            if (table.m_drops.Count > 0)
            {
                foreach (ItemDrop.ItemData item in table.GetDropListItems())
                {
                    inventory.AddItem(item);
                }
            }

            zdo.Set(FilledKey, true);
            zdo.Set(FilledCountKey, CountItems(inventory));
            zdo.Set(NextRefillKey, 0L);
        }

        private static int CountItems(Inventory inventory)
        {
            int count = 0;
            foreach (ItemDrop.ItemData item in inventory.GetAllItems()) count += item.m_stack;
            return count;
        }
    }
}
