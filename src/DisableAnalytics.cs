using System;

using HarmonyLib;
using Shared.Analytics;

namespace RiftAllseer {
    class DisableAnalytics {

        [HarmonyPatch(typeof(RiftAnalyticsService), "SendAnalyticsEvent")]
        [HarmonyPrefix]
        static bool DisableAnalyticsPatch(ref string tableName, ref string dataJsonString, bool shouldLogResponse = true){
            
            if (Plugin.shouldLogAnalytics.Value) {
                Console.Write("Analytics send event triggered:");
                Console.WriteLine(tableName);
                Console.WriteLine(dataJsonString);
            }

            if (Plugin.shouldDisableAnalytics.Value){
                return false;
            }
            return true;
        }
    }
}