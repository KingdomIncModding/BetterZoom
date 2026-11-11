using Assets.Behaviour.Flow;
using UnityEngine;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        private void OnDestroy()
        {
            // Return live cameras and the tiled backdrop to their native state.
            foreach (var view in Resources.FindObjectsOfTypeAll<WorldFlowCamera>())
            {
                if (!States.TryGetValue(view, out var state))
                    continue;

                view.SetZoomLevel(view.ZoomLevel);
                var background = BackgroundField(view);
                if (background && state.HasBackground)
                    background.transform.localScale = state.BackgroundScale;
            }

            harmony?.UnpatchSelf();
            context?.Dispose();
        }
    }
}
