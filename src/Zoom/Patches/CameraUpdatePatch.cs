using Assets.Behaviour.Flow;
using HarmonyLib;

namespace KingdomBetterZoom
{
    [HarmonyPatch(typeof(WorldFlowCamera), "Update")]
    internal static class CameraUpdatePatch
    {
        private static void Postfix(WorldFlowCamera __instance) => BetterZoomPlugin.CoverBackground(__instance);
    }
}
