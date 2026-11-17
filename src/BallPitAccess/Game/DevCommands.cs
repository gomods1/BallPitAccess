using System;
using System.IO;
using BallPitAccess.Core;
using Il2Cpp;
using UnityEngine;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Developer commands for setting up a test save without driving the game window. When
    /// `UserData/BallPitAccess/dev.txt` exists, each line is run once through the game's own save and cheat
    /// code, the results go to Latest.log ("Dev: ..."), and the file is deleted. Only with `Debug = true` under
    /// `[BallPitAccess]` in MelonPreferences.cfg (edit it while the game is closed).
    /// Commands: `resources N` (every resource +N), `blueprint kSheriffOffice` (any BuildingType),
    /// `unseen kFirstHouse` (mark a base tutorial unseen), `tut kFirstHouse` (show it), `cheat kAddResources`
    /// (any BaseCheatType; several do nothing in the shipped game), `probe` (base state and build list),
    /// `select kSheriffOffice` (the build menu's own SelectItem), `save`, `state`.
    /// </summary>
    internal static class DevCommands
    {
        private static float _nextPoll;

        internal static void Update()
        {
            if (Time.unscaledTime < _nextPoll) return;
            _nextPoll = Time.unscaledTime + 1f;
            string path = Path.Combine(Log.Dir ?? "", "dev.txt");
            if (!File.Exists(path)) return;
            string[] lines;
            try { lines = File.ReadAllLines(path); File.Delete(path); }
            catch (Exception ex) { Log.Warn("Dev: cannot read dev.txt: " + ex.Message); return; }
            foreach (var raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                try { Log.Info("Dev: " + line + " -> " + Run(line)); }
                catch (Exception ex) { Log.Error("Dev: " + line + " failed: " + ex); }
            }
        }

        private static string Run(string line)
        {
            var parts = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            string cmd = parts[0].ToLowerInvariant();
            string arg = parts.Length > 1 ? parts[1].Trim() : "";
            var sm = SaveMgr.I;
            switch (cmd)
            {
                case "resources":
                {
                    if (sm == null) return "no SaveMgr";
                    int n = arg.Length > 0 ? int.Parse(arg) : 100000;
                    for (int i = 0; i < (int)ResourceType.kNum; i++) sm.AddResources((ResourceType)i, n, false, false);
                    sm.SaveMeta();
                    RefreshBaseUI();
                    return "added " + n + " of each; gold " + sm.GetNumGold();
                }
                case "blueprint":
                {
                    if (sm == null) return "no SaveMgr";
                    var bt = (BuildingType)Enum.Parse(typeof(BuildingType), arg);
                    bool had = sm.HasBlueprint(bt);
                    if (!had) sm.GainBlueprint(bt);
                    sm.SaveMeta();
                    return bt + (had ? " already owned" : " gained");
                }
                case "tut":
                {
                    var tm = BaseTutMgr.I;
                    if (tm == null) return "no BaseTutMgr (not in the base?)";
                    var t = (BaseTutType)Enum.Parse(typeof(BaseTutType), arg);
                    bool seen = tm.IsSeen(t);
                    tm.Show(t);
                    return "showing " + t + " (was seen " + seen + ")";
                }
                case "unseen":
                {
                    if (sm == null || sm.MetaSave == null) return "no save";
                    var t = (BaseTutType)Enum.Parse(typeof(BaseTutType), arg);
                    var arr = sm.MetaSave.IsBaseTutSeen;
                    if (arr == null || (int)t >= arr.Length) return "no seen array";
                    arr[(int)t] = false;
                    sm.SaveMeta();
                    return t + " marked unseen";
                }
                case "cheat":
                {
                    var cm = BaseCheatMgr.I;
                    if (cm == null) return "no BaseCheatMgr in this build";
                    var ct = (BaseCheatType)Enum.Parse(typeof(BaseCheatType), arg);
                    cm.ApplyCheat(ct);
                    RefreshBaseUI();
                    return "applied " + ct;
                }
                case "save":
                    if (sm == null) return "no SaveMgr";
                    sm.SaveMeta();
                    return "saved";
                case "state":
                {
                    if (sm == null) return "no SaveMgr";
                    string s = "gold " + sm.GetNumGold();
                    for (int i = 1; i < (int)ResourceType.kNum; i++) s += ", " + (ResourceType)i + " " + sm.GetNumResources((ResourceType)i);
                    s += "; sheriff blueprint " + sm.HasBlueprint(BuildingType.kSheriffOffice);
                    var tm = BaseTutMgr.I;
                    if (tm != null)
                    {
                        s += "; tut " + tm.CurTut + " step " + tm.CurTutStep + " showing " + tm.IsShowing();
                        foreach (BaseTutType t in Enum.GetValues(typeof(BaseTutType)))
                            if (t != BaseTutType.kNum) s += " " + t + "=" + (tm.IsSeen(t) ? "seen" : "new");
                    }
                    s += "; cheat mgr " + (BaseCheatMgr.I != null);
                    return s;
                }
                case "probe":
                {
                    string s = "base state " + (BaseMgr.I != null ? BaseMgr.I.CurState.ToString() : "none");
                    s += "; placement " + Placement.Active + "; ftue " + UI.Navigator.FtueShowing + "; blocking " + UI.Navigator.Blocking;
                    var es = UnityEngine.EventSystems.EventSystem.current;
                    s += "; selected " + (es != null && es.currentSelectedGameObject != null ? UI.MenuReader.Path(es.currentSelectedGameObject.transform) : "none");
                    var ui = BuildUI.I;
                    if (ui != null)
                        foreach (var bi in ui.GetComponentsInChildren<BuildItem>(false))
                        {
                            var inf = bi.TgtInfo;
                            s += "\n  item " + (inf != null ? inf.Type.ToString() : "null") + " locked " + bi.IsLocked + " afford " + bi.CanAfford
                                + " interactable " + (bi.Btn != null && bi.Btn.IsInteractable()) + " cost '" + (bi.TxtCost != null ? bi.TxtCost.text : "") + "'"
                                + (inf != null ? " canBuildMore " + inf.CanBuildMore() + " blueprint " + (sm != null && sm.HasBlueprint(inf.Type)) : "");
                        }
                    return s;
                }
                case "select":
                {
                    var ui = BuildUI.I;
                    if (ui == null) return "no BuildUI";
                    var bt = (BuildingType)Enum.Parse(typeof(BuildingType), arg);
                    var item = ui.GetItemForBuilding(bt);
                    if (item == null) return "no item for " + bt;
                    ui.SelectItem(item);
                    return "SelectItem done; base state " + (BaseMgr.I != null ? BaseMgr.I.CurState.ToString() : "none");
                }
                default:
                    return "unknown command";
            }
        }

        private static void RefreshBaseUI()
        {
            try { BaseUI.I?.RefreshResourceRect(); } catch { }
        }
    }
}
