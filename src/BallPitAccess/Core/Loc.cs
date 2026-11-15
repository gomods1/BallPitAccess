using System;
using System.Collections.Generic;

namespace BallPitAccess.Core
{
    /// <summary>
    /// The mod's own strings, keyed, in every language the mod has a table for; the language follows the
    /// game's (I2 Localization). Missing keys fall back to English with a logged warning.
    /// Never concatenate fragments: use format placeholders.
    /// One table per language the game ships, in Core/Lang/ (En.cs is the reference).
    /// </summary>
    internal static partial class Loc
    {
        private static Dictionary<string, string> _cur;   // null until the first Refresh: English
        private static float _nextCheck;
        internal static string Lang { get; private set; } = "en";

        /// <summary>The game's I2 language codes and the table for each (es-US, Latin American Spanish,
        /// uses the Spanish one). Matched exactly first, then by the part before the dash.
        /// Built on first use: the tables live in other files of this partial class, and C# does not order
        /// static initializers across files, so a field initializer here could see them still null.</summary>
        private static Dictionary<string, Dictionary<string, string>> _tables;
        private static Dictionary<string, Dictionary<string, string>> Tables => _tables ??= new Dictionary<string, Dictionary<string, string>>(System.StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = En, ["es"] = Es, ["fr"] = Fr, ["de"] = De, ["it"] = It, ["ja"] = Ja, ["ko"] = Ko, ["pl"] = Pl,
            ["pt-BR"] = PtBr, ["pt"] = PtBr, ["ru"] = Ru, ["th"] = Th, ["tr"] = Tr, ["uk"] = Uk,
            ["zh-CN"] = ZhCn, ["zh"] = ZhCn, ["zh-Hans"] = ZhCn, ["zh-TW"] = ZhTw, ["zh-Hant"] = ZhTw, ["zh-HK"] = ZhTw,
        };

        private static string Match(string code)
        {
            if (string.IsNullOrEmpty(code)) return "en";
            if (Tables.ContainsKey(code)) return code;
            int dash = code.IndexOf('-');
            string head = dash > 0 ? code.Substring(0, dash) : code;
            return Tables.ContainsKey(head) ? head : "en";
        }

        /// <summary>Follow the game's language (checked once a second).</summary>
        internal static void Refresh()
        {
            float now = UnityEngine.Time.unscaledTime;
            if (now < _nextCheck) return;
            _nextCheck = now + 1f;
            string code = "en";
            try { code = Il2CppI2.Loc.LocalizationManager.CurrentLanguageCode ?? "en"; } catch { }
            string lang = Match(code);
            if (lang == Lang) return;
            Lang = lang;
            _cur = Tables[lang];
            Log.Info("Mod language: " + lang + " (game " + code + ")" + (_cur.Count < En.Count ? $", {En.Count - _cur.Count} strings fall back to English" : ""));
        }

        private static readonly HashSet<string> Warned = new HashSet<string>();

        internal static string Get(string key)
        {
            var cur = _cur ?? En;
            if (cur.TryGetValue(key, out var s)) return s;
            if (cur != En && Warned.Add(Lang + ":" + key)) Log.Warn("Missing " + Lang + " string: " + key);
            if (En.TryGetValue(key, out s)) return s;
            Log.Warn("Missing string: " + key);
            return key;
        }

        internal static bool Has(string key) => En.ContainsKey(key);

        internal static string F(string key, params object[] args)
        {
            try { return string.Format(Get(key), args); }
            catch { return Get(key); }
        }

        /// <summary>Inline sprite names in game text, turned into words. Mouse glyphs become the mod key for the current mode.</summary>
        internal static string SpriteName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            string n = name.ToLowerInvariant();
            bool kb = n.StartsWith("kb_");
            if (kb) n = n.Substring(3);
            if (n.StartsWith("mouse")) return Game.Tutorial.MouseKey(n);
            switch (n)
            {
                case "esc": return Get("key_escape");
                case "space": return Get("key_space");
                case "enter": return Get("key_enter");
                case "gold": return Get("res_gold");
                case "wheat": return Get("res_wheat");
                case "wood": return Get("res_wood");
                case "stone": return Get("res_stone");
                case "gem": case "gems": return Get("res_gem");
            }
            if (kb) return n.ToUpperInvariant();
            // Decoration next to a text that already says it ("Level 2" with an up arrow).
            if (n.Contains("arrow")) return "";
            return name.Replace('_', ' ');
        }
    }
}
