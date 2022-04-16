using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LunarHeresy
{
    public static class ProperSaveCompatibility
    {
        private static bool? _enabled;

        public static bool enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.KingEnderBrine.ProperSave");
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsRunNew() => !ProperSave.Loading.IsLoading;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Setup() {
            ProperSave.SaveFile.OnGatherSaveData += (dict) =>
            {
                string savedState = LunarCoinHandler.Save();
                if (dict.ContainsKey("lunarCoinState"))
                {
                    dict["ephemeralCoinCount"] = savedState;
                }
                else
                {
                    dict.Add("lunarCoinState", savedState);
                }
                LunarHeresy.Logger.LogInfo("Saved state: " + savedState);
            };

            ProperSave.Loading.OnLoadingEnded += (save) =>
            {
                string loadedState = save.GetModdedData<string>("lunarCoinState");
                LunarCoinHandler.Load(loadedState);
                LunarHeresy.Logger.LogInfo("Loaded state: " + loadedState);
            };
        }
    }
}