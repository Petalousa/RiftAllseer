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
            Harmony.CreateAndPatchAll(typeof(CustomMenuOptions));
            Harmony.CreateAndPatchAll(typeof(CustomSpriteShadows));
            Harmony.CreateAndPatchAll(typeof(DisableAnalytics));
            Harmony.CreateAndPatchAll(typeof(GameplayPatches));
            //Harmony.CreateAndPatchAll(typeof(TestPatches));
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

            if (Input.GetKeyDown(KeyCode.F6)) // Press F5 to reload config
            {
                Log.LogInfo("=== SCENE ===");
                LogSceneHierarchy();
            }
        }

            private void LogSceneHierarchy()
            {
                // Get all root objects in the current scene
                GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

                // Recursively log each root object's hierarchy
                foreach (GameObject rootObj in rootObjects)
                {
                    LogGameObjectHierarchy(rootObj, 1);
                }
            }

        private void LogGameObjectHierarchy(GameObject obj, int indentLevel)
        {
            // Create indentation based on the level of the hierarchy
            string indent = new string('-', indentLevel * 2);

            // Log the current GameObject's name and its type
            Log.LogInfo($"{indent} {obj.name} (Type: {obj.GetType()})");

            // Recursively log all children of the current GameObject
            foreach (Transform child in obj.transform)
            {
                LogGameObjectHierarchy(child.gameObject, indentLevel + 1);
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
