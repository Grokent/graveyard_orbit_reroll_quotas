using BepInEx.Configuration;

namespace GraveyardOrbit.Quotas
{
    internal static class QuotaConfig
    {
        public static ConfigEntry<int> Price;

        public static void Init(ConfigFile config)
        {
            Price = config.Bind(
                "Reroll",
                "Price",
                50,
                "Money charged to reroll one incomplete work order. Set 0 for free rerolls.");
        }
    }
}
