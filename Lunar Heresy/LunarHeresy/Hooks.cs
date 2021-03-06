using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace LunarHeresy
{
    public class Hooks
    {
        public static void Register()
        {
            // Main setup hook; this is where most of the mod's settings are applied
            On.RoR2.Run.Start += Run_Start;

            // Coin counter
            On.RoR2.UI.HUD.Update += HUD_Update;
            On.RoR2.Run.OnUserAdded += Run_OnUserAdded;
            // On.RoR2.Run.OnUserRemoved += Run_OnUserRemoved;

            // Cost functions
            On.RoR2.NetworkUser.RpcAwardLunarCoins += NetworkUser_RpcAwardLunarCoins;
            On.RoR2.NetworkUser.RpcDeductLunarCoins += NetworkUser_RpcDeductLunarCoins;
            On.RoR2.NetworkUser.SyncLunarCoinsToServer += NetworkUser_SyncLunarCoinsToServer;

            // Coin base drop chance
            On.RoR2.PlayerCharacterMasterController.Awake += PlayerCharacterMasterController_Awake;

            // Coin drop multiplier
            BindingFlags allFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var initDelegate = typeof(PlayerCharacterMasterController).GetNestedTypes(allFlags)[0].GetMethodCached(name: "<Init>b__72_0");
            MonoMod.RuntimeDetour.HookGen.HookEndpointManager.Modify(initDelegate, (Action<ILContext>)CoinDropHook);

            // Share lunar coin drops
            On.RoR2.GenericPickupController.OnInteractionBegin += GenericPickupController_OnInteractionBegin;

            // Bazaar is awful and requires a million hooks to do simple things
            // It has pre-loaded instances of the shop pods, seer stations, and recycler, so changing the prefabs does nothing.
            // Thus, we are forced to hook into their behaviors instead.
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.PurchaseInteraction.SetAvailable += PurchaseInteraction_SetAvailable;
            On.RoR2.ShopTerminalBehavior.GenerateNewPickupServer += ShopTerminalBehavior_GenerateNewPickupServer;
            On.RoR2.BazaarController.SetUpSeerStations += BazaarController_SetUpSeerStations;

            // Modify lunar cauldron behavior
            On.RoR2.EntityLogic.DelayedEvent.CallDelayed += DelayedEvent_CallDelayed;

            // Set up twisted scavenger loop with extended fadeout
            EntityStates.Missions.LunarScavengerEncounter.FadeOut.duration *= 5.0f;
            EntityStates.Missions.LunarScavengerEncounter.FadeOut.delay *= 3.0f;

            // Spawn a portal to the bazaar after defeating the twisted scavenger
            On.EntityStates.Missions.LunarScavengerEncounter.FadeOut.OnEnter += LunarScavengerEncounter_FadeOut;
        }

        #region Per-Run-Coins
        private static void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
        {
            orig(self);

            if (NetworkServer.active)
            {
                // If this is a new run, load the empty state
                if (ProperSaveCompatibility.enabled ? ProperSaveCompatibility.IsRunNew() : Run.instance.stageClearCount == 0)
                {
                    LunarCoinHandler.Load();
                    LunarHeresy.Logger.LogInfo("Loading fresh coin state for new run.");
                }
            }
        }

        private static void HUD_Update(On.RoR2.UI.HUD.orig_Update orig, RoR2.UI.HUD self)
        {
            orig(self);

            if (self == null || self?.lunarCoinText == null || self?.lunarCoinContainer == null) return;

            var user = self._localUserViewer?.currentNetworkUser?.Network_id.steamId.steamValue;
            if(user != null) self.lunarCoinText.targetValue = (int)LunarCoinHandler.GetCoinsFromUser((ulong)user);
            
            var lunarCoinText = self.lunarCoinContainer.transform?.Find("LunarCoinSign")?.GetComponent<RoR2.UI.HGTextMeshProUGUI>()?.text;
            if (lunarCoinText != null) lunarCoinText = "<sprite name=\"LunarCoin\" color=#adf2fa>";
        }

        private static void Run_OnUserAdded(On.RoR2.Run.orig_OnUserAdded orig, Run self, NetworkUser user)
        {
            orig(self, user);
            if (NetworkServer.active && Run.instance.time > 1f) LunarCoinHandler.AddUser(user.Network_id.steamId.steamValue);
        }

        private static void NetworkUser_RpcAwardLunarCoins(On.RoR2.NetworkUser.orig_RpcAwardLunarCoins orig, RoR2.NetworkUser self, uint count)
        {
            orig(self, 0);
            LunarCoinHandler.GiveCoinsToUser(self.Network_id.steamId.steamValue, count);
            self.SyncLunarCoinsToServer();
        }

        private static void NetworkUser_RpcDeductLunarCoins(On.RoR2.NetworkUser.orig_RpcDeductLunarCoins orig, RoR2.NetworkUser self, uint count)
        {
            orig(self, 0);
            LunarCoinHandler.TakeCoinsFromUser(self.Network_id.steamId.steamValue, count);
            self.SyncLunarCoinsToServer();
        }

        private static void NetworkUser_SyncLunarCoinsToServer(On.RoR2.NetworkUser.orig_SyncLunarCoinsToServer orig, RoR2.NetworkUser self)
        {
            orig(self);
            self.CallCmdSetNetLunarCoins((uint)LunarCoinHandler.GetCoinsFromUser(self.Network_id.steamId.steamValue));
        }
        #endregion

        #region Coin Drop
        private static void PlayerCharacterMasterController_Awake(On.RoR2.PlayerCharacterMasterController.orig_Awake orig, PlayerCharacterMasterController self)
        {
            orig(self);
            self.SetFieldValue("lunarCoinChanceMultiplier", Configuration.DropChance.Value);
        }

        private static void CoinDropHook(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(
                x => x.MatchDup(),
                x => x.MatchLdfld<PlayerCharacterMasterController>("lunarCoinChanceMultiplier"),
                x => x.MatchLdcR4(0.5f),
                x => x.MatchMul()
                );
            c.Index += 2;
            c.Next.Operand = Configuration.DropMulti.Value;
            c.Index += 2;
            c.EmitDelegate<Func<float, float>>((originalChance) =>
            {
                return Math.Max(originalChance, Configuration.DropMin.Value);
            });
        }

        private static void GenericPickupController_OnInteractionBegin(On.RoR2.GenericPickupController.orig_OnInteractionBegin orig, GenericPickupController self, Interactor interactor)
        {
            orig(self, interactor);

            if (NetworkServer.active && Configuration.ShareLunarCoins.Value &&
                self.pickupIndex == PickupCatalog.FindPickupIndex("LunarCoin.Coin0"))
            {
                foreach (var pcmc in PlayerCharacterMasterController.instances)
                {
                    // Award it to everyone but who picked it up
                    if (interactor.GetComponent<CharacterBody>() != pcmc.master.GetBody())
                    {
                        pcmc.networkUser.AwardLunarCoins(1);
                    }
                }
            }
        }
        #endregion

        #region BazaarChanges
        private static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                // Ensure all lunar interactables on the stage match their configured price
                LunarPricesHandler.EnforceConfiguredInteractablePrices();

                // Ensure all cauldrons on the stage match their configured price
                LunarCauldronHandler.EnforceConfiguredCauldronPrices();

                // Make sure that white cauldron is present on Commencement
                if (Configuration.GuaranteeWhiteOnMoon.Value && SceneManager.GetActiveScene().name == "moon2") {
                    LunarHeresy.Logger.LogDebug("Guaranteeing white cauldron on commencement");
                    LunarCauldronHandler.GuaranteeWhiteCauldron();
                }
            }
        }

        private static void BazaarController_SetUpSeerStations(On.RoR2.BazaarController.orig_SetUpSeerStations orig, BazaarController self)
        {
            orig(self);
            foreach (SeerStationController seerStationController in self.seerStations)
            {
                seerStationController.GetComponent<PurchaseInteraction>().Networkcost = (int)Configuration.SeerCost.Value;
                if (Configuration.SeerCost.Value == 0) { seerStationController.GetComponent<PurchaseInteraction>().costType = CostTypeIndex.None; }
            }
        }

        [Server]
        private static void PurchaseInteraction_SetAvailable(On.RoR2.PurchaseInteraction.orig_SetAvailable orig, PurchaseInteraction self, bool newAvailable)
        {
            if (self.name.StartsWith("LunarRecycler")) {
                orig(self, false);
            } else { 
                orig(self, newAvailable); 
            }
        }

        [Server]
        private static void ShopTerminalBehavior_GenerateNewPickupServer(On.RoR2.ShopTerminalBehavior.orig_GenerateNewPickupServer orig, ShopTerminalBehavior self)
        {
            if (self.name.StartsWith("LunarShop") && Configuration.ShopRefresh.Value) { 
                self.NetworkhasBeenPurchased = false;
                orig(self);
                self.GetComponent<PurchaseInteraction>().SetAvailable(true);
                return;
            }

            orig(self);
        }

        [Server]
        private static void DelayedEvent_CallDelayed(On.RoR2.EntityLogic.DelayedEvent.orig_CallDelayed orig, RoR2.EntityLogic.DelayedEvent self, float timer) 
        {
            if (NetworkServer.active & Configuration.EnableLunarCoinCauldrons.Value && self.name.StartsWith("LunarCauldron"))
            {
                // When using lunar coins to pay for cauldrons we don't want a delay to wait for items
                timer = 0f;

                // Remove duplicate DropPickup calls so it only drops one white per lunar coin
                LunarCauldronHandler.MaybeModifyLunarCauldronDrop(self);
            }

            orig(self, timer);
        }
        #endregion

        #region TwistedScavengerLoop
        private static void LunarScavengerEncounter_FadeOut(On.EntityStates.Missions.LunarScavengerEncounter.FadeOut.orig_OnEnter orig, EntityStates.Missions.LunarScavengerEncounter.FadeOut self)
        {
            orig(self);

            // Only the server spawns the portal
            if (!NetworkServer.active) return;

            // Create spawn card for Bazaar portal to use in SpawnRequest
            SpawnCard portalSpawnCard = ScriptableObject.CreateInstance<SpawnCard>();
            portalSpawnCard.prefab = Resources.Load<GameObject>("prefabs/networkedobjects/PortalShop");

            GameObject portalObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(portalSpawnCard, new DirectorPlacementRule
            {
                // Set the position of the portal to the center of the arena on the floor
                position = new Vector3(0f, -10f, 0f),
                minDistance = 10,
                maxDistance = 15,
                placementMode = DirectorPlacementRule.PlacementMode.Direct,
            }, RoR2Application.rng));

            // If the portal spawned successfully, post in the chat and update the run state
            if (portalObject)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = "PORTAL_SHOP_OPEN" });
                Run.instance.shopPortalCount++;
            }
        }
        #endregion
    }
}
