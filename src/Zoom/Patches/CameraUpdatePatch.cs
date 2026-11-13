using KingdomFoundation.Api;
namespace KingdomBetterZoom
{
    internal static class CameraUpdatePatch
    {
        internal static void After(GameHookCall call) => ZoomController.CoverBackground(call.View);
    }
}
