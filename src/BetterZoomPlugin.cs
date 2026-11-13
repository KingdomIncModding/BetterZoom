using BepInEx;

namespace KingdomBetterZoom
{
    [BepInPlugin(Id, "Better Zoom", Version)]
    [BepInDependency("urakka.kingdominc.foundation", "0.14.0")]
    public sealed class BetterZoomPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.betterzoom";
        public const string Version = "1.2.1";
        public const string ResearchId = Id + ".unlock";
        private BetterZoomLifecycle lifecycle;
        private void Awake()
        {
            lifecycle = new BetterZoomLifecycle();
            lifecycle.Start(Config, Logger);
        }

        private void OnDestroy() => lifecycle?.Dispose();
    }
}
