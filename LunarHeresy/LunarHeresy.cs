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
        // The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Bordraw";
        public const string PluginName = "LunarHeresy";
        public const string PluginVersion = "1.0.0";

        public static new BepInEx.Logging.ManualLogSource Logger;

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Setup logger for debugging
            Logger = base.Logger;

            // Load configuration
            Configuration.Register(this);

            // Enable per-run lunar coins
            LunarCoinHandler.Register();

            // Enforce lunar price changes
            LunarPricesHandler.Register(this);

            // Enable ProperSave Compatibility
            if (ProperSaveCompatibility.enabled) ProperSaveCompatibility.Setup();

            // Register Hooks
            Hooks.Register();

            // This line of log will appear in the bepinex console when the Awake method is done.
            Logger.LogInfo(nameof(Awake) + " done.");
        }
    }
}
