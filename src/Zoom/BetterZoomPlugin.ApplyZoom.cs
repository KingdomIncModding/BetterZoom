using Assets.Behaviour.Flow;
using Assets.Behaviour.UI.Shared;
using UnityEngine;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        internal static void ApplyZoom(WorldFlowCamera view, float level)
        {
            float min = Unlocked ? -ExtraOut.Value : 0f;
            float max = view.MaxZoom + (Unlocked ? ExtraIn.Value : 0);
            level = Mathf.Clamp(level, min, max);

            // The game persists its original integer level. Fractional or extended
            // levels only affect the live camera, never the kingdom save format.
            view.SetZoomLevel(Mathf.Clamp(Mathf.RoundToInt(level), 0, view.MaxZoom));
            States.GetOrCreateValue(view).FloatLevel = level;
            CameraField(view).orthographicSize = Screen.height / ZoomScale(level);

            CoverBackground(view);
            FontFilteringSwitch.UpdateCameraZoom(view);
        }
    }
}
