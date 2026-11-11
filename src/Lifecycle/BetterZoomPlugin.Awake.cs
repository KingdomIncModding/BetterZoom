using HarmonyLib;
using KingdomFoundation.Api;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        private void Awake()
        {
            context = FoundationApi.ForMod(Id);
            RegisterResearch();
            BindConfiguration();
            RegisterSettings();

            harmony = new Harmony(Id);
            harmony.PatchAll(typeof(BetterZoomPlugin).Assembly);
            Logger.LogInfo("Better Zoom ready.");
        }
    }
}
