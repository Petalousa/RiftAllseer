using System.Reflection;
using HarmonyLib;
using Shared.MenuOptions;
using Shared.Title;
using System;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.SceneManagement;

namespace RiftAllseer {
    class CustomMenuOptions {

        public static TextButtonOption modOptions;
        public static SelectableOptionGroup modOptionsGroup;

        [HarmonyPatch(typeof(SettingsAccessor), "RequestSettingsMenu")]
        [HarmonyPostfix]
        public static void RequestSettingsMenu(ref SettingsMenuManager __result){
            // if (__result == null){ return; }

            // if (modOptionsGroup == null){
            //     modOptionsGroup = YoinkOG(__result.transform.gameObject);
            // }

            // if (modOptionsGroup == null){
            //     Console.Write("Couldn't find optiongroup :9");
            //     return;
            // }

            // //SelectableOption s1 = modOptionsGroup.GetOptionAtIndex(0);
            // Console.Write($"{modOptionsGroup.name} - modOptionsGroup");
            
            // FieldInfo f_settings_button = typeof(SettingsMenuManager).GetField("_returnButton", BindingFlags.NonPublic | BindingFlags.Instance);
            // TextButtonOption return_button = (TextButtonOption)f_settings_button.GetValue(__result);

            // Console.Write("FIELD :9");

            // FieldInfo f = typeof(TextButtonOption).GetField("_textLabels", BindingFlags.NonPublic | BindingFlags.Instance);
            // TextButtonOption m1 = TextButtonOption.Instantiate(return_button);
            // TMP_Text[] _textLabels = (TMP_Text[])f.GetValue(m1);
            // _textLabels[0].text = "BOB";

            // Console.Write("FIELD :8");

            // m1.gameObject.transform.SetParent(return_button.transform.parent);
            // modOptionsGroup.TryAddOption(m1);
            // Console.Write("FIELD :7");
            //LogGameObjectHierarchy(__result.transform.parent.gameObject, 0, 3);

            //Console.Write(__result.gameObject)
        }

        private static SelectableOptionGroup YoinkOG(GameObject obj){
            MonoBehaviour[] behaviours = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour.GetType() == typeof(SelectableOptionGroup)){
                    return (SelectableOptionGroup)behaviour;
                }
            }

            foreach (Transform child in obj.transform)
            {
                SelectableOptionGroup result = YoinkOG(child.gameObject);
                if (result != null){
                    return result;
                }
            }
            return null;
        }


            // if (__result == null){ return; }
        public static void IGNOREME(ref SettingsMenuManager __result){

            // Transform a = __result.transform.GetChild(0);
            // TextButtonOption t = (TextButtonOption)a.gameObject.GetComponent(typeof(TextButtonOption));
            // if (t == null){
            //     Console.Write("cannot find the textoptns");
            // }
            // t = (TextButtonOption)a.gameObject.GetComponentInChildren(typeof(TextButtonOption));
            // if (t == null){
            //     Console.Write("still cannot find the textoptns");
            //     return;
            // }

            // Console.Write($"found textoption - {t}");
            // Console.Write($"found next thing - {t.transform.parent.name}");
            // Console.Write($"found next thing up - {t.transform.parent.transform.parent.name}");
            // LogGameObjectHierarchy(t.transform.parent.transform.parent.gameObject, 0);

            // // Shared.MenuOptions.OptionsScreenInputController Shared.PauseScreen._inputController
            // // need to find a 
            // // SettingsAccessor.Instance.RequestSettingsMenu(SceneLoadingController.Instance.CurrentSceneName, base.transform);
            // // 

            // MonoBehaviour[] scripts = t.transform.parent.gameObject.GetComponentsInChildren<MonoBehaviour>();
            // foreach(MonoBehaviour s in scripts){
            //     Console.Write($"{s.name} - {s.GetScriptClassName()}");
            //}
            // return;

            // FieldInfo f = typeof(TextButtonOption).GetField("_textLabels", BindingFlags.NonPublic | BindingFlags.Instance);
            // TMP_Text[] _textLabels = (TMP_Text[])f.GetValue(t);

            // foreach (TMP_Text t1 in _textLabels){
            //     Console.Write($"Found {t1.text}");
            //     t1.text = "BETURNE TOO GAMEE";
            // }

            // modOptions = TextButtonOption.Instantiate(t);
            // modOptions.name = "AAAAAAA";
            // RectTransform r = modOptions.transform as RectTransform;
            // TMP_Text[] _textLabels2 = (TMP_Text[])f.GetValue(modOptions);
            // _textLabels2[0].SetText("Allseer Mod");
            
            // //g.TryAddOption(modOptions);

            // return;

            
            //r.SetParent(t.transform.parent);
            //TryAddOption(modOptions, int indexToAddAt = -1)
            //modOptions = new TextButtonOption();
        }
        private static void LogGameObjectHierarchy(GameObject obj, int indentLevel, int maxLevel=21)
        {
            // Create indentation based on the level of the hierarchy
            string indent = new string('-', indentLevel * 2);

            // Log the current GameObject's name and its type
            Console.Write($"{indent} {obj.name} (Type: {obj.GetType()})");
            MonoBehaviour[] behaviours = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                Console.Write($"{indent} {obj.name} <-- {behaviour.GetType().Name}");
            }
            if (indentLevel >= maxLevel){
                Console.Write($"{indent} {obj.name} -- max depth --");
                return;
            }
            // Recursively log all children of the current GameObject
            foreach (Transform child in obj.transform)
            {
                LogGameObjectHierarchy(child.gameObject, indentLevel + 1, maxLevel);
            }
        }
    }
}