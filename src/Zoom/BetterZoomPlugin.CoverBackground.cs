using Assets.Behaviour.Flow;
using UnityEngine;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        internal static void CoverBackground(WorldFlowCamera view)
        {
            var background = BackgroundField(view);
            if (!background || !background.enabled || !background.sprite)
                return;

            var camera = CameraField(view);
            var state = States.GetOrCreateValue(view);
            if (!state.HasBackground)
            {
                state.BackgroundScale = background.transform.localScale;
                state.HasBackground = true;
            }

            // Expand the existing tiled backdrop instead of spawning more tiles.
            background.transform.localScale = state.BackgroundScale;
            var position = camera.transform.position;
            int period = Mathf.Max(1, BackgroundPeriod(view));
            background.transform.position = new Vector3(
                position.x - position.x % period,
                position.y - position.y % period,
                background.transform.position.z);

            var bounds = background.bounds;
            float halfHeight = camera.orthographicSize;
            float width = 2f * (halfHeight * camera.aspect + Mathf.Abs(position.x - bounds.center.x) + period);
            float height = 2f * (halfHeight + Mathf.Abs(position.y - bounds.center.y) + period);
            float coverage = Mathf.Max(1f,
                width / Mathf.Max(0.01f, bounds.size.x),
                height / Mathf.Max(0.01f, bounds.size.y));

            int factor = Mathf.NextPowerOfTwo(Mathf.CeilToInt(coverage));
            background.transform.localScale = new Vector3(
                state.BackgroundScale.x * factor,
                state.BackgroundScale.y * factor,
                state.BackgroundScale.z);
        }
    }
}
