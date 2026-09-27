using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace OdinsHollow.DungeonGen
{
    internal static class LootTables
    {
        // Parses "Item:min:max:weight,..." into a vanilla DropTable. Unknown items are skipped with a warning.
        internal static DropTable BuildDropTable(string definition, int rollsMin, int rollsMax)
        {
            DropTable table = new()
            {
                m_dropMin = Math.Max(0, Math.Min(rollsMin, rollsMax)),
                m_dropMax = Math.Max(0, Math.Max(rollsMin, rollsMax)),
                m_dropChance = 1f,
                m_oneOfEach = false,
                m_drops = new List<DropTable.DropData>()
            };

            foreach (string entry in Split(definition))
            {
                string[] parts = entry.Split(':');
                string name = parts[0].Trim();
                GameObject? item = ObjectDB.instance ? ObjectDB.instance.GetItemPrefab(name) : null;
                if (item == null)
                {
                    Debug.LogWarning($"[OdinsHollow] Loot table item '{name}' not found, skipping.");
                    continue;
                }

                int min = parts.Length > 1 ? ParseInt(parts[1], 1) : 1;
                int max = parts.Length > 2 ? ParseInt(parts[2], min) : min;
                float weight = parts.Length > 3 ? ParseFloat(parts[3], 1f) : 1f;
                table.m_drops.Add(new DropTable.DropData
                {
                    m_item = item,
                    m_stackMin = Math.Max(1, Math.Min(min, max)),
                    m_stackMax = Math.Max(1, Math.Max(min, max)),
                    m_weight = Math.Max(0.01f, weight),
                    m_dontScale = false
                });
            }

            return table;
        }

        // Parses "Name:weight,Name,..." into (name, weight) pairs. A missing weight counts as 1.
        internal static List<KeyValuePair<string, float>> ParseWeighted(string definition)
        {
            List<KeyValuePair<string, float>> result = new();
            foreach (string entry in Split(definition))
            {
                string[] parts = entry.Split(':');
                float weight = parts.Length > 1 ? ParseFloat(parts[1], 1f) : 1f;
                if (weight > 0f)
                {
                    result.Add(new KeyValuePair<string, float>(parts[0].Trim(), weight));
                }
            }

            return result;
        }

        internal static string? PickWeighted(List<KeyValuePair<string, float>> options, System.Random rng)
        {
            float total = 0f;
            foreach (KeyValuePair<string, float> option in options) total += option.Value;
            if (total <= 0f) return null;

            double roll = rng.NextDouble() * total;
            foreach (KeyValuePair<string, float> option in options)
            {
                roll -= option.Value;
                if (roll <= 0) return option.Key;
            }

            return options[options.Count - 1].Key;
        }

        // Finds a prefab in ZNetScene; an OH_DG_ name falls back to the OH_ build piece with the same name.
        internal static GameObject? FindNetworkedPrefab(string name)
        {
            if (ZNetScene.instance == null || string.IsNullOrEmpty(name)) return null;
            GameObject? prefab = ZNetScene.instance.GetPrefab(name);
            if (prefab == null && name.StartsWith("OH_DG_", StringComparison.Ordinal))
            {
                prefab = ZNetScene.instance.GetPrefab("OH_" + name.Substring("OH_DG_".Length));
            }

            return prefab;
        }

        private static IEnumerable<string> Split(string definition)
        {
            foreach (string entry in definition.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = entry.Trim();
                if (trimmed.Length > 0) yield return trimmed;
            }
        }

        private static int ParseInt(string s, int fallback) => int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : fallback;

        private static float ParseFloat(string s, float fallback) => float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : fallback;
    }
}
