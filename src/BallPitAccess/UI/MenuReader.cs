using System;
using System.Collections.Generic;
using System.Text;
using BallPitAccess.Core;
using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BallPitAccess.UI
{
    /// <summary>
    /// Reads the game's UI focus (template 10.1): the game's CoolSelectables take uGUI selection, so the
    /// EventSystem's selected object is the focused item. Polled every frame; spoken when it changes.
    /// The first line is spoken, the rest go to the Details buffer (Ctrl+Up).
    /// </summary>
    internal static class MenuReader
    {
        private static IntPtr _lastFocus = IntPtr.Zero;
        private static string _lastText = "";
        private static float _focusSince;
        private static GameObject _pending;
        private static int _errors;

        internal static void Update()
        {
            try
            {
                var es = EventSystem.current;
                var go = es != null ? es.currentSelectedGameObject : null;
                IntPtr ptr = go != null ? go.Pointer : IntPtr.Zero;
                float now = Time.unscaledTime;
                if (ptr != _lastFocus)
                {
                    _lastFocus = ptr;
                    _pending = go;
                    _focusSince = now;
                    if (go != null && Config.Debug) Log.Info("Focus: " + Path(go.transform));
                }
                // Wait a few frames: labels and the details panel are filled after the item is selected.
                if (_pending != null && now - _focusSince >= 0.08f && !Navigator.Settling)
                {
                    var g = _pending;
                    _pending = null;
                    // While the hero is being played nothing is navigated: the selection the game parks on a
                    // HUD button (the speed button, after a popup closes) is not news.
                    bool idleBattle = Game.Battle.InBattle && !Navigator.Blocking;
                    // In a battle the game also parks the selection on things that are not controls (a hint's text).
                    bool notControl = Game.Battle.InBattle && g.GetComponent<CoolSelectable>() == null && g.GetComponent<TMP_InputField>() == null
                        && !Readers.IsItem(g);
                    if (!idleBattle && !notControl) Announce(g);
                    else if (Navigator.Blocking) Log.Info("Focus not announced (" + (idleBattle ? "battle" : "not a control") + "): " + Path(g.transform));
                }
                // After activating or adjusting a control: say it again if what it says changed.
                if (_recheckAt > 0 && now >= _recheckAt)
                {
                    _recheckAt = -1f;
                    if (go != null && go.activeInHierarchy && _pending == null && ptr == _recheckPtr && !Navigator.Settling)
                    {
                        string before = _lastText;
                        string after = Describe(go, out var details);
                        if (after != before) { _lastText = after; SetDetails(after, details, go); Speech.SayMenu(after); }
                    }
                }
            }
            catch (Exception ex)
            {
                if (++_errors <= 10) Log.Error("MenuReader: " + ex.Message);
            }
        }

        private static bool _retry;
        private static int _retries;
        private static float _recheckAt = -1f;
        private static IntPtr _recheckPtr;

        internal static void Announce(GameObject go)
        {
            if (go == null) return;
            // A choice still sliding in (an evolution on the fuser's list) can be inactive for a moment: a known item
            // is read anyway, or it was skipped without a word (a player: "one of the evolutions doesn't read at all").
            if (!go.activeInHierarchy && !Readers.IsItem(go)) { Log.Info("Focus on a hidden object, not read: " + Path(go.transform)); return; }
            string text = Describe(go, out var details);
            // The game's details panel has not caught up with the focus yet: look again shortly. The equipment
            // panels (encyclopedia, evolutions, pause equipment) take a few hundred milliseconds.
            if (_retry && _retries < 10)
            {
                _retries++;
                _pending = go;
                _focusSince = Time.unscaledTime - 0.03f;
                return;
            }
            if (_retry) Log.Info("Panel still behind after " + _retries + " retries: " + text);
            else if (_retries > 0 && Config.Debug) Log.Info("Panel caught up after " + _retries + " retries");
            _retries = 0;
            _lastText = text;
            SetDetails(text, details, go);
            string suffix = NextSuffix;
            NextSuffix = null;
            Speech.SayMenu(string.IsNullOrEmpty(suffix) ? text : text + ", " + suffix);
        }

        /// <summary>Said once after the next announced item (the size of the group Tab just entered).</summary>
        internal static string NextSuffix;

        /// <summary>Look at the focused control again shortly (its value may have changed).</summary>
        internal static void Recheck(float delay = 0.2f)
        {
            var es = EventSystem.current;
            var go = es != null ? es.currentSelectedGameObject : null;
            if (go == null) return;
            _recheckPtr = go.Pointer;
            _recheckAt = Time.unscaledTime + delay;
        }

        /// <summary>
        /// The focused item's buffer: its name (top, Ctrl+Down), its own details, then the tooltip the game
        /// shows for it by the time the player asks (Ctrl+Up). Only this item: the other texts of the screen
        /// are F2's, or Ctrl+Up walks into the neighbours' names.
        /// </summary>
        private static void SetDetails(string summary, List<string> details, GameObject go = null)
        {
            var lines = new List<string>();
            if (!string.IsNullOrEmpty(summary)) lines.Add(summary);
            if (details != null) lines.AddRange(details);
            // Read the item again when the player opens its details: by then the game's panel shows this item.
            Func<List<string>> fresh = null;
            if (go != null)
            {
                IntPtr ptr = go.Pointer;
                fresh = () =>
                {
                    var es = EventSystem.current;
                    var cur = es != null ? es.currentSelectedGameObject : null;
                    if (cur == null || cur.Pointer != ptr || !cur.activeInHierarchy) return null;
                    string again = Describe(cur, out var d2);
                    var l2 = new List<string>();
                    if (!string.IsNullOrEmpty(again)) l2.Add(again);
                    if (d2 != null) l2.AddRange(d2);
                    return l2;
                };
            }
            Buffers.SetDetails(lines, HoverLines, fresh);
            Buffers.StartReading();     // the name was just spoken: Ctrl+Up reads the next line
        }

        private static string Describe(GameObject go, out List<string> details)
        {
            var rich = Readers.Read(go);
            _retry = rich != null && rich.Retry;
            if (rich != null && !string.IsNullOrEmpty(rich.Summary))
            {
                details = rich.Details;
                return rich.Summary;
            }
            var lines = TextLines(go.transform);
            if (lines.Count == 0 && go.transform.parent != null && go.transform.parent.GetComponent<CoolSelectable>() == null)
            {
                // A button whose label is a sibling (icon + text rows). Never climb into a whole panel, and
                // never borrow the labels of the buttons next to it (an icon button in a button bar).
                int buttons = 0;
                try { buttons = go.transform.parent.GetComponentsInChildren<CoolButton>(false).Length; } catch { }
                if (buttons <= 1)
                {
                    var pl = TextLines(go.transform.parent);
                    if (pl.Count <= 3) lines = pl;
                }
            }
            string first = lines.Count > 0 ? lines[0] : IconName(go.name);
            var sb = new StringBuilder(first);
            if (lines.Count > 1 && lines[1].Length <= 24) { sb.Append(", ").Append(lines[1]); }
            var btn = go.GetComponent<CoolButton>();
            if (btn != null && !btn.IsInteractable()) sb.Append(", ").Append(Loc.Get("disabled"));
            var tog = go.GetComponent<CoolToggleButton>();
            if (tog != null) sb.Append(", ").Append(Loc.Get(tog.IsToggled ? "on" : "off"));
            details = lines.Count > 1 ? lines.GetRange(1, lines.Count - 1) : null;
            return sb.ToString();
        }

        /// <summary>Tooltip text the game is showing right now (its hover popups), for the Details buffer.</summary>
        internal static List<string> HoverLines()
        {
            var list = new List<string>();
            try
            {
                var seen = new HashSet<IntPtr>();
                HoverPopup[] pops = { BaseHoverPopup.I, GameHoverPopup.I, HoverPopup.I };
                foreach (var hp in pops)
                {
                    if (hp == null || !hp.gameObject.activeInHierarchy || !seen.Add(hp.Pointer)) continue;
                    foreach (var l in TextLines(hp.transform, true)) if (!list.Contains(l)) list.Add(l);
                }
            }
            catch (Exception ex) { if (Config.Debug) Log.Warn("HoverLines: " + ex.Message); }
            return list;
        }

        /// <summary>A name for a button that shows only an icon, from its object name.</summary>
        internal static string IconName(string objName)
        {
            string n = (objName ?? "").ToLowerInvariant();
            if (n.Contains("close") || n.Contains("exit")) return Loc.Get("btn_close");
            if (n.Contains("prev") || n.Contains("left")) return Loc.Get("btn_previous");
            if (n.Contains("next") || n.Contains("right")) return Loc.Get("btn_next");
            if (n.Contains("leaderboard") || n.EndsWith("lb")) return Loc.Get("btn_leaderboards");
            if (n.Contains("setting") || n.Contains("option")) return Loc.Get("btn_settings");
            if (n.Contains("pause")) return Loc.Get("state_kPaused");
            if (n.Contains("stat")) return Loc.Get("btn_stats");
            if (n.Contains("upgrade")) return Loc.Get("btn_upgrade");
            if (n.Contains("ach")) return Loc.Get("btn_achievements");
            if (n.Contains("discord")) return "Discord";
            return CleanName(objName);
        }

        /// <summary>The mod moved focus itself: announce this item once its labels have settled.</summary>
        internal static void ForceAnnounce(GameObject go)
        {
            if (go == null) return;
            _lastFocus = go.Pointer;
            _pending = go;
            _focusSince = Time.unscaledTime;
        }

        internal static void Repeat()
        {
            if (!string.IsNullOrEmpty(_lastText)) Speech.SayMenu(_lastText);
        }

        /// <summary>Visible text under a transform, in hierarchy order, deduplicated.</summary>
        internal static List<string> TextLines(Transform root, bool onScreenOnly = false)
        {
            var list = new List<string>();
            var texts = root.GetComponentsInChildren<TMP_Text>(false);
            // Groups faded out with a CanvasGroup keep their texts active: collect the faded ones once.
            var faded = new List<Transform>();
            foreach (var cg in root.GetComponentsInChildren<CanvasGroup>(false))
                if (cg != null && cg.alpha < 0.05f) faded.Add(cg.transform);
            foreach (var t in texts)
            {
                if (t == null || !t.isActiveAndEnabled) continue;
                if (t.color.a < 0.05f) continue;
                if (faded.Count > 0 && Under(t.transform, faded)) continue;
                if (onScreenOnly)
                {
                    Vector2 sp = Navigator.ScreenPos(t.transform);
                    if (sp.x < 0 || sp.y < 0 || sp.x > Screen.width || sp.y > Screen.height) continue;
                }
                string raw = t.text ?? "";
                string s;
                // Parsed text drops inline sprites (key icons in tutorials); the raw text keeps their tags.
                if (raw.Contains("<sprite")) s = raw;
                else
                {
                    try { s = t.GetParsedText(); } catch { s = null; }
                    if (string.IsNullOrWhiteSpace(s)) s = raw;
                }
                if (Config.Debug && raw.Contains("<sprite")) Log.Info("Sprite text: " + raw);
                s = Speech.Clean(s);
                if (s.Length == 0 || IsJunk(s) || list.Contains(s)) continue;
                list.Add(s);
            }
            // Key and button icons drawn as images.
            var keys = new List<string>();
            foreach (var p in root.GetComponentsInChildren<BtnPromptImg>(false))
                if (p != null && p.isActiveAndEnabled && p.ShowKeyboard) keys.Add(KeyName(p.TgtKeyboardBtn));
            foreach (var p in root.GetComponentsInChildren<ActionBtnPromptImg>(false))
            {
                if (p == null || !p.isActiveAndEnabled) continue;
                try
                {
                    var im = InputMgr.I;
                    if (im == null) continue;
                    var kb = im.GetKeyboardBtnForAction(p.TgtAction, Il2CppRewired.Pole.Positive);
                    if (kb != KeyboardBtn.kNum) keys.Add(KeyName(kb));
                }
                catch { }
            }
            if (keys.Count > 0) list.Insert(0, Loc.F("keys", string.Join(" ", keys)));
            return list;
        }

        private static bool Under(Transform t, List<Transform> roots)
        {
            for (var p = t; p != null; p = p.parent)
                foreach (var r in roots) if (p.Pointer == r.Pointer) return true;
            return false;
        }

        internal static string KeyName(KeyboardBtn k)
        {
            string s = k.ToString();
            return s.StartsWith("k") ? s.Substring(1) : s;
        }

        private static bool IsJunk(string s) => s == "New Text" || s.StartsWith("Lorem");

        internal static string CleanName(string n)
        {
            if (string.IsNullOrEmpty(n)) return Loc.Get("unlabeled");
            n = n.Replace("(Clone)", "").Replace("Btn", " button").Replace('_', ' ');
            var sb = new StringBuilder();
            for (int i = 0; i < n.Length; i++)
            {
                char c = n[i];
                if (i > 0 && char.IsUpper(c) && char.IsLower(n[i - 1])) sb.Append(' ');
                sb.Append(c);
            }
            return sb.ToString().Trim();
        }

        internal static string Path(Transform t)
        {
            var sb = new StringBuilder(t.name);
            for (var p = t.parent; p != null; p = p.parent) sb.Insert(0, p.name + "/");
            return sb.ToString();
        }

        /// <summary>On demand: every visible text on screen, grouped by canvas, as Details lines.</summary>
        internal static void ReadScreen()
        {
            var lines = new List<string>();
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Canvas>())
            {
                if (c == null || !c.isActiveAndEnabled || !c.isRootCanvas) continue;
                var cg = c.GetComponent<CanvasGroup>();
                if (cg != null && cg.alpha < 0.05f) continue;
                foreach (var s in TextLines(c.transform)) if (!lines.Contains(s)) lines.Add(s);
            }
            if (lines.Count == 0) { Speech.SayMenu(Loc.Get("events_empty")); return; }
            Buffers.SetDetails(lines);
            Buffers.StartReading();
            Speech.SayMenu(Loc.F("lines", lines.Count, lines[0]));
            if (Config.Debug) Log.WriteFile("screen.txt", string.Join("\n", lines));
        }
    }
}
