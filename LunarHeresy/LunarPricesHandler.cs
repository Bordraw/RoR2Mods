using BepInEx;
using R2API;
using R2API.Networking;
using R2API.Utils;
using RoR2;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace LunarHeresy
{
    public static class LunarPricesHandler
    {
        private static LunarHeresy lunarHeresyInstance;

        internal static string[] lunarInteractables = {
            "RoR2/Base/LunarRecycler/LunarRecycler.prefab",
            "RoR2/Base/LunarChest/LunarChest.prefab",
            "RoR2/Base/NewtStatue/NewtStatue.prefab",
            "RoR2/Base/LunarShopTerminal/LunarShopTerminal.prefab",
            "RoR2/Base/bazaar/SeerStation.prefab",
            "RoR2/Base/moon/FrogInteractable.prefab"
        };

        public static void Register(LunarHeresy instance) {

            lunarHeresyInstance = instance;

            // Change the interactable costs of all lunar interactable prefabs to match config.
            // Some prefabs get instantiated before this call happens, which means we still need to update them on stage load.
            foreach (string x in lunarInteractables)
            {
                GameObject lunarInteractable = Addressables.LoadAssetAsync<GameObject>(x).WaitForCompletion();

                // Modify Lunar frog to be free and only require one pet
                if (lunarInteractable.name == "FrogInteractable")
                {
                    lunarInteractable.GetComponent<PurchaseInteraction>().Networkcost = 0;
                    lunarInteractable.GetComponent<FrogController>().maxPets = 1;
                    LunarHeresy.Logger.LogDebug("Glass frog price set to 0 and pet count set to 1");
                    continue;
                }

                PurchaseInteraction purchaseInteraction = lunarInteractable.GetComponent<PurchaseInteraction>();

                if (purchaseInteraction == null)
                {
                    LunarHeresy.Logger.LogWarning("Lunar interactable " + lunarInteractable.name + " has no purchase interaction to configure");
                    continue;
                }

                MaybeSetConfiguredLunarPrice(purchaseInteraction);
            }

            LunarHeresy.Logger.LogInfo("Finished modifying lunar interactable prefabs.");
        }

        // Sets the price of an interactable to match the value set in config if it doesn't already match
        private static void MaybeSetConfiguredLunarPrice(PurchaseInteraction purchaseInteraction)
        {
            int newPrice = -1;
            if (purchaseInteraction.name.StartsWith("LunarRecycler"))
            {
                newPrice = 0;
            }
            else if (purchaseInteraction.name.StartsWith("LunarChest"))
            {
                newPrice = (int)Configuration.PodCost.Value;
            }
            else if (purchaseInteraction.name.StartsWith("LunarShopTerminal"))
            {
                newPrice = (int)Configuration.ShopCost.Value;
            }
            else if (purchaseInteraction.name.StartsWith("SeerStation"))
            {
                newPrice = (int)Configuration.SeerCost.Value;
            }
            else if (purchaseInteraction.name.StartsWith("LunarRecycler"))
            {
                newPrice = 0;
            }
            else if (purchaseInteraction.name.StartsWith("NewtStatue"))
            {
                newPrice = (int)Configuration.NewtShrineCost.Value;
            }

            // Don't change the price on unrecognized interactables
            // Don't apply negative prices
            // Don't change interactables that are already correct
            if (newPrice < 0 || newPrice == purchaseInteraction.Networkcost)
            {
                LunarHeresy.Logger.LogDebug(purchaseInteraction.name + " price not modified");
                return;
            }
            else
            {
                // If our configured value is different from the price of this instance of the interactable, modify it
                purchaseInteraction.Networkcost = newPrice;
                if (newPrice == 0) purchaseInteraction.costType = CostTypeIndex.None;

                LunarHeresy.Logger.LogDebug(purchaseInteraction.name + " price set to " + newPrice);
            }
        }

        [Server]
        // Changes all of the interactables on the stage to match config values
        // This accounts for interactables instantiated before the prefabs were modified.
        public static void EnforceConfiguredInteractablePrices()
        {
            if (!NetworkServer.active)
            {
                LunarHeresy.Logger.LogWarning("[Server] function 'LunarPricesHandler.EnforceConfiguredInteractablePrices' called on client");
                return;
            }
            lunarHeresyInstance.StartCoroutine(_EnforceConfiguredInteractablePrices());
        }


        private static IEnumerator _EnforceConfiguredInteractablePrices()
        {
            // Delay to give time for instantiation of stage interactables.
            yield return null;

            foreach (PurchaseInteraction purchaseInteraction in InstanceTracker.GetInstancesList<PurchaseInteraction>())
                MaybeSetConfiguredLunarPrice(purchaseInteraction);

            LunarHeresy.Logger.LogInfo("Finished stage load modification of lunar interactable costs.");
        }
    }
}
