using BepInEx.Configuration;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomBetterZoom
{
    internal static class ZoomConfiguration
    {
        internal static ConfigEntry<int> ExtraOut;
        internal static ConfigEntry<int> ExtraIn;
        internal static ConfigEntry<float> ZoomStep;
        internal static void BindConfiguration(ConfigFile config)
        {
            ExtraOut = config.Bind("Zoom", "ExtraZoomOutLevels", 3, new ConfigDescription("Extra wheel steps beyond the normal farthest view. Each step shows twice the width and height.", new AcceptableValueRange<int>(0, 5)));
            ExtraIn = config.Bind("Zoom", "ExtraZoomInLevels", 3, new ConfigDescription("Extra wheel steps beyond the normal closest view. Each step doubles magnification.", new AcceptableValueRange<int>(0, 5)));
            ZoomStep = config.Bind("Zoom", "ZoomStep", 1.0f, new ConfigDescription("Zoom step increment per wheel tick. 1.0 is full vanilla level (2x scale change); lower values (e.g. 0.25) provide fine-tuned zooming.", new AcceptableValueRange<float>(0.05f, 1.0f)));
        }

        private static string FormatZoomStep(float value) => Mathf.Approximately(value, 1.0f) ? "1.00 (Standard)" : $"{value:0.00} (Fine)";
        internal static void RegisterSettings(FoundationModContext context)
        {
            var settings = context.Settings("Better Zoom", "Zoom further out and in in the workshop and research tree, with fine-tuned zoom steps.");
            settings.AddSlider(ZoomStep, 0.05f, 1.0f, 0.05f, "Zoom Step Size", FormatZoomStep);
            settings.AddSlider(ExtraOut, 0, 5, "Extra Zoom Out");
            settings.AddSlider(ExtraIn, 0, 5, "Extra Zoom In");
        }
    }
}
