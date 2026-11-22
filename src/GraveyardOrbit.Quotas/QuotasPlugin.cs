using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace GraveyardOrbit.Quotas
{
    [BepInPlugin(PluginGuid, "Graveyard Orbit Quotas", PluginVersion)]
    public sealed class QuotasPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "agentbox.graveyardorbit.quotas";
        public const string PluginVersion = "0.1.0";

        internal static ManualLogSource Log;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            QuotaConfig.Init(Config);

            _harmony = new Harmony(PluginGuid);
            try
            {
                _harmony.PatchAll(typeof(QuotasPlugin).Assembly);
                Log.LogInfo($"Quotas loaded: work-order reroll costs ${QuotaConfig.Price.Value}.");
            }
            catch (Exception ex)
            {
                Log.LogError("Quotas patch failure (mod inactive): " + ex);
            }
        }

        private void OnDestroy()
        {
            try { _harmony?.UnpatchSelf(); } catch { }
        }
    }
}
