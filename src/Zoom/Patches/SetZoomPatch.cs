using Assets.Behaviour.Flow;
using HarmonyLib;
using UnityEngine;

namespace KingdomBetterZoom
{
    [HarmonyPatch(typeof(WorldFlowCamera), nameof(WorldFlowCamera.SetZoomLevel))]
    internal static partial class SetZoomPatch
    {
        private static void Prefix(WorldFlowCamera __instance, ref int lv)
        {
            lv = Mathf.Clamp(lv, 0, 4);
            BetterZoomPlugin.States.GetOrCreateValue(__instance).FloatLevel = lv;
        }
    }
}
