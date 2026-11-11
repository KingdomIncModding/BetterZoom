using System;
using Assets.Behaviour.Flow;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomBetterZoom
{
    [HarmonyPatch(typeof(WorldFlowCamera), nameof(WorldFlowCamera.ChangeZoomLevel))]
    internal static class ChangeZoomPatch
    {
        private static bool Prefix(WorldFlowCamera __instance, int diff)
        {
            if (diff == 0)
                return false;

            float step = Mathf.Clamp(BetterZoomPlugin.ZoomStep.Value, 0.05f, 1f);
            float min = BetterZoomPlugin.Unlocked ? -BetterZoomPlugin.ExtraOut.Value : 0;
            float max = __instance.MaxZoom + (BetterZoomPlugin.Unlocked ? BetterZoomPlugin.ExtraIn.Value : 0);
            float current = Mathf.Clamp(BetterZoomPlugin.FloatingLevel(__instance), min, max);
            float next = Mathf.Clamp(current + Math.Sign(diff) * step, min, max);
            if (Mathf.Approximately(next, current))
                return false;

            var camera = BetterZoomPlugin.CameraField(__instance);
            Vector2 cursor = Mouse.current != null ? Mouse.current.position.ReadValue() :
                new Vector2(Screen.width / 2f, Screen.height / 2f);
            var before = camera.ScreenToWorldPoint(new Vector3(cursor.x, cursor.y, 0));

            BetterZoomPlugin.ApplyZoom(__instance, next);

            var after = camera.ScreenToWorldPoint(new Vector3(cursor.x, cursor.y, 0));
            camera.transform.position += new Vector3(before.x - after.x, before.y - after.y, 0);
            BetterZoomPlugin.CoverBackground(__instance);
            return false;
        }
    }
}
