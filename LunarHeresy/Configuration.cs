using BepInEx.Configuration;
using R2API;

namespace LunarHeresy
{
    class Configuration
    {
        public static ConfigEntry<float> DropChance;
        public static ConfigEntry<float> DropMulti;
        public static ConfigEntry<float> DropMin;
        public static ConfigEntry<bool> ShareLunarCoins;

        public static ConfigEntry<float> NewtShrineCost;
        public static ConfigEntry<float> PodCost;
        public static ConfigEntry<float> ShopCost;
        public static ConfigEntry<bool> ShopRefresh;
        public static ConfigEntry<float> SeerCost;

        public static ConfigEntry<bool> EnableLunarCoinCauldrons;
        public static ConfigEntry<float> WhiteCauldronCost;
        public static ConfigEntry<float> GreenCauldronCost;
        public static ConfigEntry<float> RedCauldronCost;

        public static void Register(LunarHeresy instance)
        {
            #region Lunar Coins
            DropChance = instance.Config.Bind(
                "Lunar Coins", 
                "Drop Chance", 
                1.5f,
                new ConfigDescription("The initial %chance for enemies to drop coins. Vanilla is 0.5%")
                );
            DropMulti = instance.Config.Bind(
                "Lunar Coins", 
                "Drop Multiplier", 
                0.90f, 
                new ConfigDescription("The multiplier applied to the drop chance after a coin has dropped. Vanilla is 0.5 (chance reduced by 50% after each drop)")
                );
            DropMin = instance.Config.Bind(
                "Lunar Coins", 
                "Drop Min Chance", 
                0.5f, 
                new ConfigDescription("The lowest %chance for enemies to drop coins after DropMulti is applied. Vanilla is 0")
                );
            ShareLunarCoins = instance.Config.Bind(
                "Lunar Coins",
                "Share Lunar Coin Pickups",
                true,
                new ConfigDescription("Whether to share all lunar coin pickups among each player. Vanilla is false")
                );
            #endregion

            #region Lunar Prices
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
            #endregion

            #region Lunar Cauldrons
            EnableLunarCoinCauldrons = instance.Config.Bind(
                "Lunar Cauldrons",
                "Enable",
                true,
                new ConfigDescription("Change lunar cauldrons to cost lunar coins instead of items? Vanilla is false")
                );
            WhiteCauldronCost = instance.Config.Bind(
                "Lunar Cauldrons",
                "White Cauldron Cost",
                1f,
                new ConfigDescription("How many lunar coins it costs to use the cauldron that creates a white item.")
                );
            GreenCauldronCost = instance.Config.Bind(
                "Lunar Cauldrons",
                "Green Cauldron Cost",
                3f,
                new ConfigDescription("How many lunar coins it costs to use the cauldron that creates a green item.")
                );
            RedCauldronCost = instance.Config.Bind(
                "Lunar Cauldrons",
                "Red Cauldron Cost",
                5f,
                new ConfigDescription("How many lunar coins it costs to use the cauldron that creates a red item.")
                );
            #endregion
        }
    }
}
