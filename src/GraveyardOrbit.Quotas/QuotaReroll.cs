using System;
using System.Collections.Generic;
using GraveyardOrbit.Audio;
using GraveyardOrbit.UI;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GraveyardOrbit.Quotas
{
    /// <summary>
    /// Replaces one active work order with a new random order of the same kind.
    /// Family orders never reroll into their current weapon family.
    /// </summary>
    internal static class QuotaReroll
    {
        internal enum Category
        {
            None,
            General,
            Family,
            Purchase,
            Cross
        }

        internal static Category CategoryOf(QuotaDef def)
        {
            if (def == null) return Category.None;
            if (def.IsFamily) return Category.Family;
            if (def.IsGeneral) return Category.General;
            if (def.IsPurchase) return Category.Purchase;
            if (def.IsCross) return Category.Cross;
            return Category.None; // special-material orders are not part of the normal draw
        }

        internal static bool CanReroll(QuotaState state, JObject inst, QuotaDef def)
        {
            return state != null && inst != null && def != null &&
                   !state.IsClaimed(inst) && !state.IsComplete(inst);
        }

        internal static int Price => Mathf.Max(0, QuotaConfig.Price.Value);

        internal static bool CanAfford()
        {
            GameState gs = GameState.I;
            return gs != null && (Price <= 0 || gs.Money >= Price);
        }

        internal static bool HasReplacement(QuotaState state, QuotaDef current)
        {
            return BuildPool(state, current).Count > 0;
        }

        internal static QuotaDef PickReplacement(QuotaState state, QuotaDef current)
        {
            List<QuotaDef> pool = BuildPool(state, current);
            if (pool.Count == 0) return null;
            return pool[UnityEngine.Random.Range(0, pool.Count)];
        }

        private static List<QuotaDef> BuildPool(QuotaState state, QuotaDef current)
        {
            var pool = new List<QuotaDef>();
            if (state == null || current == null) return pool;

            Category category = CategoryOf(current);
            if (category == Category.None) return pool;

            bool demoLocks = GameData.DemoLocksDrones || state.SetIndex <= 2;
            foreach (QuotaDef candidate in QuotaCatalog.All)
            {
                if (candidate == null || candidate.id == current.id) continue;
                if (CategoryOf(candidate) != category) continue;
                if (demoLocks && QuotaCatalog.NeedsDrones(candidate)) continue;

                if (category == Category.Family)
                {
                    // The whole point of the reroll: never return the same
                    // weapon type again.
                    if (candidate.family == current.family) continue;
                    if (GameData.IsDemoLockedBranch(candidate.family)) continue;
                }

                pool.Add(candidate);
            }
            return pool;
        }

        internal static bool TryReroll(QuotasScreen screen, QuotaState state, JObject inst)
        {
            try
            {
                QuotaDef current = state?.Def(inst);
                if (!CanReroll(state, inst, current)) return false;

                int price = Price;
                GameState gs = GameState.I;
                if (gs == null || (price > 0 && gs.Money < price)) return false;

                QuotaDef next = PickReplacement(state, current);
                if (next == null) return false;

                if (price > 0) gs.Money -= price;
                inst["id"] = next.id;
                inst["progress"] = 0;
                inst["claimed"] = false;
                state.Save();

                RunLog.Event("quota_reroll", -price,
                    RunLog.D("from", current.id, "to", next.id, "set", state.SetIndex));
                QuotasPlugin.Log.LogInfo(
                    $"[reroll] {current.id} -> {next.id} for ${price} (money {gs.Money})");

                try { Sfx.Play("buy"); } catch { }
                try { screen?.DevRefresh(); } catch { }
                return true;
            }
            catch (Exception ex)
            {
                QuotasPlugin.Log.LogWarning("[reroll] failed: " + ex);
                return false;
            }
        }
    }
}
