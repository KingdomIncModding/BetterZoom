using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomBetterZoom
{
    internal static class SetZoomPatch
    {
        internal static void Before(GameHookCall call)
        {
            int level = Mathf.Clamp(call.Argument<int>(0), 0, 4);
            call.SetArgument(0, level);
            if (!ZoomController.IsSuspended(call.View))
                ZoomController.States.GetOrCreateValue(call.View).FloatLevel = level;
        }

        internal static void After(GameHookCall call)
        {
            if (ZoomController.IsSuspended(call.View))
                return;
            ZoomController.CoverBackground(call.View);
        }
    }
}
