using BepInEx;
using Newtonsoft.Json;
using R2API.Networking;
using R2API.Networking.Interfaces;
using R2API.Utils;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LunarHeresy
{
    public class SyncCoinStorage : INetMessage
    {
        public ulong steamId;
        public uint coinCount;

        public SyncCoinStorage() { }

        public SyncCoinStorage(ulong steamId, uint coinCount)
        {
            this.steamId = steamId;
            this.coinCount = coinCount;
        }

        public void Deserialize(NetworkReader reader)
        {
            steamId = reader.ReadUInt64();
            coinCount = reader.ReadUInt32();
        }

        public void OnReceived()
        {
            if (NetworkServer.active)
            {
                LunarHeresy.Logger.LogWarning("SyncCoinStorage should only be run on clients.");
                return;
            }

            LunarCoinHandler.AddUser(steamId, coinCount, true);
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(steamId);
            writer.Write(coinCount);
        }
    }

    public static class LunarCoinHandler
    {
        // Maps from CSteamId.steamValue to the count of coins that Steam user has
        private static readonly Dictionary<ulong, uint> coinState = new Dictionary<ulong, uint>();

        public static void Register() {
            // Override the CostType delegates so that we can use a different coin count check when the artifact is active.
            // Hacky solution, but it works.
            RoR2Application.onLoad += () => {
                CostTypeCatalog.Register(CostTypeIndex.LunarCoin, new CostTypeDef
                {
                    costStringFormatToken = "COST_LUNARCOIN_FORMAT",
                    saturateWorldStyledCostString = false,
                    darkenWorldStyledCostString = true,
                    isAffordable = delegate (CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
                    {
                        NetworkUser networkUser = Util.LookUpBodyNetworkUser(context.activator.gameObject);
                        ulong steamId = networkUser.Network_id.steamId.steamValue;
                        return GetCoinsFromUser(steamId) >= context.cost;
                    },
                    payCost = delegate (CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
                    {
                        NetworkUser networkUser = Util.LookUpBodyNetworkUser(context.activator.gameObject);
                        if ((bool)networkUser)
                        {
                            networkUser.DeductLunarCoins((uint)context.cost);
                            RoR2.Items.MultiShopCardUtils.OnNonMoneyPurchase(context);
                        }
                    },
                    colorIndex = ColorCatalog.ColorIndex.LunarCoin
                });
            };

            NetworkingAPI.RegisterMessageType<SyncCoinStorage>();
        }

        [Server]
        public static void Load(string savedState) {
            Load(DeserializeState(savedState));
        }

        [Server]
        public static void Load(Dictionary<ulong, uint> loadedState = null) 
        {
            if (!NetworkServer.active)
            {
                LunarHeresy.Logger.LogWarning("[Server] function 'LunarCoinHandler.Load' called on client");
                return;
            }


            coinState.Clear();

            if (loadedState != null) {
                foreach (var record in loadedState) {
                    coinState.Add(record.Key, record.Value);
                }
            }

            foreach (NetworkUser user in NetworkUser.readOnlyInstancesList)
            {
                if (user == null)
                {
                    LunarHeresy.Logger.LogWarning("Encountered disconnected player while loading coin tracking.");
                    return;
                }

                ulong steamId = user.Network_id.steamId.steamValue;

                // Add any players who aren't already tracked
                AddUser(steamId);

                // Update any clients
                if (NetworkServer.active) new SyncCoinStorage(steamId, coinState[steamId]).Send(NetworkDestination.Clients);
            }
            LunarHeresy.Logger.LogInfo("Loaded coin tracking.");
        }

        public static string Save()
        {
            // Convert the keys and values into arrays?
            return SerializeState();
        }

        private static Dictionary<ulong, uint> DeserializeState(string savedState)
        {
            // Convert the keys and values into arrays?
            return JsonConvert.DeserializeObject<Dictionary<ulong, uint>>(savedState);
        }

        private static string SerializeState() {
            // Convert the keys and values into arrays?
            return JsonConvert.SerializeObject(coinState);
        }

        [Server]
        public static void AddUser(ulong steamId, uint count = 0, bool force = false) {
            if (!NetworkServer.active)
            {
                LunarHeresy.Logger.LogWarning("[Server] function 'LunarCoinHandler.AddUser' called on client");
                return;
            }

            if (!coinState.ContainsKey(steamId))
            {
                coinState.Add(steamId, count);
                LunarHeresy.Logger.LogInfo($"{steamId} added to coin tracking");
            } else if (force) {
                coinState[steamId] = count;
                LunarHeresy.Logger.LogInfo($"Lunar coins set to {count} for {steamId}");
            }

            // Update the clients
            new SyncCoinStorage(steamId, coinState[steamId]).Send(NetworkDestination.Clients);
        }

        public static void GiveCoinsToUser(ulong steamId, uint count)
        {
            if (!coinState.ContainsKey(steamId))
            {
                LunarHeresy.Logger.LogInfo($"Tried to give coins to non-registered user {steamId}");
                return;
            }

            coinState[steamId] += count;
            LunarHeresy.Logger.LogInfo($"Gave {count} lunar coins to {steamId}");
        }

        public static void TakeCoinsFromUser(ulong steamId, uint count)
        {
            if (!coinState.ContainsKey(steamId)) {
                coinState.Add(steamId, 0);
                LunarHeresy.Logger.LogInfo($"{steamId} added to coin tracking");
                return;
            }

            if (coinState[steamId] <= count) {
                coinState[steamId] = 0;
            }
            else {
                coinState[steamId] -= count;
            }
            
            LunarHeresy.Logger.LogInfo($"Took {count} lunar coins from {steamId}");
        }

        public static uint GetCoinsFromUser(ulong steamId)
        {
            // Spams the console due to HUD hook, only used for debugging.
            // LunarHeresy.Logger.LogInfo("getCoinsFromUser: " + user.userName + player.coinCount);
            if (!coinState.ContainsKey(steamId)) {
                return 0;
            } else {
                return coinState[steamId];
            }
        }
    }
}
