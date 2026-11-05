using System;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI.TechTree;
using Assets.Source.Player;
using BepInEx.Bootstrap;
using HarmonyLib;
using KingdomFoundation;
using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KingdomBetterZoom
{
    public sealed partial class BetterZoomPlugin
    {
        void VerifyLocked(WorldFlowCamera view, string name)
        {
            Check(!Unlocked, name + " extra zoom starts locked");
            view.SetZoomLevel(0);
            for (int i = 0; i < 8; i++) view.ChangeZoomLevel(-1);
            Check(Level(view) == 0, name + " cannot exceed far limit before research");
            view.SetZoomLevel(view.MaxZoom);
            for (int i = 0; i < 8; i++) view.ChangeZoomLevel(1);
            Check(Level(view) == view.MaxZoom, name + " cannot exceed close limit before research");
            view.SetZoomLevel(1); view.ChangeZoomLevel(1);
            Check(Level(view) == 2, name + " native zoom works before research");
            view.SetZoomLevel(3);
        }

        void SelectResearchTab(bool mods)
        {
            var tabs = TechTreeMenu.Instance.GetComponent<TechTreeTabs>();
            Check(tabs, "Foundation research tabs exist");
            AccessTools.Method(typeof(TechTreeTabs), "Select").Invoke(tabs, new object[] { mods, false });
        }

        void VerifyResearchUnlock()
        {
            Check(Chainloader.PluginInfos.TryGetValue("urakka.kingdominc.foundation", out var info) && info.Metadata.Version >= new Version(0, 4, 1), "Foundation dependency loads before Better Zoom");
            Check(FoundationApi.TechNodes.TryGet(ResearchId, out var registered) && ReferenceEquals(research, registered) && registered is INativeTechNode, "Research belongs to the public Foundation registry");
            var menu = TechTreeMenu.Instance;
            Check(!menu.Nodes.Any(n => n.Tech.Identifier == ResearchId), "Better Zoom is absent from Base Game research");
            SelectResearchTab(true);
            var node = menu.Nodes.Single(n => n.Tech.Identifier == ResearchId);
            Check(node.Tech == ((INativeTechNode)research).NativeNode && node.Tech.Name == "Better Zoom", "Better Zoom appears as native research under Mods");
            Check(node.Tech.IsAvailable && !node.Tech.IsPurchased, "Research is available without a new prerequisite");
            var player = GamePlayer.Current;
            long gold = player.GetCurrency(Currency.Gold);
            Check(player.SpendCurrency(new CurrencyCollection(Currency.Gold, gold)), "Disposable kingdom starts purchase check with zero gold");
            player.AddCurrency(Currency.Gold, 99, false);
            var left = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            node.OnPointerClick(left);
            Check(!Unlocked && player.GetCurrency(Currency.Gold) == 99, "Insufficient gold leaves research locked and spends nothing");
            player.AddCurrency(Currency.Gold, 1, false);
            node.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right });
            Check(!Unlocked && player.GetCurrency(Currency.Gold) == 100, "Right click does not purchase research");
            node.OnPointerClick(left);
            Check(Unlocked && node.Tech.IsPurchased && player.GetCurrency(Currency.Gold) == 0, "Native purchase spends exactly 100 gold and unlocks extended zoom");
            menu.Nodes.Single(n => n.Tech.Identifier == ResearchId).OnPointerClick(left);
            Check(Unlocked && player.GetCurrency(Currency.Gold) == 0, "Purchased research cannot charge twice");
            var json = player.ToJson();
            Check(json["Tech"].AsJsonArray.Any(v => v.AsString == ResearchId), "Research persists through the native saved technology ID list");
            try
            {
                var restored = GamePlayer.FromJson(json);
                Check(restored.HasTech(ResearchId), "Native save round trip retains Better Zoom unlock");
            }
            finally { GamePlayer.Current = player; }
            VerifyCamera(WorldFlowCamera.Instance, "Mods research tab");
        }

        void VerifyFoundationTabMemory()
        {
            var view = WorldFlowCamera.Instance;
            var camera = CameraField(view);
            Apply(view, -2);
            camera.transform.position = new Vector3(800, 150, camera.transform.position.z);
            float modsSize = camera.orthographicSize;
            SelectResearchTab(false);
            Apply(view, view.MaxZoom + 2);
            camera.transform.position = new Vector3(-300, -200, camera.transform.position.z);
            float baseSize = camera.orthographicSize;
            SelectResearchTab(true);
            Near(camera.orthographicSize, modsSize, "Mods tab retains its extended zoom");
            Check(Level(view) == -2 && Vector2.Distance(camera.transform.position, new Vector2(800, 150)) < .01f, "Mods tab restores extended level and map position");
            VerifyBackground(view, "Mods tab restored background");
            SelectResearchTab(false);
            Near(camera.orthographicSize, baseSize, "Base Game tab retains its own extended zoom");
            Check(Level(view) == view.MaxZoom + 2 && Vector2.Distance(camera.transform.position, new Vector2(-300, -200)) < .01f, "Base Game restores separate extended level and map position");
        }
    }
}
