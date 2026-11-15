using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BallPitAccess.Core;
using Il2Cpp;
using Il2CppI2.Loc;
using Il2CppTMPro;

namespace BallPitAccess.Game
{
    /// <summary>
    /// Names of game things in the game's own language (I2 terms from the info objects), with fallbacks:
    /// the info's design-time name, then the enum name split into words. Never throws.
    /// </summary>
    internal static class Names
    {
        private static readonly Regex Params = new Regex(@"\{\[[^\]]*\]\}", RegexOptions.Compiled);
        private static readonly Dictionary<int, string> EnemyCache = new Dictionary<int, string>();
        private static string _cacheLang = "";

        /// <summary>Translation of an I2 term, cleaned; "" when there is none.</summary>
        internal static string Tr(string term)
        {
            if (string.IsNullOrEmpty(term)) return "";
            try
            {
                string t = LocalizationManager.GetTranslation(term);
                // {[x]} is the title's "x" glyph ("RISEN{[x]}BALLBYLON"): a word break when spoken.
                if (!string.IsNullOrEmpty(t)) return Speech.Clean(Params.Replace(t.Replace("{[x]}", " "), ""));
            }
            catch { }
            return "";
        }

        /// <summary>The text a Localize component is showing now ("" when hidden).</summary>
        internal static string Shown(Localize loc)
        {
            try
            {
                if (loc == null || !loc.gameObject.activeInHierarchy) return "";
                var t = loc.GetComponent<TMP_Text>();
                return t != null ? Speech.Clean(t.text) : "";
            }
            catch { return ""; }
        }

        /// <summary>
        /// A Localize's term translated with the placeholders filled from the params manager the game set for
        /// it ("{[DUR]}" → "3"), so the numbers survive even when the visible text has not caught up yet.
        /// "" when the term is unknown or a placeholder has no value.
        /// </summary>
        internal static string Filled(Localize loc, LocalizationParamsManager pm, string fallbackTerm = null)
        {
            try
            {
                string term = null;
                try { if (loc != null) term = loc.Term; } catch { }
                if (string.IsNullOrEmpty(term)) term = fallbackTerm;
                if (string.IsNullOrEmpty(term) || pm == null) return "";
                string t = LocalizationManager.GetTranslation(term);
                if (string.IsNullOrEmpty(t)) return "";
                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var list = pm._Params;
                if (list != null)
                    for (int i = 0; i < list.Count; i++)
                    {
                        var p = list[i];
                        if (p != null && !string.IsNullOrEmpty(p.Name)) values[p.Name] = p.Value ?? "";
                    }
                t = t.Replace("{[x]}", " ");
                var sb = new StringBuilder();
                int at = 0;
                foreach (Match m in Params.Matches(t))
                {
                    string key = m.Value.Substring(2, m.Value.Length - 4);
                    if (!values.TryGetValue(key, out var v) || v.Length == 0) return "";
                    sb.Append(t, at, m.Index - at).Append(v);
                    at = m.Index + m.Length;
                }
                sb.Append(t, at, t.Length - at);
                return Speech.Clean(sb.ToString());
            }
            catch { return ""; }
        }

        internal static string Shown(TMP_Text t)
        {
            try { return t != null && t.gameObject.activeInHierarchy ? Speech.Clean(t.text) : ""; }
            catch { return ""; }
        }

        /// <summary>The text shown on a component's object (or its first child text); "" when hidden.
        /// For the game's LocalizationParamsManager fields, which sit on the text they fill.</summary>
        internal static string ShownAt(UnityEngine.Component c)
        {
            try
            {
                if (c == null || !c.gameObject.activeInHierarchy) return "";
                var t = c.GetComponent<TMP_Text>();
                if (t == null) t = c.GetComponentInChildren<TMP_Text>(false);
                return t != null ? Speech.Clean(t.text) : "";
            }
            catch { return ""; }
        }

        /// <summary>The player owns the Risen Ballbylon expansion (the game's own check).</summary>
        internal static bool HasDlc
        {
            get { try { return DLCUtl.CanAccessDLC(DLCId.kBallbylonRise); } catch { return false; } }
        }

