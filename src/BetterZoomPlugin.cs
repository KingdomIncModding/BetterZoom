using System.Runtime.CompilerServices;
using Assets.Behaviour.Flow;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomBetterZoom
{
    [BepInPlugin(Id, "Better Zoom", Version)]
    [BepInDependency("urakka.kingdominc.foundation", "0.11.0")]
    public sealed partial class BetterZoomPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.betterzoom";
        public const string Version = "1.2.0";
        public const string ResearchId = Id + ".unlock";

        internal static readonly ConditionalWeakTable<WorldFlowCamera, ZoomState> States =
            new ConditionalWeakTable<WorldFlowCamera, ZoomState>();

        internal static readonly AccessTools.FieldRef<WorldFlowCamera, Camera> CameraField =
            AccessTools.FieldRefAccess<WorldFlowCamera, Camera>("_camera");

        internal static readonly AccessTools.FieldRef<WorldFlowCamera, SpriteRenderer> BackgroundField =
            AccessTools.FieldRefAccess<WorldFlowCamera, SpriteRenderer>("_background");

        private static readonly AccessTools.FieldRef<WorldFlowCamera, int> BackgroundPeriod =
            AccessTools.FieldRefAccess<WorldFlowCamera, int>("backgroundSize");

        internal static ConfigEntry<int> ExtraOut;
        internal static ConfigEntry<int> ExtraIn;
        internal static ConfigEntry<float> ZoomStep;

        private FoundationModContext context;
        private Harmony harmony;

        internal static bool Unlocked => FoundationApi.World.Player != null &&
            FoundationApi.Progression.HasTech(ResearchId);
    }
}
