using Assets.Behaviour.Flow;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        internal static float FloatingLevel(WorldFlowCamera view) =>
            States.TryGetValue(view, out var state) ? state.FloatLevel : view.ZoomLevel;
    }
}
