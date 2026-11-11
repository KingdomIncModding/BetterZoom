using Assets.Behaviour.Flow;
using HarmonyLib;

namespace KingdomBetterZoom
{
    [HarmonyPatch(typeof(WorldFlowCamera), nameof(WorldFlowCamera.ZoomScale), MethodType.Getter)]
    internal static class ZoomScalePatch
    {
        private static void Postfix(WorldFlowCamera __instance, ref float __result) =>
            __result = BetterZoomPlugin.ZoomScale(BetterZoomPlugin.FloatingLevel(__instance));
    }
}
