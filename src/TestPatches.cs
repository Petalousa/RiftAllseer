using HarmonyLib;
using RhythmRift;
using System;
using System.Reflection;
using UnityEngine;
using RhythmRift.Enemies;
using System.Diagnostics;
using Shared.RhythmEngine;
using Shared;
using UnityEngine.SceneManagement;
using Shared.MenuOptions;
using System.Collections.Generic;

namespace RiftAllseer {
    class TestPatches {
        [HarmonyPatch(typeof(RREnemyController), "SpawnEnemy", new Type[] {typeof(SpawnEnemyData), typeof(Guid), typeof(FmodTimeCapsule)})]
        [HarmonyPrefix]
        static bool AlertEnemySpawn() {
            //Console.WriteLine("enemy spawned");
            return true; // Execute original method
        }


        // [HarmonyPatch(typeof(RhythmRift.RRTileView), "Awake")]
        // [HarmonyPostfix]
        // static void CreateTile(ref MaterialPropertyBlock ____materialPropertyBlock){
        //     Console.WriteLine($"Tile AWAKE! {____materialPropertyBlock}");
        //     Vector3 newPosition = ____materialPropertyBlock.GetVector("_Position");
        //     newPosition.x -= 100.0f;
        //     newPosition.y += 500.0f;
        //     newPosition.z += 700.0f;
        //     ____materialPropertyBlock.SetVector("_Position", newPosition);
        // }

