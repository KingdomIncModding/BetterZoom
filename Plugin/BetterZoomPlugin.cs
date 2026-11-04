using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI.Shared;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomBetterZoom
{
    [BepInPlugin(Id, "Better Zoom", Version)]
    public sealed partial class BetterZoomPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.betterzoom";
        public const string Version = "1.0.1";
        static readonly ConditionalWeakTable<WorldFlowCamera, ZoomState> States = new ConditionalWeakTable<WorldFlowCamera, ZoomState>();
        static readonly AccessTools.FieldRef<WorldFlowCamera, Camera> CameraField = AccessTools.FieldRefAccess<WorldFlowCamera, Camera>("_camera");
        static readonly AccessTools.FieldRef<WorldFlowCamera, SpriteRenderer> BackgroundField = AccessTools.FieldRefAccess<WorldFlowCamera, SpriteRenderer>("_background");
        static readonly AccessTools.FieldRef<WorldFlowCamera, int> BackgroundPeriod = AccessTools.FieldRefAccess<WorldFlowCamera, int>("backgroundSize");
        static ConfigEntry<int> extraOut, extraIn;
        Harmony harmony;
        sealed class ZoomState
        {
            public int Level;
            public bool HasBackground;
            public Vector3 BackgroundScale;
        }
        static bool Diagnostic => Environment.GetCommandLineArgs().Contains("-betterzoom-smoke");

        void Awake()
        {
            extraOut = Config.Bind("Zoom", "ExtraZoomOutLevels", 3, new ConfigDescription("Extra wheel steps beyond the normal farthest view. Each step shows twice the width and height.", new AcceptableValueRange<int>(0, 5)));
            extraIn = Config.Bind("Zoom", "ExtraZoomInLevels", 3, new ConfigDescription("Extra wheel steps beyond the normal closest view. Each step doubles magnification.", new AcceptableValueRange<int>(0, 5)));
            harmony = new Harmony(Id);
            foreach (var type in new[] { typeof(ChangeZoom), typeof(ResetZoom), typeof(ActualScale), typeof(BackgroundCoverage) })
                harmony.CreateClassProcessor(type).Patch();
            if (Diagnostic)
            {
                foreach (var type in new[] { typeof(NoSave), typeof(NoAutosave), typeof(NoLoad), typeof(NoAchievement), typeof(Offline) })
                    harmony.CreateClassProcessor(type).Patch();
                Application.runInBackground = true;
                InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                StartCoroutine(RuntimeCheck());
            }
            Logger.LogInfo("Better Zoom ready: " + extraOut.Value + " extra zoom-out levels and " + extraIn.Value + " extra zoom-in levels.");
        }

        void OnDestroy()
        {
            // Restore a native view before removing the scale hook.
            foreach (var camera in Resources.FindObjectsOfTypeAll<WorldFlowCamera>())
                if (States.TryGetValue(camera, out var state))
                {
                    camera.SetZoomLevel(camera.ZoomLevel);
                    var background = BackgroundField(camera);
                    if (background && state.HasBackground) background.transform.localScale = state.BackgroundScale;
                }
            harmony?.UnpatchSelf();
        }

        internal static int Level(WorldFlowCamera camera) => States.TryGetValue(camera, out var state) ? state.Level : camera.ZoomLevel;
        internal static float Scale(int level) => Mathf.Pow(2f, level - 2);

        static void Apply(WorldFlowCamera camera, int level)
        {
            // Native levels stay in 0..4 for backgrounds, keyboard movement and saves.
            // The extended level belongs only to this live camera and is never serialized.
            camera.SetZoomLevel(Mathf.Clamp(level, 0, camera.MaxZoom));
            States.GetOrCreateValue(camera).Level = level;
            CameraField(camera).orthographicSize = Screen.height / Scale(level);
            CoverBackground(camera);
            FontFilteringSwitch.UpdateCameraZoom(camera);
        }

        static void CoverBackground(WorldFlowCamera view)
        {
            var background = BackgroundField(view);
            if (!background || !background.enabled || !background.sprite) return;
            var camera = CameraField(view);
            var state = States.GetOrCreateValue(view);
            if (!state.HasBackground)
            {
                state.BackgroundScale = background.transform.localScale;
                state.HasBackground = true;
            }
            // Keep the native tile count bounded rather than building millions of
            // sprite vertices at distant zoom. The backdrop becomes coarser there.
            background.transform.localScale = state.BackgroundScale;
            var position = camera.transform.position;
            int period = Mathf.Max(1, BackgroundPeriod(view));
            background.transform.position = new Vector3(position.x - position.x % period,
                position.y - position.y % period, background.transform.position.z);
            var bounds = background.bounds;
            float halfHeight = camera.orthographicSize;
            float requiredWidth = 2f * (halfHeight * camera.aspect + Mathf.Abs(position.x - bounds.center.x) + period);
            float requiredHeight = 2f * (halfHeight + Mathf.Abs(position.y - bounds.center.y) + period);
            float coverage = Mathf.Max(1f, requiredWidth / Mathf.Max(.01f, bounds.size.x), requiredHeight / Mathf.Max(.01f, bounds.size.y));
            int factor = Mathf.NextPowerOfTwo(Mathf.CeilToInt(coverage));
            background.transform.localScale = new Vector3(state.BackgroundScale.x * factor,
                state.BackgroundScale.y * factor, state.BackgroundScale.z);
        }

        [HarmonyPatch(typeof(WorldFlowCamera), nameof(WorldFlowCamera.ChangeZoomLevel))]
        static class ChangeZoom
        {
            static bool Prefix(WorldFlowCamera __instance, int diff)
            {
                if (diff == 0) return false;
                int current = Level(__instance);
                int next = Mathf.Clamp(current + Math.Sign(diff), -extraOut.Value, __instance.MaxZoom + extraIn.Value);
                if (next == current) return false;
                var camera = CameraField(__instance);
                // Use this camera directly; this also works for the research tree camera.
                Vector2 cursor = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Screen.width / 2f, Screen.height / 2f);
                Vector3 before = camera.ScreenToWorldPoint(new Vector3(cursor.x, cursor.y, 0));
                Apply(__instance, next);
                Vector3 after = camera.ScreenToWorldPoint(new Vector3(cursor.x, cursor.y, 0));
                camera.transform.position += new Vector3(before.x - after.x, before.y - after.y, 0);
                CoverBackground(__instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(WorldFlowCamera), nameof(WorldFlowCamera.SetZoomLevel))]
        static class ResetZoom
        {
            static void Prefix(WorldFlowCamera __instance, ref int lv)
            {
                lv = Mathf.Clamp(lv, 0, 4);
                States.GetOrCreateValue(__instance).Level = lv;
            }
            static void Postfix(WorldFlowCamera __instance) => CoverBackground(__instance);
        }

        [HarmonyPatch(typeof(WorldFlowCamera), "Update")]
        static class BackgroundCoverage
        {
            static void Postfix(WorldFlowCamera __instance) => CoverBackground(__instance);
        }

        [HarmonyPatch(typeof(WorldFlowCamera), nameof(WorldFlowCamera.ZoomScale), MethodType.Getter)]
        static class ActualScale
        {
            static void Postfix(WorldFlowCamera __instance, ref float __result) => __result = Scale(Level(__instance));
        }
    }
}
