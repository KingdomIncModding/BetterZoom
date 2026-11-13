using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BepInEx.Logging;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomBetterZoom
{
    internal sealed class BetterZoomLifecycle : IDisposable
    {
        private FoundationModContext context;
        private readonly List<IDisposable> hooks = new List<IDisposable>();
        internal void Start(ConfigFile config, ManualLogSource logger)
        {
            context = FoundationApi.ForMod(BetterZoomPlugin.Id);
            ZoomResearch.Register(context);
            ZoomConfiguration.BindConfiguration(config);
            ZoomConfiguration.RegisterSettings(context);
            hooks.Add(FoundationApi.GameHooks.Subscribe(BetterZoomPlugin.Id, GameHookPoints.CameraUpdate, after: CameraUpdatePatch.After));
            hooks.Add(FoundationApi.GameHooks.Subscribe(BetterZoomPlugin.Id, GameHookPoints.CameraChangeZoom, before: ChangeZoomPatch.Before));
            hooks.Add(FoundationApi.GameHooks.Subscribe(BetterZoomPlugin.Id, GameHookPoints.CameraSetZoom, before: SetZoomPatch.Before, after: SetZoomPatch.After));
            hooks.Add(FoundationApi.GameHooks.Subscribe(BetterZoomPlugin.Id, GameHookPoints.CameraZoomScale, after: ZoomScalePatch.After));
            logger.LogInfo("Better Zoom ready.");
        }

        public void Dispose()
        {
            // Return live cameras and the tiled backdrop to their native state.
            foreach (var view in FoundationApi.CameraAccess.Views)
            {
                if (!ZoomController.States.TryGetValue(view, out var state))
                    continue;
                FoundationApi.CameraAccess.SetZoomLevel(view, FoundationApi.CameraAccess.ZoomLevel(view));
                var background = FoundationApi.CameraAccess.Background(view);
                if (background && state.HasBackground)
                    background.transform.localScale = state.BackgroundScale;
            }

            foreach (var hook in hooks)
                hook.Dispose();
            hooks.Clear();
            context?.Dispose();
        }
    }
}
