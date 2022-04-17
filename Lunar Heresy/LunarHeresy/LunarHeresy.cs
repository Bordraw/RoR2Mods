using BepInEx;
using R2API;
using R2API.Networking;
using R2API.Utils;

namespace LunarHeresy
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [R2APISubmoduleDependency(nameof(ItemAPI), nameof(LanguageAPI), nameof(NetworkingAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency("com.KingEnderBrine.ProperSave", BepInDependency.DependencyFlags.SoftDependency)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class LunarHeresy : BaseUnityPlugin
    {
        public const string PluginAuthor = "Bordraw";
        public const string PluginName = "LunarHeresy";
        public const string PluginVersion = "1.0.0";
        public const string PluginGUID = PluginAuthor + "." + PluginName;

        public static new BepInEx.Logging.ManualLogSource Logger;
        public static PluginInfo PluginInfo { get; private set; }

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Setup logger for debugging
            Logger = base.Logger;

            // Store PluginInfo
            PluginInfo = base.Info;

            // Load configuration
            Configuration.Register(this);

            // Register language files with R2API
            Languages.Register();

            // Enable per-run lunar coins
            LunarCoinHandler.Register();

            // Enforce lunar price changes
            LunarPricesHandler.Register(this);

            // Change lunar cauldrons to cost lunar coins
            LunarCauldronHandler.Register(this);

            // Enable ProperSave Compatibility
            if (ProperSaveCompatibility.enabled) ProperSaveCompatibility.Setup();

            // Register Hooks
            Hooks.Register();

            // This line of log will appear in the bepinex console when the Awake method is done.
            Logger.LogInfo(nameof(Awake) + " done.");
        }
    }
}
