using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomBetterZoom
{
    internal static class ZoomResearch
    {
        internal static bool Unlocked => FoundationApi.World.IsInKingdom && FoundationApi.Progression.HasTech(BetterZoomPlugin.ResearchId);

        internal static void Register(FoundationModContext context)
        {
            context.Register(new TechNodeDefinition(BetterZoomPlugin.ResearchId, "Better Zoom", BetterZoomPlugin.Id, "Better Zoom", new Vector2(800, 0), "Zoom further out and in in the workshop and research tree. Unlocks three additional steps in each direction by default.", research: TechNodeOptions.Create(nodeKind: "Utility", fixedCost: new GameMoney("Gold", 100))));
        }
    }
}
