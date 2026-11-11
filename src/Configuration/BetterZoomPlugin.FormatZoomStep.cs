using UnityEngine;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        private static string FormatZoomStep(float value) =>
            Mathf.Approximately(value, 1.0f) ? "1.00 (Standard)" : $"{value:0.00} (Fine)";
    }
}
