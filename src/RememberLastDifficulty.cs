
using System;
using System.Reflection;
using HarmonyLib;
using Shared;
using Shared.Title;
using Shared.PlayerData;

namespace RiftAllseer {
    public class RememberLastDifficulty{
        /* 
        For some reason the last difficulty you selected in 'play' is overridden when you enter the main menu.
        This just ensures it doesn't get overwritten;
        */
        public static Difficulty lastDifficulty = Difficulty.Medium;

        [HarmonyPatch(typeof(MainMenuManager), "Awake")]
        [HarmonyPrefix]
        public static bool GetLastLoadedDifficulty()
        {
            if (PlayerSaveController.Instance != null){
                lastDifficulty = PlayerSaveController.Instance.GetSelectedArcadeDifficulty();
            }
            return true;
        }


        [HarmonyPatch(typeof(MainMenuManager), "Awake")]
        [HarmonyPostfix]
        public static void SetLastLoadedDifficulty()
        {
            PlayerSaveController.Instance.SetSelectedArcadeDifficulty(lastDifficulty);
        }
    }
}