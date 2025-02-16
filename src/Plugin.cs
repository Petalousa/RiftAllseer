using System;
using System.IO;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

using RhythmRift;

namespace RiftAllseer
{

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("RiftOfTheNecroDancer.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static ConfigEntry<bool> shouldDisableAnalytics;
        internal static ConfigEntry<bool> shouldLogAnalytics;

        internal static ConfigEntry<bool> shouldDisableHiddenTraps;

        internal static ConfigEntry<float> scrollSpeedModifier;

        private void Awake()
        {
            // Plugin startup logic
            Log = base.Logger;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            scrollSpeedModifier = Config.Bind("Gameplay", "Scroll Speed Modifier", 1.0f, "Increase / Decrease Scroll Speed");
            shouldDisableHiddenTraps = Config.Bind("Traps", "Disable Hidden Traps", true, "Prevents ? traps from spawning.");

            shouldDisableAnalytics = Config.Bind("Analytics", "Disable Analytics", true, "Prevents Analytics from being sent.");
            shouldLogAnalytics = Config.Bind("Analytics", "Log Analytics to Console", false, "Shows JSON of analytics in console.");
            // TODO allow player to change this and reload while in game instead of requiring restart.

            shouldDisableHiddenTraps.SettingChanged += (sender, args) => 
            {
                var configEntry = (ConfigEntry<bool>)sender;
                // Log the new value when changed
                Log.LogInfo($"Config changed: {configEntry.Value}");
            };
            scrollSpeedModifier.SettingChanged += (sender, args) => 
            {
                var configEntry = (ConfigEntry<bool>)sender;
                Log.LogInfo($"scrollSpeedModifier changed: {configEntry.Value}");
            };

            string pluginDLLLocation = Assembly.GetExecutingAssembly().Location;
            string pluginDirectory = Path.GetDirectoryName(pluginDLLLocation);
            string imageDirectoryPath = Path.Join(pluginDirectory, "res", "img");

            // start 
            SpriteLoader.Setup();
            SpriteLoader.SetBasePath(imageDirectoryPath);

            CustomSpriteShadows.LoadSprites();  // load sprites.
            Log.LogInfo("Sprites Loaded...");

            Harmony.CreateAndPatchAll(typeof(Plugin));
            Harmony.CreateAndPatchAll(typeof(CustomSpriteShadows));
            Harmony.CreateAndPatchAll(typeof(DisableAnalytics));
            Harmony.CreateAndPatchAll(typeof(GameplayPatches));
            Harmony.CreateAndPatchAll(typeof(TestPatches));
            Harmony.CreateAndPatchAll(typeof(RememberLastDifficulty));

            Log.LogInfo("Patched");
        }

        // private void Start()
        // {
        //     Logger = base.Logger;
        //     
        // }
        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.F5)) // Press F5 to reload config
            {
                Config.Reload();
                Log.LogInfo("Config reloaded manually.A");
                Log.LogWarning($"shouldDisableHiddenTraps {shouldDisableHiddenTraps.Value}");
                Log.LogWarning($"shouldDisableAnalytics {shouldDisableAnalytics.Value}");
                Log.LogWarning($"shouldLogAnalytics {shouldLogAnalytics.Value}");
                Log.LogWarning($"scrollSpeedModifier {scrollSpeedModifier.Value}");
            }
        }

        [HarmonyPatch(typeof(RRStageController), "UploadScoreToLeaderboardAndRefreshUi")]
        [HarmonyPrefix]
        static bool DisableLeaderboardScores(){
            Console.WriteLine("A Preventing score upload.");  // TODO make this more player visible
            return false;
        }
    }
}
