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
            ZoomController.States.GetOrCreateValue(call.View).FloatLevel = level;
        }

        internal static void After(GameHookCall call) => ZoomController.CoverBackground(call.View);
    }
}
