using System;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomBetterZoom
{
    /// <summary>Optional public integration API for coordinating external camera captures with Better Zoom.</summary>
    public static class BetterZoomApi
    {
        private sealed class CaptureScope : IDisposable
        {
            private readonly Component view;
            private readonly ZoomState state;
            private readonly float savedFloatLevel;
            private bool disposed;

            internal CaptureScope(Component view, ZoomState state, float savedFloatLevel)
            {
                this.view = view;
                this.state = state;
                this.savedFloatLevel = savedFloatLevel;
            }

            public void Dispose()
            {
                if (disposed)
                    return;
                disposed = true;
                if (state.SuspensionDepth > 0)
                    state.SuspensionDepth--;
                if (view && state.SuspensionDepth == 0)
                    ZoomController.ApplyZoom(view, savedFloatLevel);
            }
        }

        /// <summary>Suspends Better Zoom mutations for <paramref name="cameraView"/> during capture and restores its saved floating level on disposal.</summary>
        public static IDisposable SuspendForCapture(Component cameraView)
        {
            if (!cameraView)
                return null;
            var state = ZoomController.States.GetOrCreateValue(cameraView);
            float saved = ZoomController.FloatingLevel(cameraView);
            state.SuspensionDepth++;
            return new CaptureScope(cameraView, state, saved);
        }

        /// <summary>Gets the current floating zoom level tracked for <paramref name="cameraView"/>.</summary>
        public static float GetFloatingZoomLevel(Component cameraView)
            => cameraView ? ZoomController.FloatingLevel(cameraView) : 0f;

        /// <summary>Applies and clamps <paramref name="floatLevel"/> for <paramref name="cameraView"/>.</summary>
        public static void SetFloatingZoomLevel(Component cameraView, float floatLevel)
        {
            if (cameraView)
                ZoomController.ApplyZoom(cameraView, floatLevel);
        }
    }
}

