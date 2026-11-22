using System;
using GraveyardOrbit.UI;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraveyardOrbit.Quotas
{
    /// <summary>
    /// Turns the vanity CLAIM key into a REROLL key while the order is still
    /// incomplete. Once the target is met, the vanilla CLAIM key is left alone,
    /// so both controls share exactly the same slot.
    /// </summary>
    [HarmonyPatch(typeof(QuotasScreen), "MakeCard")]
    internal static class QuotaCardRerollPatch
    {
        private static readonly Color RerollOrange = new Color(1f, 0.62f, 0.18f);
        private static readonly Color RerollDim = new Color(1f, 0.62f, 0.18f, 0.32f);
        private static readonly Color Ink = new Color(0.03f, 0.05f, 0.035f);

        private static void Postfix(QuotasScreen __instance, QuotaState q, JObject inst,
            VisualElement __result)
        {
            try
            {
                if (__instance == null || q == null || inst == null || __result == null) return;

                QuotaDef def = q.Def(inst);
                if (!QuotaReroll.CanReroll(q, inst, def)) return;

                VisualElement key = __result.Q<VisualElement>(className: "q-slabkey");
                Button btn = key?.Q<Button>();
                if (btn == null) return;

                int price = QuotaReroll.Price;
                bool affordable = QuotaReroll.CanAfford();
                bool replaceable = QuotaReroll.HasReplacement(q, def);
                bool usable = affordable && replaceable;

                // Vanilla hides this key until the order is complete. We take
                // the slot over for rerolling instead.
                key.style.visibility = Visibility.Visible;
                btn.text = price > 0
                    ? "REROLL $" + price.ToString("N0", Loc.Culture)
                    : "REROLL FREE";
                btn.SetEnabled(usable);
                btn.style.color = usable ? RerollOrange : RerollDim;
                btn.tooltip = !replaceable
                    ? "No other work order is available."
                    : (affordable
                        ? (price > 0 ? $"Reroll this work order for ${price}." : "Reroll this work order for free.")
                        : $"Need ${price} to reroll this work order.");

                SlabFrame frame = key.Q<SlabFrame>();
                Color restStroke = RerollOrange;
                Color restFill = frame != null ? frame.FillColor : Color.black;
                Color restTint = frame != null ? frame.TintColor : Color.clear;
                if (frame != null)
                {
                    frame.StrokeColor = usable ? RerollOrange : RerollDim;
                    frame.TintColor = new Color(RerollOrange.r, RerollOrange.g, RerollOrange.b, 0.10f);
                }

                if (usable)
                {
                    btn.clicked += delegate
                    {
                        // Rebuilding the screen during the click event can be
                        // awkward; let the event finish first.
                        __result.schedule.Execute(new Action(delegate
                        {
                            QuotaReroll.TryReroll(__instance, q, inst);
                        }));
                    };
                    btn.RegisterCallback<ClickEvent>(delegate(ClickEvent e)
                    {
                        e.StopPropagation();
                    });
                    key.RegisterCallback<PointerEnterEvent>(delegate
                    {
                        if (!btn.enabledSelf) return;
                        btn.style.color = Ink;
                        if (frame != null)
                        {
                            frame.StrokeColor = Color.Lerp(RerollOrange, Color.white, 0.5f);
                            frame.FillColor = RerollOrange;
                            frame.TintColor = Color.clear;
                        }
                    });
                    key.RegisterCallback<PointerLeaveEvent>(delegate
                    {
                        btn.style.color = RerollOrange;
                        if (frame != null)
                        {
                            frame.StrokeColor = restStroke;
                            frame.FillColor = restFill;
                            frame.TintColor = restTint;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                QuotasPlugin.Log.LogWarning("[reroll] card patch failed: " + ex);
            }
        }
    }
}
