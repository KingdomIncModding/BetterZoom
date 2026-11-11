using Assets.Source.Player;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        private void RegisterResearch()
        {
            context.Register(new TechNodeDefinition(
                ResearchId,
                "Better Zoom",
                Id,
                "Better Zoom",
                new Vector2(800, 0),
                "Zoom further out and in in the workshop and research tree. Unlocks three additional steps in each direction by default.",
                research: new TechNodeOptions(
                    nodeType: TechNodeType.Utility,
                    fixedCost: new CurrencyCollection(Currency.Gold, 100))));
        }
    }
}
