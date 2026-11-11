using Assets.Behaviour.Flow;

namespace KingdomBetterZoom
{
    internal static partial class SetZoomPatch
    {
        private static void Postfix(WorldFlowCamera __instance) => BetterZoomPlugin.CoverBackground(__instance);
    }
}
