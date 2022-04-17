using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace LunarHeresy
{
    public static class LunarCauldronHandler
    {

        internal static string[] cauldronInteractables = {
            "RoR2/Base/LunarCauldrons/LunarCauldron, GreenToRed Variant.prefab",
            "RoR2/Base/LunarCauldrons/LunarCauldron, RedToWhite Variant.prefab",
            "RoR2/Base/LunarCauldrons/LunarCauldron, WhiteToGreen.prefab",
        };

        private static LunarHeresy lunarHeresyInstance;

        public static void Register(LunarHeresy instance)
        {
            lunarHeresyInstance = instance;

            if (!(bool)Configuration.EnableLunarCoinCauldrons.Value) return;

            // Change the interactable costs and drop events of all lunar cauldron prefabs to match config.
            // Some prefabs get instantiated before this call happens, which means we still need to update them on stage load.
            foreach (string x in cauldronInteractables)
            {
                GameObject cauldronInteractable = Addressables.LoadAssetAsync<GameObject>(x).WaitForCompletion();

                var purchaseInteraction = cauldronInteractable.GetComponent<PurchaseInteraction>();
                if (purchaseInteraction == null) 
                    LunarHeresy.Logger.LogWarning("Lunar cauldron " + cauldronInteractable.name + " has no purchase interaction to configure");
                else
                    ModifyLunarCauldronCost(purchaseInteraction);

                var delayedEvent = cauldronInteractable.GetComponent<RoR2.EntityLogic.DelayedEvent>();
                if (delayedEvent != null) MaybeModifyLunarCauldronDrop(delayedEvent);
            }

            LunarHeresy.Logger.LogInfo("Finished modifying lunar cauldron prefabs.");
        }

        private static void ModifyLunarCauldronCost(PurchaseInteraction purchaseInteraction) {
            if (purchaseInteraction.name.StartsWith("LunarCauldron"))
            {
                int newPrice = -1;

                if (purchaseInteraction.name.StartsWith("LunarCauldron, RedToWhite"))
                {
                    newPrice = (int)Configuration.WhiteCauldronCost.Value;
                }
                else if (purchaseInteraction.name.StartsWith("LunarCauldron, WhiteToGreen"))
                {
                    newPrice = (int)Configuration.GreenCauldronCost.Value;
                }
                else if (purchaseInteraction.name.StartsWith("LunarCauldron, GreenToRed"))
                {
                    newPrice = (int)Configuration.RedCauldronCost.Value;
                }

                // Do nothing if the configured price is negative
                if (newPrice < 0)
                {
                    LunarHeresy.Logger.LogDebug(purchaseInteraction.name + " price not modified");
                    return;
                }

                // Change the cost type to Lunar coins (or None if the cost is 0)
                purchaseInteraction.costType = newPrice == 0 ? CostTypeIndex.None : CostTypeIndex.LunarCoin;
                purchaseInteraction.cost = newPrice;
                purchaseInteraction.Networkcost = newPrice;
                // purchaseInteraction.contextToken = "EXAMPLE_TOKEN_007";
                LunarHeresy.Logger.LogDebug(purchaseInteraction.name + " price set to " + newPrice + " lunar coins");
            }
        }

        [Server]
        public static void MaybeModifyLunarCauldronDrop(RoR2.EntityLogic.DelayedEvent delayedEvent) {
            if (delayedEvent.name.StartsWith("LunarCauldron, RedToWhite"))
            {
                PersistentCallGroup listenerGroup = delayedEvent.action.m_PersistentCalls;
                bool performEdit = false;

                // Filter out extra DropPickup calls
                List<PersistentCall> filteredListeners = new List<PersistentCall>();
                foreach (PersistentCall listener in listenerGroup.GetListeners())
                {
                    if (listener.methodName == "DropPickup")
                    {
                        if (!performEdit) performEdit = true;
                        else continue;
                    }
                    filteredListeners.Add(listener);
                }

                if (!performEdit)
                {
                    LunarHeresy.Logger.LogWarning("No edits necessary for white cauldron");
                    return;
                }

                LunarHeresy.Logger.LogWarning("Modifying white cauldron");

                listenerGroup.Clear();

                foreach (PersistentCall listener in filteredListeners)
                    listenerGroup.AddListener(listener);
            }
        }

        [Server]
        // Changes all of the cauldrons on the stage to match config values
        // This accounts for interactables instantiated before the prefabs were modified.
        public static void EnforceConfiguredCauldronPrices()
        {
            if (!NetworkServer.active)
            {
                LunarHeresy.Logger.LogWarning("[Server] function 'LunarCauldronhandler.EnforceConfiguredCauldronPrices' called on client");
                return;
            }

            if (!(bool)Configuration.EnableLunarCoinCauldrons.Value) return;

            lunarHeresyInstance.StartCoroutine(_EnforceConfiguredCauldronPrices());
        }


        private static IEnumerator _EnforceConfiguredCauldronPrices()
        {
            // Delay to give time for instantiation of stage interactables.
            yield return null;

            foreach (PurchaseInteraction purchaseInteraction in InstanceTracker.GetInstancesList<PurchaseInteraction>())
                ModifyLunarCauldronCost(purchaseInteraction);

            LunarHeresy.Logger.LogInfo("Finished stage load modification of lunar cauldrons.");
        }
    }
}
