using UnityEngine;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        internal static float ZoomScale(float level) => Mathf.Pow(2f, level - 2f);
    }
}
