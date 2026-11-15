using KingdomFoundation.Api;
namespace KingdomBetterZoom
{
    internal static class CameraUpdatePatch
    {
        internal static void After(GameHookCall call)
        {
            if (ZoomController.IsSuspended(call.View))
                return;
            ZoomController.CoverBackground(call.View);
        }
    }
}
