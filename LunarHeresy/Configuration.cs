using BepInEx.Configuration;
using R2API;

namespace LunarHeresy
{
    class Configuration
    {
        public static ConfigEntry<float> DropChance;
        public static ConfigEntry<float> DropMulti;
        public static ConfigEntry<float> DropMin;

        public static ConfigEntry<float> NewtShrineCost;
        public static ConfigEntry<float> PodCost;
        public static ConfigEntry<float> ShopCost;
        public static ConfigEntry<bool> ShopRefresh;
        public static ConfigEntry<float> SeerCost;

        public static void Register(LunarHeresy instance)
        {
            DropChance = instance.Config.Bind(
                "Coin Drop Rate", 
                "Drop Chance", 
                1.5f, 
                new ConfigDescription("The initial %chance for enemies to drop coins. Vanilla is 0.5%")
                );
            DropMulti = instance.Config.Bind(
                "Coin Drop Rate", 
                "Drop Multiplier", 
                0.90f, 
                new ConfigDescription("The multiplier applied to the drop chance after a coin has dropped. Vanilla is 0.5 (chance reduced by 50% after each drop)")
                );
            DropMin = instance.Config.Bind(
                "Coin Drop Rate", 
                "Drop Min Chance", 
                0.5f, 
                new ConfigDescription("The lowest %chance for enemies to drop coins after DropMulti is applied. Vanilla is 0")
                );

            NewtShrineCost = instance.Config.Bind(
                "Lunar Prices",
                "Newt Shrine Cost",
                1f,
                new ConfigDescription("The cost to activate a newt shrine. Vanilla is 1")
                );
            PodCost = instance.Config.Bind(
                "Lunar Prices", 
                "Pod Cost", 
                0f, 
                new ConfigDescription("The cost of Lunar Pods found on stages. Vanilla is 1")
                );
            ShopCost = instance.Config.Bind(
                "Lunar Prices", 
                "Shop Cost", 
                1f, 
                new ConfigDescription("The cost of Lunar Buds in the Bazaar. Vanilla is 2")
                );
            ShopRefresh = instance.Config.Bind(
                "Lunar Prices", 
                "Shop Refresh", 
                true, 
                new ConfigDescription("Do empty Lunar Buds in the Bazaar refresh when the Slab (reroller) is used? Vanilla is false")
                );
            SeerCost = instance.Config.Bind(
                "Lunar Prices", 
                "Seer Cost", 
                1f, 
                new ConfigDescription("The cost of Lunar Seers in the Bazaar. Vanilla is 3")
                );
        }
    }
}
