using KingdomFoundation.Api;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomBetterZoom
{
    internal static class ChangeZoomPatch
    {
        internal static void Before(GameHookCall call)
        {
            var view = call.View;
            int diff = call.Argument<int>(0);
            call.Cancel();
            if (diff == 0)
                return;

            float step = Mathf.Clamp(ZoomConfiguration.ZoomStep.Value, 0.05f, 1f);
            float min = ZoomResearch.Unlocked ? -ZoomConfiguration.ExtraOut.Value : 0;
            float max = FoundationApi.CameraAccess.MaximumZoom(view) + (ZoomResearch.Unlocked ? ZoomConfiguration.ExtraIn.Value : 0);
            float current = Mathf.Clamp(ZoomController.FloatingLevel(view), min, max);
            float next = Mathf.Clamp(current + Math.Sign(diff) * step, min, max);
            if (Mathf.Approximately(next, current))
                return;

            var camera = FoundationApi.CameraAccess.Camera(view);
            Vector2 cursor = Mouse.current != null ? Mouse.current.position.ReadValue() :
                new Vector2(Screen.width / 2f, Screen.height / 2f);
            var before = camera.ScreenToWorldPoint(new Vector3(cursor.x, cursor.y, 0));

            ZoomController.ApplyZoom(view, next);

            var after = camera.ScreenToWorldPoint(new Vector3(cursor.x, cursor.y, 0));
            camera.transform.position += new Vector3(before.x - after.x, before.y - after.y, 0);
            ZoomController.CoverBackground(view);
            return;
        }
    }
}
