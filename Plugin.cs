using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using BepInEx.Configuration;
using HarmonyLib;
using RhythmRift.Traps;
using RhythmRift;
using Shared.RhythmEngine;
using Shared.Analytics;
using System;
using System.Reflection;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.UIElements;
using UnityEngine.Experimental.Rendering;
using RhythmRift.Enemies;
using System.IO;

namespace RiftAllseer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("RiftOfTheNecroDancer.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    internal static ConfigEntry<bool> shouldDisableAnalytics;
    internal static ConfigEntry<bool> shouldLogAnalytics;

    internal static ConfigEntry<bool> shouldDisableHiddenTraps;

    internal static ConfigEntry<float> scrollSpeedModifier;
    internal static float z_offset = 0.0f;
    internal static float y_offset = 0.0f;
    internal static Texture2D customOnBeatTexture;
    internal static Texture2D customHalfBeatTexture;
    internal static Texture2D customOtherBeatTexture;
    internal static Sprite customOnBeatSprite;
    internal static Sprite customHalfBeatSprite;
    internal static Sprite customOtherBeatSprite;
    private GameObject _textObject;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        scrollSpeedModifier = Config.Bind("Gameplay", "Scroll Speed Modifier", 1.0f, "Increase / Decrease Scroll Speed");
        shouldDisableHiddenTraps = Config.Bind("Traps", "Disable Hidden Traps", true, "Prevents ? traps from spawning.");

        shouldDisableAnalytics = Config.Bind("Analytics", "Disable Analytics", true, "Prevents Analytics from being sent.");
        shouldLogAnalytics = Config.Bind("Analytics", "Log Analytics to Console", false, "Shows JSON of analytics in console.");
        // TODO allow player to change this and reload while in game instead of requiring restart.

        shouldDisableHiddenTraps.SettingChanged += (sender, args) => 
        {
            var configEntry = (ConfigEntry<bool>)sender;
            // Log the new value when changed
            Logger.LogInfo($"Config changed: {configEntry.Value}");
        };
        scrollSpeedModifier.SettingChanged += (sender, args) => 
        {
            var configEntry = (ConfigEntry<bool>)sender;
            Logger.LogInfo($"scrollSpeedModifier changed: {configEntry.Value}");
        };

        string whermst = Assembly.GetExecutingAssembly().Location;
        // Logger.LogWarning($"START A - {whermst} ");
        string dir_name = Path.GetDirectoryName(whermst);
        // Logger.LogWarning($"START A - {dir_name} ");
        // string modFolder = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Name;
        // Logger.LogWarning($"START - {modFolder} ");
        
        // Create(Texture2D texture, Rect rect, Vector2 pivot); 
        string imageDirectoryPath = Path.Join(dir_name, "img");

        customOnBeatTexture = new Texture2D(512, 512, GraphicsFormat.R8G8B8A8_UNorm, 1, TextureCreationFlags.None);
        customHalfBeatTexture = new Texture2D(512, 512, GraphicsFormat.R8G8B8A8_UNorm, 1, TextureCreationFlags.None);
        customOtherBeatTexture = new Texture2D(512, 512, GraphicsFormat.R8G8B8A8_UNorm, 1, TextureCreationFlags.None);

        customOnBeatTexture.LoadImage(File.ReadAllBytes(Path.Join(imageDirectoryPath, "on_beat.PNG")));
        customHalfBeatTexture.LoadImage(File.ReadAllBytes(Path.Join(imageDirectoryPath, "half_beat.PNG")));
        customOtherBeatTexture.LoadImage(File.ReadAllBytes(Path.Join(imageDirectoryPath, "other_beat.PNG")));

        Rect rect1 = new Rect(0, 0, customOnBeatTexture.width, customOnBeatTexture.height);
        Rect rect2 = new Rect(0, 0, customOnBeatTexture.width, customOnBeatTexture.height);
        Rect rect3 = new Rect(0, 0, customOnBeatTexture.width, customOnBeatTexture.height);
        // TODO automatically adjust the pixelsToUnits

        customOnBeatSprite = Sprite.Create(customOnBeatTexture, rect1, new Vector2(0.5f, 0.5f), 50.0f);
        customHalfBeatSprite = Sprite.Create(customHalfBeatTexture, rect2, new Vector2(0.5f, 0.5f), 50.0f);
        customOtherBeatSprite = Sprite.Create(customOtherBeatTexture, rect3, new Vector2(0.5f, 0.5f), 50.0f);

        Harmony.CreateAndPatchAll(typeof(Plugin));

        // Positioning (using anchors for top-right)
        // RectTransform rectTransform = _textObject.GetComponent<RectTransform>();
        // rectTransform.anchorMin = new Vector2(1, 1); // Top-right
        // rectTransform.anchorMax = new Vector2(1, 1); // Top-right
        // rectTransform.pivot = new Vector2(1, 1);   // Top-right
        // rectTransform.anchoredPosition = new Vector2(-10, -10); // Offset from top-right corner (adjust as needed)

        Logger.LogInfo("Patched");
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
            Logger.LogInfo("Config reloaded manually.A");
            Logger.LogWarning($"shouldDisableHiddenTraps {shouldDisableHiddenTraps.Value}");
            Logger.LogWarning($"shouldDisableAnalytics {shouldDisableAnalytics.Value}");
            Logger.LogWarning($"shouldLogAnalytics {shouldLogAnalytics.Value}");
            Logger.LogWarning($"scrollSpeedModifier {scrollSpeedModifier.Value}");
        }

        // use page up / down to modify scroll speed.
        if (Input.GetKeyDown(KeyCode.LeftShift)){
            if (Input.GetKeyUp(KeyCode.PageUp))
            {
                z_offset += 0.1f;
            }
            if (Input.GetKeyUp(KeyCode.PageDown))
            {
                z_offset -= 0.1f;
            }
        } else {
            if (Input.GetKeyUp(KeyCode.PageUp))
            {
                y_offset += 0.1f;
            }
            if (Input.GetKeyUp(KeyCode.PageDown))
            {
                y_offset -= 0.1f;
            }
        }
        if (Input.GetKeyUp(KeyCode.Home))
        {
            Logger.LogWarning($"offsets info: y {y_offset} z {z_offset}");
        }
    }

    [HarmonyPatch(typeof(RRTrapController), "SpawnTrap")]
    [HarmonyPrefix]
    static bool SkipAllTrapSpawns(ref TrapSpawnData trapSpawnData) {
        if (trapSpawnData.TrapType == RRTrapType.Mystery && shouldDisableHiddenTraps.Value)
        {
            Console.WriteLine("Skipping ? trap spawned");
            return false; // Skip original method
        }
        return true; // Execute original method
    }

    [HarmonyPatch(typeof(RREnemyController), "SpawnEnemy", new Type[] {typeof(SpawnEnemyData), typeof(Guid), typeof(FmodTimeCapsule)})]
    [HarmonyPrefix]
    static bool AlertEnemySpawn() {
        //Console.WriteLine("enemy spawned");
        return true; // Execute original method
    }

    [HarmonyPatch(typeof(RiftAnalyticsService), "SendAnalyticsEvent")]
    [HarmonyPrefix]
    static bool DisableAnalytics(ref string tableName, ref string dataJsonString, bool shouldLogResponse = true){
        if (shouldLogAnalytics.Value) {
            Console.Write("Analytics send event triggered:");
            Console.WriteLine(tableName);
            Console.WriteLine(dataJsonString);
        }

        if (shouldDisableAnalytics.Value){
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(RRStageController), "UploadScoreToLeaderboardAndRefreshUi")]
    [HarmonyPrefix]
    static bool DisableLeaderboardScores(){
        Console.WriteLine("Preventing score upload.");  // TODO make this more player visible
        return false;
    }

    [HarmonyPatch(typeof(RhythmRift.RRTileView), "Awake")]
    [HarmonyPostfix]
    static void CreateTile(ref MaterialPropertyBlock ____materialPropertyBlock){
        Console.WriteLine($"Tile AWAKE! {____materialPropertyBlock}");
        Vector3 newPosition = ____materialPropertyBlock.GetVector("_Position");
        newPosition.x -= 100.0f;
        newPosition.y += 500.0f;
        newPosition.z += 700.0f;
        ____materialPropertyBlock.SetVector("_Position", newPosition);
    }

    [HarmonyPatch(typeof(RhythmRift.RRGridView), "GetTileWorldPositionFromGridPosition")]
    [HarmonyPostfix]
    static void GetTileWorldPositionFromGridPosition(int xCoordinate, int yCoordinate, ref Vector3 __result){
        //Console.WriteLine($"modifying coord for {xCoordinate} {yCoordinate} - {__result.x} {__result.y} {__result.z}!");
        __result = new Vector3(
            __result.x ,//+ (float)Math.Sin(Time.time * 4.0) * 0.5f,
            __result.y + y_offset, // y is near/far plane (can use it for shrinking lmao)
            __result.z + z_offset  // z is up/down screen
        );
    }

    [HarmonyPatch(typeof(RREnemyController), "Initialize")]
    [HarmonyPrefix]
    static bool Initialize(ref IRRGridDataAccessor gridDataAccessor){
        Console.WriteLine($"Initialized EnemyController.... grid is r{gridDataAccessor.NumRows} x r{gridDataAccessor.NumColumns}");



        // asdf
        //gridDataAccessor.GetType().GetField("NumRows", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(gridDataAccessor, 5);

        //Console.WriteLine($"Reflected upon enemyController.... grid is r{gridDataAccessor.NumRows} x r{gridDataAccessor.NumColumns}");

        return true;
    }
    [HarmonyPatch(typeof(RhythmRift.Enemies.RREnemy), "UpdateMovement")]
    [HarmonyPostfix]
    static void Whoop(ref RREnemy __instance){
        //Vector3 newPosition = ((Component)__instance).transform.position;
        //newPosition.x += (float)Math.Sin(Time.time * 8.0) * 0.1f;
        //((Component)__instance).transform.position = newPosition;
        
    }
    [HarmonyPatch(typeof(RREnemy), "Initialize")]
    [HarmonyPrefix]
    static bool NewHooves(ref RREnemy __instance){
        Console.WriteLine("Minitialized");


        FieldInfo onBeatField = typeof(RREnemy).GetField("_onBeatShadowSprite", BindingFlags.Instance | BindingFlags.NonPublic);
        onBeatField.SetValue(__instance, customOnBeatSprite);

        FieldInfo halfBeatField = typeof(RREnemy).GetField("_halfBeatShadowSprite", BindingFlags.Instance | BindingFlags.NonPublic);
        halfBeatField.SetValue(__instance, customHalfBeatSprite);

        FieldInfo otherBeatField = typeof(RREnemy).GetField("_otherBeatShadowSprite", BindingFlags.Instance | BindingFlags.NonPublic);
        otherBeatField.SetValue(__instance, customOtherBeatSprite);


        // Sprite shadow_sprite = (Sprite)spr.GetValue(__instance);
        // if (shadow_sprite == null){
        //     return true;
        // } else if (starSprite == shadow_sprite){
        //     Console.WriteLine("ALreeedy mine :)");
        //     return true;
        // }
        // if (shadow_sprite.texture == null){
        //     return true;
        // }

        // Console.Write("texture info: ");
        // Console.Write($"texture : {shadow_sprite.texture}");
        // Console.Write($"rect : {shadow_sprite.rect}");
        // Console.Write($"pivot : {shadow_sprite.pivot}");
        // Console.Write($"pixelsPerUnit : {shadow_sprite.pixelsPerUnit}");
        // Console.Write($"textureRect : {shadow_sprite.textureRect}");
        // Console.Write($"packed : {shadow_sprite.packed}");
        // Console.Write($"border : {shadow_sprite.border}");
        // Console.Write("\n");

        // spr.SetValue(__instance, starSprite);

        // if (shadow_sprite == null){
        //     Console.WriteLine("Canbdnna find me a RREMEMEMNAY spriite.");
        //     return true;
        // }

        // FieldInfo textureField = typeof(Sprite).GetField("m_texture", 
        //     BindingFlags.Instance | BindingFlags.NonPublic);

        // // Or search through all private fields if you're not sure of the name
        // FieldInfo[] privateFields = typeof(Sprite).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
        // foreach (var field in privateFields) {
        //     Console.WriteLine(field.Name); // This will help you find the actual field name
        // }

        // // Once you have the right field, you can set it
        // if (textureField == null) {
        //     Console.WriteLine("Error finding texturefield."); // This will help you find the actual field name
        //     return true;
        // }
        // textureField.SetValue(shadow_sprite, starTexture);

        // //shadow_sprite.texture = starTexture;
        // Console.WriteLine("BRED TIM");
        // PropertyInfo espernam = typeof(Sprite).GetProperty("texture", BindingFlags.Instance | BindingFlags.Public);
        // if (espernam == null){
        //     Console.WriteLine("Canbdnna espernam spriite.");
        //     return true;
        // }
        // espernam.SetValue(shadow_sprite, starTexture);
        
        //shadow_sprite.texture = my_custom_texture;
        return true;
        // replace the shadow sprite.


        //Vector3 newPosition = ((Component)__instance).transform.position;
        //newPosition.x += (float)Math.Sin(Time.time * 8.0) * 0.1f;
        //((Component)__instance).transform.position = newPosition;
        

        /*
[Info   :   Console] Minitialized
[Info   :   Console] BRED TIM
[Error  : Unity Log] NullReferenceException: Object reference not set to an instance of an object
Stack trace:
RiftAllseer.Plugin.NewHooves (RhythmRift.Enemies.RREnemy& __instance) (at <85f28e56952f4189ab1c6728c06c5026>:0)
(wrapper dynamic-method) RhythmRift.Enemies.RREnemy.DMD<RhythmRift.Enemies.RREnemy::Initialize>(RhythmRift.Enemies.RREnemy,RhythmRift.RREnemyInitializationData,UnityEngine.AnimationCurve,Shared.RhythmEngine.FmodTimeCapsule,bool)
RhythmRift.Enemies.RRSkeletonEnemy.Initialize (RhythmRift.RREnemyInitializationData enemyInitializationData, UnityEngine.AnimationCurve defaultMovementCurve, Shared.RhythmEngine.FmodTimeCapsule fmodTimeCapsule, System.Boolean shouldDisableMovementAnimations) (at <6c554ed27fa1478db4b343d56024cb45>:0)
RhythmRift.RREnemyController.SpawnEnemy (RhythmRift.SpawnEnemyData spawnEnemyData, System.Guid groupId, Shared.RhythmEngine.FmodTimeCapsule fmodTimeCapsule, Unity.Mathematics.int2 spawnGridPosition) (at <6c554ed27fa1478db4b343d56024cb45>:0)
(wrapper dynamic-method) RhythmRift.RREnemyController.DMD<RhythmRift.RREnemyController::SpawnEnemy>(RhythmRift.RREnemyController,RhythmRift.SpawnEnemyData,System.Guid,Shared.RhythmEngine.FmodTimeCapsule)
RhythmRift.RRStageController.HandleEnemySpawnBeatEvent (RhythmRift.SpawnEnemyData spawnEnemyData) (at <6c554ed27fa1478db4b343d56024cb45>:0)
RhythmRift.RRBeatmapPlayer.ProcessBeatEvent (System.Single currentTime, Shared.RhythmEngine.BeatmapEvent beatEvent, System.Boolean isAddedEvent) (at <6c554ed27fa1478db4b343d56024cb45>:0)
Shared.RhythmEngine.BeatmapPlayer.ProcessBeatEvents (System.Single currentTime) (at <6c554ed27fa1478db4b343d56024cb45>:0)
Shared.RhythmEngine.BeatmapPlayer.Update () (at <6c554ed27fa1478db4b343d56024cb45>:0)
        */
    }


    // [HarmonyPatch(typeof(RhythmRift.RREnemyInitializationData), "SetData")]
    // [HarmonyPrefix]
    // static bool SetData(
    //     ref RREnemyDefinition enemyDefinition,
    //     bool shouldStartFacingRight, float spawnTrueBeatNumber,
    //     int2 gridPosition, Vector3 worldPosition, int enemyLength,
    //     Guid groupId, int itemToDropOnDeathId, bool shouldIgnoreForTutorialSuccess,
    //     bool shouldClampToSubdivisions = true
	// ) {
    //     Console.WriteLine($"setenemy! {enemyDefinition.DisplayName} {worldPosition}");
    //     return true;
    // }

    /*
    [HarmonyPatch(typeof(Shared.RhythmEngine.BeatmapPlayer), "SetSongSpeedModifier")]
    [HarmonyPrefix]
    static bool ForceDoubleTime(ref BeatmapPlayer __instance){
        Console.WriteLine("Forcing Double Time :)");
        typeof(BeatmapPlayer).GetField("_activeSpeedAdjustment", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, 2.0d);

        return true;
    }

    [HarmonyPatch(typeof(Shared.RhythmEngine.BeatmapPlayer), "Awake")]
    [HarmonyPrefix]
    static void Awake(ref BeatmapPlayer __instance){
        Console.WriteLine("Awaken and BIND!");
        const double newSpeed = 0.5d;
        typeof(BeatmapPlayer).GetField("_activeSpeedAdjustment", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(__instance, newSpeed);

        // Or, if you know the field's type:
        // __instance.GetType().GetField("yourPrivateVariableName", BindingFlags.Instance | BindingFlags.NonPublic).Se
    }

    [HarmonyPatch(typeof(Shared.RhythmEngine.BeatmapPlayer), "SetBeatmapInternal")]
    [HarmonyPrefix]
    static bool SetBeatmapInternal(ref Beatmap beatmapToSet){
        Console.WriteLine("overrideing internal bpm speed.");
        beatmapToSet.bpm *= 2;
        return true;
    }
    //*/

    //     Awake()

    // check why feedback no worky 
    // seemed to work fine? idk.
    // [HarmonyPatch(typeof(Shared.Feedback.FeedbackController), "Update")]
    // [HarmonyPrefix]
    // static bool OnFeedbackUpdate(Shared.Feedback.FeedbackController __instance){
    //     Type type = __instance.GetType();
    //     FieldInfo fieldInfo = type.GetField("_input", BindingFlags.NonPublic | BindingFlags.Instance);
    //     RiftInputActions _input = (RiftInputActions)fieldInfo.GetValue(__instance);

    //     if (_input != null){
    //         Console.WriteLine($"{__instance.IsShowingFeedbackScreen} {_input.UI.OpenFeedback.WasPerformedThisFrame()} {_input.Gameplay.OpenFeedback.WasPerformedThisFrame()} {_input.Debug.OpenFeedback.WasPerformedThisFrame()}");
    //     } else {
    //         Console.WriteLine($"{__instance.IsShowingFeedbackScreen} cannot find :( ");
    //     }
    //     return true;
    // }

    /*
    EnemyController UpdateSystem


    */

    
}