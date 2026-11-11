using BepInEx.Configuration;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        private void BindConfiguration()
        {
            ExtraOut = Config.Bind("Zoom", "ExtraZoomOutLevels", 3,
                new ConfigDescription("Extra wheel steps beyond the normal farthest view. Each step shows twice the width and height.",
                    new AcceptableValueRange<int>(0, 5)));

            ExtraIn = Config.Bind("Zoom", "ExtraZoomInLevels", 3,
                new ConfigDescription("Extra wheel steps beyond the normal closest view. Each step doubles magnification.",
                    new AcceptableValueRange<int>(0, 5)));

            ZoomStep = Config.Bind("Zoom", "ZoomStep", 1.0f,
                new ConfigDescription("Zoom step increment per wheel tick. 1.0 is full vanilla level (2x scale change); lower values (e.g. 0.25) provide fine-tuned zooming.",
                    new AcceptableValueRange<float>(0.05f, 1.0f)));
        }
    }
}
