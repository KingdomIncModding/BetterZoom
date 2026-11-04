using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Behaviour.Flow;
using PlayerInput = Assets.Behaviour.Input.PlayerInput;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.Shared;
using Assets.Behaviour.UI.TechTree;
using Assets.Source.Flow.Elements;
using Assets.Source.Player;
using Assets.Source.Util;
using BepInEx;
using HarmonyLib;
using LightJson;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        readonly List<string> checks = new List<string>();
        void Check(bool passed, string label)
        { if (!passed) throw new InvalidOperationException(label); checks.Add(label); }
        void Near(float actual, float expected, string label)
            => Check(Mathf.Abs(actual - expected) < Mathf.Max(.001f, Mathf.Abs(expected) * .0001f), label);

        IEnumerator RuntimeCheck()
        {
            yield return new WaitForSecondsRealtime(5);
            string directory = Path.Combine(Paths.GameRootPath, "_Mods", "BetterZoom", "Verification");
            Directory.CreateDirectory(directory);
            string error = null;
            Mouse mouse = null;
            Keyboard keyboard = null;
            WorldFlowCamera workshop = null;
            try
            {
                Check(GamePlayer.Current == null && !GameUI.Instance, "No user save loaded");
                Check(!SaveGame.LoadLatestSave(), "User save loading blocked");
                var player = GamePlayer.CreateNewPlayer();
                player.AddTech("manifold");
                player.Flows[0].Elements.Add(new Manifold { Position = new Vector2Int(2000, 2000) });
                player.Flows[0].Elements.Add(new Manifold { Position = new Vector2Int(-2000, -2000) });
                SceneManager.LoadScene("Game");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(4);
            if (error == null) try
            {
                mouse = InputSystem.AddDevice<Mouse>(); mouse.MakeCurrent();
                keyboard = InputSystem.AddDevice<Keyboard>(); keyboard.MakeCurrent();
                PlayerInput.Asset.devices = new InputDevice[] { mouse, keyboard };
                PlayerInput.Enable();
                InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(Screen.width * .57f, Screen.height * .47f) });
                InputSystem.Update();
                workshop = WorldFlowCamera.Instance;
                Check(workshop && WorldFlowManager.Visible, "Workshop camera loaded");
                VerifyCamera(workshop, "workshop");
                VerifyWheel(workshop, mouse);
                GameUI.Instance.ToggleTechMenu(true);
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(2);
            if (error == null) try
            {
                Check(WorldFlowCamera.Instance != workshop, "Research tree uses separate camera");
                VerifyCamera(WorldFlowCamera.Instance, "research tree");
                VerifyWheel(WorldFlowCamera.Instance, mouse);
                VerifyBackgroundExtremes(WorldFlowCamera.Instance);
                Check(GamePlayer.Current.FlowZoom >= 0 && GamePlayer.Current.FlowZoom <= 4 && GamePlayer.Current.TechTreeZoom >= 0 && GamePlayer.Current.TechTreeZoom <= 4, "Both stored camera levels remain vanilla compatible");
                var json = GamePlayer.Current.ToJson();
                Check(json["FlowZoom"].AsInteger >= 0 && json["TechTreeZoom"].AsInteger >= 0, "Native save JSON contains safe nonnegative zoom levels");
                var view = WorldFlowCamera.Instance;
                Apply(view, -3);
                var nodes = TechTreeMenu.Instance.Nodes.ToArray();
                if (nodes.Length > 0)
                {
                    float x = (nodes.Min(n => n.transform.position.x) + nodes.Max(n => n.transform.position.x)) / 2f;
                    float y = (nodes.Min(n => n.transform.position.y) + nodes.Max(n => n.transform.position.y)) / 2f;
                    CameraField(view).transform.position = new Vector3(x, y, CameraField(view).transform.position.z);
                }
                CoverBackground(view);
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForEndOfFrame();
            if (error == null) try
            {
                CaptureCamera(CameraField(WorldFlowCamera.Instance), Path.Combine(directory, "research-tree-far-1.0.1.png"));
                GameUI.Instance.ToggleTechMenu(false);
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(1);
            if (error == null) try
            {
                Check(WorldFlowCamera.Instance == workshop, "Workshop returns after closing research tree");
                Apply(workshop, -extraOut.Value);
                var camera = CameraField(workshop);
                camera.transform.position = new Vector3(0, 0, camera.transform.position.z);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D)); InputSystem.Update();
                AccessTools.Method(typeof(WorldFlowCamera), "Update").Invoke(workshop, null);
                Check(camera.transform.position.x > 0, "Keyboard pans right at far zoom");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
                Apply(workshop, workshop.MaxZoom + extraIn.Value);
                camera.transform.position = new Vector3(0, 0, camera.transform.position.z);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D)); InputSystem.Update();
                AccessTools.Method(typeof(WorldFlowCamera), "Update").Invoke(workshop, null);
                Check(camera.transform.position.x > 0, "Keyboard pans right at close zoom without stopping or reversing");
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); InputSystem.Update();
                workshop.SetZoomLevel(workshop.ZoomLevel);
                var background = BackgroundField(workshop);
                var state = States.GetOrCreateValue(workshop);
                if (background && state.HasBackground) background.transform.localScale = state.BackgroundScale;
                foreach (var method in new[] { AccessTools.Method(typeof(WorldFlowCamera), "ChangeZoomLevel"), AccessTools.Method(typeof(WorldFlowCamera), "SetZoomLevel"), AccessTools.PropertyGetter(typeof(WorldFlowCamera), "ZoomScale"), AccessTools.Method(typeof(WorldFlowCamera), "Update") })
                    harmony.Unpatch(method, HarmonyPatchType.All, Id);
                workshop.SetZoomLevel(GamePlayer.Current.FlowZoom);
                Near(camera.orthographicSize, Screen.height / WorldFlowCamera.GetZoomScale(workshop.ZoomLevel), "Removing Better Zoom restores vanilla camera without invalid background indexing");
                if (background && state.HasBackground) Check(background.transform.localScale == state.BackgroundScale, "Removal restores native background scale");
            }
            catch (Exception ex) { error = ex.ToString(); }
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            var report = new JsonObject {
                {"passed", error == null}, {"version", Version}, {"gameVersion", Application.version},
                {"unityVersion", Application.unityVersion}, {"bepInExVersion", typeof(BaseUnityPlugin).Assembly.GetName().Version.ToString()},
                {"dllSha256", HashFile(typeof(BetterZoomPlugin).Assembly.Location)}, {"count", checks.Count},
                {"checks", new JsonArray(checks.Select(c => (JsonValue)c).ToArray())},
                {"plugins", new JsonArray(BepInEx.Bootstrap.Chainloader.PluginInfos.Values.Select(p => (JsonValue)(p.Metadata.Name + " " + p.Metadata.Version)).ToArray())},
                {"error", error ?? ""}
            };
            File.WriteAllText(Path.Combine(directory, "runtime-tests.json"), report.ToString());
            if (error != null) Logger.LogError(error); else Logger.LogInfo("Better Zoom diagnostic passed: " + checks.Count);
            Application.Quit();
        }

        void VerifyCamera(WorldFlowCamera view, string label)
        {
            var camera = CameraField(view);
            view.SetZoomLevel(3);
            Near(camera.orthographicSize, Screen.height / 2f, label + " native default size unchanged");
            for (int i = 0; i < 20; i++) view.ChangeZoomLevel(-1);
            Check(Level(view) == -extraOut.Value, label + " reaches extended far limit");
            Near(camera.orthographicSize, Screen.height / .25f * 8f, label + " shows eight times vanilla far width and height");
            var position = camera.transform.position;
            view.ChangeZoomLevel(-1);
            Check(camera.transform.position == position, label + " far limit has no cursor drift");
            for (int level = -extraOut.Value; level <= view.MaxZoom + extraIn.Value; level++)
            {
                Near(camera.orthographicSize, Screen.height / Scale(level), label + " actual scale at " + level);
                Near(view.ZoomScale, Scale(level), label + " public scale at " + level);
                Near(view.TextScale, Scale(level) / 2f, label + " text scale at " + level);
                Check(view.ZoomLevel >= 0 && view.ZoomLevel <= view.MaxZoom, label + " native level safe at " + level);
                VerifyBackground(view, label + " level " + level);
                Vector2 cursor = Mouse.current.position.ReadValue();
                var before = camera.ScreenToWorldPoint(cursor);
                view.ChangeZoomLevel(1);
                var after = camera.ScreenToWorldPoint(cursor);
                Check(Vector2.Distance(before, after) < .05f, label + " cursor anchor stable at " + level);
            }
            Near(camera.orthographicSize, Screen.height / WorldFlowCamera.GetZoomScale(view.MaxZoom) / 8f, label + " eight times vanilla closest magnification");
            position = camera.transform.position;
            view.ChangeZoomLevel(1);
            Check(camera.transform.position == position, label + " close limit has no cursor drift");
            int oldOut = extraOut.Value, oldIn = extraIn.Value;
            extraOut.Value = extraIn.Value = 0;
            view.SetZoomLevel(0); view.ChangeZoomLevel(-1);
            Check(Level(view) == 0, label + " zero extra out restores vanilla limit");
            view.SetZoomLevel(view.MaxZoom); view.ChangeZoomLevel(1);
            Check(Level(view) == view.MaxZoom, label + " zero extra in restores vanilla limit");
            extraOut.Value = oldOut; extraIn.Value = oldIn;
            view.SetZoomLevel(3);
        }

        void VerifyBackground(WorldFlowCamera view, string label)
        {
            var background = BackgroundField(view);
            if (!background || !background.enabled) return; // Workshop intentionally hides its grid at native level zero.
            var camera = CameraField(view);
            var bounds = background.bounds;
            float left = camera.transform.position.x - camera.orthographicSize * camera.aspect;
            float right = camera.transform.position.x + camera.orthographicSize * camera.aspect;
            float bottom = camera.transform.position.y - camera.orthographicSize;
            float top = camera.transform.position.y + camera.orthographicSize;
            Check(bounds.min.x <= left && bounds.max.x >= right && bounds.min.y <= bottom && bounds.max.y >= top, label + " backdrop covers all four viewport edges");
        }

        static void CaptureCamera(Camera camera, string path)
        {
            var previous = RenderTexture.active;
            var oldTarget = camera.targetTexture;
            var target = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, ImageConversion.EncodeToPNG(texture));
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Destroy(texture);
            }
        }

        void VerifyBackgroundExtremes(WorldFlowCamera view)
        {
            var camera = CameraField(view);
            var background = BackgroundField(view);
            Check(background && background.enabled && background.drawMode == SpriteDrawMode.Tiled, "Research backdrop is native tiled sprite");
            Vector2 tileArea = background.size;
            Vector3 original = camera.transform.position;
            float originalAspect = camera.aspect;
            int originalOut = extraOut.Value;
            extraOut.Value = 5;
            foreach (float aspect in new[] { 4f / 3f, 16f / 9f, 32f / 9f })
            foreach (var position in new[] { new Vector2(-12345, -54321), new Vector2(12345, 54321) })
            {
                camera.aspect = aspect;
                camera.transform.position = new Vector3(position.x, position.y, original.z);
                Apply(view, -5);
                VerifyBackground(view, "Maximum configured zoom out, aspect " + aspect + " pan " + position);
                Check(background.size == tileArea, "Extreme zoom retains bounded native tile count");
                // Exercise the actual per-frame hook after a pan at far zoom.
                camera.transform.position += new Vector3(111, -222, 0);
                AccessTools.Method(typeof(WorldFlowCamera), "Update").Invoke(view, null);
                VerifyBackground(view, "Per-frame background follows pan at " + aspect);
            }
            camera.aspect = originalAspect;
            camera.transform.position = original;
            extraOut.Value = originalOut;
            view.SetZoomLevel(3);
            Check(background.transform.localScale == States.GetOrCreateValue(view).BackgroundScale, "Returning to normal zoom restores native grid scale");
        }

        void VerifyWheel(WorldFlowCamera view, Mouse mouse)
        {
            Check(!UIBlocksZoom.Blocked, "Native UI permits wheel zoom");
            view.SetZoomLevel(0);
            InputSystem.QueueDeltaStateEvent(mouse.scroll, Vector2.zero); InputSystem.Update();
            InputSystem.QueueDeltaStateEvent(mouse.scroll, new Vector2(0, -120)); InputSystem.Update();
            Check(Level(view) == -1, "Native wheel input reaches added far level (actual=" + Level(view) + ", enabled=" + PlayerInput.Enabled + ", action=" + PlayerInput.Controls.UI.Zoom.enabled + ")");
            InputSystem.QueueDeltaStateEvent(mouse.scroll, Vector2.zero); InputSystem.Update();
            InputSystem.QueueDeltaStateEvent(mouse.scroll, new Vector2(0, 120)); InputSystem.Update();
            Check(Level(view) == 0, "Native wheel input returns from added level");
            InputSystem.QueueDeltaStateEvent(mouse.scroll, Vector2.zero); InputSystem.Update();
        }

        static string HashFile(string path)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.Store))] static class NoSave { static bool Prefix() => false; }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.StoreAutosaveState))] static class NoAutosave { static bool Prefix() => false; }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.LoadLatestSave))] static class NoLoad { static bool Prefix(ref bool __result) { __result = false; return false; } }
        [HarmonyPatch(typeof(SteamAchievement), nameof(SteamAchievement.Trigger))] static class NoAchievement { static bool Prefix() => false; }
        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Initialized), MethodType.Getter)] static class Offline { static bool Prefix(ref bool __result) { __result = false; return false; } }
    }
}
