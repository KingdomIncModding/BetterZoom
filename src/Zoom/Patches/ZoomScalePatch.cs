using KingdomFoundation.Api;
namespace KingdomBetterZoom
{
    internal static class ZoomScalePatch
    {
        internal static void After(GameHookCall call) => call.SetResult(ZoomController.ZoomScale(ZoomController.FloatingLevel(call.View)));
    }
}
