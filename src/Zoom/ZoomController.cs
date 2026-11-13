using System.Runtime.CompilerServices;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomBetterZoom
{
    internal static class ZoomController
    {
        internal static readonly ConditionalWeakTable<Component, ZoomState> States = new ConditionalWeakTable<Component, ZoomState>();
        internal static void ApplyZoom(Component view, float level)
        {
            float min = ZoomResearch.Unlocked ? -ZoomConfiguration.ExtraOut.Value : 0f;
            float max = FoundationApi.CameraAccess.MaximumZoom(view) + (ZoomResearch.Unlocked ? ZoomConfiguration.ExtraIn.Value : 0);
            level = Mathf.Clamp(level, min, max);
            // The game persists its original integer level. Fractional or extended
            // levels only affect the live camera, never the kingdom save format.
            FoundationApi.CameraAccess.SetZoomLevel(view, Mathf.Clamp(Mathf.RoundToInt(level), 0, FoundationApi.CameraAccess.MaximumZoom(view)));
            States.GetOrCreateValue(view).FloatLevel = level;
            FoundationApi.CameraAccess.Camera(view).orthographicSize = Screen.height / ZoomScale(level);
            CoverBackground(view);
            FoundationApi.CameraAccess.RefreshFontFiltering(view);
        }

        internal static void CoverBackground(Component view)
        {
            var background = FoundationApi.CameraAccess.Background(view);
            if (!background || !background.enabled || !background.sprite)
                return;
            var camera = FoundationApi.CameraAccess.Camera(view);
            var state = States.GetOrCreateValue(view);
            if (!state.HasBackground)
            {
                state.BackgroundScale = background.transform.localScale;
                state.HasBackground = true;
            }

            // Expand the existing tiled backdrop instead of spawning more tiles.
            background.transform.localScale = state.BackgroundScale;
            var position = camera.transform.position;
            int period = Mathf.Max(1, FoundationApi.CameraAccess.BackgroundPeriod(view));
            background.transform.position = new Vector3(position.x - position.x % period, position.y - position.y % period, background.transform.position.z);
            var bounds = background.bounds;
            float halfHeight = camera.orthographicSize;
            float width = 2f * (halfHeight * camera.aspect + Mathf.Abs(position.x - bounds.center.x) + period);
            float height = 2f * (halfHeight + Mathf.Abs(position.y - bounds.center.y) + period);
            float coverage = Mathf.Max(1f, width / Mathf.Max(0.01f, bounds.size.x), height / Mathf.Max(0.01f, bounds.size.y));
            int factor = Mathf.NextPowerOfTwo(Mathf.CeilToInt(coverage));
            background.transform.localScale = new Vector3(state.BackgroundScale.x * factor, state.BackgroundScale.y * factor, state.BackgroundScale.z);
        }

        internal static float FloatingLevel(Component view) => States.TryGetValue(view, out var state) ? state.FloatLevel : FoundationApi.CameraAccess.ZoomLevel(view);
        internal static float ZoomScale(float level) => Mathf.Pow(2f, level - 2f);
    }
}