        [HarmonyPatch(typeof(RhythmRift.RRGridView), "GetTileWorldPositionFromGridPosition")]
        [HarmonyPostfix]
        static void GetTileWorldPositionFromGridPosition(int xCoordinate, int yCoordinate, ref Vector3 __result){
            
            float z_offset = 0.0f;
            float y_offset = 0.0f;
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

        static void wiggleInstance(ref RREnemy __instance){
            Vector3 newPosition = ((Component)__instance).transform.position;
            newPosition.x += (float)Math.Sin(Time.time * 8.0) * 0.1f;
            ((Component)__instance).transform.position = newPosition;
        }

        [HarmonyPatch(typeof(RREnemy), "Initialize")]
        [HarmonyPostfix]
        static void SmooothMovement(ref RREnemy __instance){
            FieldInfo mcurv = typeof(RREnemy).GetField("_movementCurve", BindingFlags.Instance | BindingFlags.NonPublic);
            AnimationCurve _movementCurve = (AnimationCurve) mcurv.GetValue(__instance);
            _movementCurve.SetKeys(
                [
                    new Keyframe(0.0f, 0.0f),
                    new Keyframe(0.75f, 0.0f),
                    new Keyframe(0.95f, 1.0f)
                ]
            );
        }

        [HarmonyPatch(typeof(RREnemy), "UpdateMovement")]
        [HarmonyPrefix]
        static bool Whoop(ref RREnemy __instance, ref FmodTimeCapsule fmodTimeCapsule){
            MethodInfo m = typeof(RREnemy).GetMethod("GetNormalizedProgressToNextMove", BindingFlags.Instance | BindingFlags.NonPublic);

            float normalizedProgressToNextMove = (float)m.Invoke(__instance, new object[] {fmodTimeCapsule} );
            FieldInfo mcurv = typeof(RREnemy).GetField("_movementCurve", BindingFlags.Instance | BindingFlags.NonPublic);
            AnimationCurve _movementCurve = (AnimationCurve) mcurv.GetValue(__instance);
            float num5 = _movementCurve.Evaluate(normalizedProgressToNextMove);
            Console.Write($"{__instance.name}, {__instance.DisplayName} {__instance.EnemyId}- {num5}- {normalizedProgressToNextMove}");
            // Type typeInfo = typeof(RREnemy);
            // MemberInfo[] members = typeInfo.GetMembers(); // Get all members
            // foreach (var member in members)
            // {
            //     Console.WriteLine($"{member.MemberType}: {member.Name}");
            // }
            PropertyInfo isSnapProp = typeof(RREnemy).GetProperty("IsSnappingToActionRow", BindingFlags.Instance | BindingFlags.NonPublic);
            bool IsSnappingToActionRow = (bool)isSnapProp.GetValue(__instance);

            if (IsSnappingToActionRow){
                // seems to occur w/ blue slimes?
                // not sure about yellow ones

                // Console.Write($"is_snapping - {__instance.DisplayName}");
                // wiggleInstance(ref __instance);
                // return false;
            }

            FieldInfo f = typeof(RREnemy).GetField("_specialActionMoveCurve", BindingFlags.Instance | BindingFlags.NonPublic);
            AnimationCurve _specialActionMoveCurve = (AnimationCurve)f.GetValue(__instance);
            if (__instance.IsPerformingSpecialActionMovement && f != null){
                Console.Write($"is_specialmove - {__instance.DisplayName}");
                wiggleInstance(ref __instance);
            }

            return true;
            //Vector3 newPosition = ((Component)__instance).transform.position;
            //newPosition.x += (float)Math.Sin(Time.time * 8.0) * 0.1f;
            //((Component)__instance).transform.position = newPosition;
            
        }
        [HarmonyPatch(typeof(RREnemy), "Initialize")]
        [HarmonyPrefix]
        static bool NewHooves(ref RREnemy __instance, ref AnimationCurve defaultMovementCurve){
            //Console.WriteLine("Minitialized");

            FieldInfo overrideMoveCurve = typeof(RREnemy).GetField("_shouldOverrideDefaultMoveCurve", BindingFlags.Instance | BindingFlags.NonPublic);
            if (overrideMoveCurve != null){
                overrideMoveCurve.SetValue(__instance, true);
            } else {
                Console.Write("failed to find move curve.");
            }
            //FieldInfo otherBeatField = typeof(RREnemy).GetField("_otherBeatShadowSprite", BindingFlags.Instance | BindingFlags.NonPublic);
            defaultMovementCurve.SetKeys(
                [
                    new Keyframe(0.0f, 0.0f),
                    new Keyframe(0.5f, 1.0f),
                    new Keyframe(0.6f, 0.0f),
                    new Keyframe(1f, 1.0f)
                ]
            );

            // FieldInfo curvy1 = typeof(RREnemy).GetField("_enemyMovementCurve", BindingFlags.Instance | BindingFlags.NonPublic);
            // FieldInfo curvy2 = typeof(RREnemy).GetField("_movementCurve", BindingFlags.Instance | BindingFlags.NonPublic);
            // if (curvy1 == null){
            //     Console.Write("no curves");
            //     return true;
            // }
            // AnimationCurve c1 = (AnimationCurve)curvy1.GetValue(__instance);
            

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

        //     var scene = ((GameObject)__instance).scene;
        //     string sceneName = scene.isLoaded ? scene.name : "Unknown";

        //     // If you need to access more internal details (e.g., root object name), you could use reflection
        //     var rootObject = scene.GetRootGameObjects();
        //     foreach (var root in rootObject)
        //     {
        //         // Assuming we want the name of the root object
        //         Console.Write($"Object '{__instance.name}' is a child of the scene root: {root.name}");
        //     }

        //     // Log the scene information
        //     Console.Write($"Object '{__instance.name}' is in scene: {sceneName}");
        //     return true;
        // }

        [HarmonyPatch(typeof(Shared.Feedback.FeedbackSpawner), "LoadRoutine")]
        [HarmonyPrefix]
        static bool onfeedbackl(Shared.Feedback.FeedbackSpawner __instance){
            Scene activeSceneBefore = SceneManager.GetActiveScene();
            Console.WriteLine($"ACTIVE SCENE {activeSceneBefore}");
            Console.WriteLine($"ACTIVE SCENE {activeSceneBefore.name}");
            return true;
        }

        [HarmonyPatch(typeof(Shared.Feedback.FeedbackSpawner), "LoadRoutine")]
        [HarmonyPostfix]
        static void onfeedbackAPRES(Shared.Feedback.FeedbackSpawner __instance){
            Scene activeSceneBefore = SceneManager.GetActiveScene();
            Console.WriteLine($"AFTER ACTIVE SCENE {activeSceneBefore}");
            Console.WriteLine($"AFTER ACTIVE SCENE {activeSceneBefore.name}");
        }



        [HarmonyPatch(typeof(Shared.Feedback.FeedbackController), "OnDestroy")]
        [HarmonyPrefix]
        static bool OnFeedbackDestroy(Shared.Feedback.FeedbackController __instance){
            StackTrace stackTrace = new StackTrace(true);           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();
            Console.WriteLine($"a {stackFrames.Length}");
            foreach (StackFrame f in stackFrames){
                Console.WriteLine($"{f}");
            }



            var originalCallerMethod = stackFrames[1].GetMethod();
            Console.Write($"{originalCallerMethod.DeclaringType.FullName}.{originalCallerMethod.Name}");
            Console.WriteLine("THEY KILLED ME");
            return true;
        }

        [HarmonyPatch(typeof(Shared.Feedback.FeedbackController), "Close")]
        [HarmonyPrefix]
        static bool OnFeedbackClose(Shared.Feedback.FeedbackController __instance){
            
            Console.WriteLine("THEY Close ME");
            return true;
        }
        

        /*
        EnemyController UpdateSystem


        */

        // set default difficulty
        // [HarmonyPatch(typeof(Shared.TrackSelection.DifficultySelectionOptionGroup), "Initialize")]
        // [HarmonyPrefix]
        // static bool InitializeDifficultySelector(ref Difficulty selectedDifficulty){
        //     Console.Write($"diff sel start - {selectedDifficulty}");
        //     selectedDifficulty = Difficulty.Impossible;
        //     return true;
        // this causes wierd UI issues with 'play' bg.
        // }
        // [HarmonyPatch(typeof(Shared.TrackSelection.DifficultySelectionOptionGroup), "Initialize")]
        // [HarmonyPostfix]
        // static void CallDiffOnChange(ref Shared.TrackSelection.DifficultySelectionOptionGroup __instance){

        //     // MethodInfo [] ms = typeof(Shared.TrackSelection.DifficultySelectionOptionGroup).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
        //     // foreach( MethodInfo m1 in ms){
        //     //     Console.Write($"{m1.Name}");
        //     // }
        //     // return;
        //     //EventInfo type = typeof(Shared.TrackSelection.DifficultySelectionOptionGroup).GetEvent("OnDifficultyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        //     FieldInfo selField = typeof(Shared.TrackSelection.DifficultySelectionOptionGroup).GetField("_selectionIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        //     PropertyInfo sel2Field = typeof(Shared.TrackSelection.DifficultySelectionOptionGroup).GetProperty("LastOptionIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        //     int _selectionIndex = (int)selField.GetValue(__instance);
        //     int _selectionIndexBase = (int)sel2Field.GetValue(__instance);

        //     FieldInfo opField = typeof(Shared.TrackSelection.DifficultySelectionOptionGroup).GetField("_options", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        //     List<SelectableOption> _options = (List<SelectableOption>)opField.GetValue(__instance);
        //     Console.Write($"{_selectionIndex} - {_selectionIndexBase}");
        //     //_options[1].SetSelected(false, false);

        //     // ((SelectableOption)_options[1]).SetSelected(false, false);
        //     // ((SelectableOption)_options[3]).SetSelected(true, false);

        //     // _options[lastSelectionIndex].SetSelected(value: false, isMovingUpLeft);
        //     // _options[_selectionIndex].SetSelected(value: true, isMovingUpLeft);


        //     FieldInfo backingField = typeof(Shared.TrackSelection.DifficultySelectionOptionGroup).GetField("OnDifficultyChanged", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        //     Delegate eventDelegate = (Delegate)backingField.GetValue(__instance);
        //     //GetMethod("OnDifficultyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        //     //eventDelegate.DynamicInvoke([Difficulty.Impossible]);
        //     Console.Write($"post diff sel start ");
        //     // Delegate d = (Delegate)
        //     // m.Get(__instance, [Difficulty.Impossible]);

        // }
    }
}