        /// <summary>"kSkeletonKing" → "skeleton king".</summary>
        internal static string FromEnum(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.Length > 1 && s[0] == 'k' && char.IsUpper(s[1])) s = s.Substring(1);
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (c == '_') { sb.Append(' '); continue; }
                if (char.IsUpper(c) && sb.Length > 0 && sb[sb.Length - 1] != ' ') sb.Append(' ');
                sb.Append(char.ToLowerInvariant(c));
            }
            return sb.ToString();
        }

        internal static string Enemy(GridPieceInst inst)
        {
            if (inst == null) return "";
            int key = (int)inst.Type;
            if (_cacheLang != Loc.Lang) { EnemyCache.Clear(); _cacheLang = Loc.Lang; }
            if (EnemyCache.TryGetValue(key, out var cached)) return cached;
            string name = "";
            try
            {
                var info = inst.GetInfo();
                if (info != null)
                {
                    int idx = 0;
                    try { var lm = LevelMgr.I; if (lm != null) idx = info.GetVizIdx(lm.CurLevel); } catch { }
                    name = Tr(info.GetNameSlug(idx));
                    if (name.Length == 0 && idx != 0) name = Tr(info.GetNameSlug(0));
                    if (name.Length == 0) name = Speech.Clean(info.DisplayName);
                }
            }
            catch { }
            if (name.Length == 0) name = FromEnum(inst.Type.ToString());
            EnemyCache[key] = name;
            return name;
        }

        internal static string Upgrade(UpgradeInfo inf)
        {
            if (inf == null) return "";
            try
            {
                string n = Tr(inf.GetNameSlug());
                if (n.Length > 0) return n;
                n = Speech.Clean(inf.Name);
                if (n.Length > 0) return n;
                return FromEnum(inf.name);
            }
            catch { return ""; }
        }

        /// <summary>Description with its numbers removed (the game fills them per level); a fallback for panels.</summary>
        internal static string UpgradeDesc(UpgradeInfo inf)
        {
            if (inf == null) return "";
            try { return Tr(inf.GetDescSlug()); } catch { return ""; }
        }

        internal static string Char(CharInfo inf)
        {
            if (inf == null) return "";
            try
            {
                string n = Tr(inf.GetNameSlug());
                if (n.Length > 0) return n;
                n = Speech.Clean(inf.Name);
                return n.Length > 0 ? n : FromEnum(inf.Type.ToString());
            }
            catch { return ""; }
        }

        internal static string Char(CharMetaInst c)
        {
            if (c == null) return "";
            try { return Char(c.GetInfo()); } catch { return ""; }
        }

        internal static string Building(BuildingInfo inf)
        {
            if (inf == null) return "";
            try
            {
                string n = Tr(inf.GetNameSlug());
                if (n.Length > 0) return n;
                n = Speech.Clean(inf.Name);
                return n.Length > 0 ? n : FromEnum(inf.Type.ToString());
            }
            catch { return ""; }
        }

        internal static string Level(LevelInfo inf)
        {
            if (inf == null) return "";
            try
            {
                string n = Speech.Clean(inf.GetNameTranslation(false));
                if (n.Length > 0) return n;
                n = Tr(inf.GetNameSlug());
                return n.Length > 0 ? n : FromEnum(inf.Type.ToString());
            }
            catch { return ""; }
        }

        internal static string Resource(ResourceType rt)
        {
            switch (rt)
            {
                case ResourceType.kGold: return Loc.Get("res_gold");
                case ResourceType.kWheat: return Loc.Get("res_wheat");
                case ResourceType.kWood: return Loc.Get("res_wood");
                case ResourceType.kStone: return Loc.Get("res_stone");
                case ResourceType.kGem: return Loc.Get("res_gem");
            }
            return FromEnum(rt.ToString());
        }

        /// <summary>"30 gold, 10 wood" for a cost; "" when empty.</summary>
        internal static string Cost(Cost c)
        {
            if (c == null) return "";
            try
            {
                var num = c.Num;
                if (num == null) return "";
                var parts = new List<string>();
                for (int i = 0; i < num.Length && i < (int)ResourceType.kNum; i++)
                    if (num[i] > 0) parts.Add(num[i] + " " + Resource((ResourceType)i));
                return string.Join(", ", parts);
            }
            catch { return ""; }
        }

        internal static string Mod(string prefix, string enumName)
        {
            string key = prefix + enumName;
            return Loc.Has(key) ? Loc.Get(key) : FromEnum(enumName);
        }

        internal static string Pickup(PickupType t) => Mod("pick_", t.ToString());
        internal static string PlayerStatus(PlayerStatusEffectType t) => Mod("pfx_", t.ToString());
        internal static string Obstacle(ObstacleType t) => Mod("obs_", t.ToString());
    }
}
