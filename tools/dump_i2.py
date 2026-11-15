"""Dump the game's I2 Localization terms with every translation, from Balls_Data/resources.assets.

Usage: python tools/dump_i2.py   -> game/i2_terms.txt (gitignored with the rest of game/)

Unity serializes a string as an int32 byte length + UTF-8 bytes, padded to 4. An I2 term is stored as
its name followed by one string per language, in the order of the source's language list. This scans
for every such string, then groups them: a term name (lowercase, underscores, no spaces) starts a group.
Output: one block per term, "term" then "  code: text" lines. Used to look up the game's official
translation of a name (New Ballbylon, fuser, XP...) when translating the mod's own strings.
"""
import os
import re
import struct

G = r"S:\steam\steamapps\common\BALLxPIT\Balls_Data"
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "game", "i2_terms.txt")


def strings(data):
    i, n = 0, len(data)
    while i + 4 <= n:
        (ln,) = struct.unpack_from("<i", data, i)
        if 0 < ln < 6000 and i + 4 + ln <= n:
            raw = data[i + 4:i + 4 + ln]
            try:
                s = raw.decode("utf-8")
            except UnicodeDecodeError:
                s = None
            if s is not None and not any(ord(c) < 32 and c not in "\n\r\t" for c in s):
                yield s
                i += 4 + ln
                i = (i + 3) & ~3
                continue
        i += 4


TERM = re.compile(r"^[A-Za-z0-9_/\-\.]+$")


def main():
    files = [os.path.join(G, f) for f in os.listdir(G) if f.endswith(".assets")]
    # Column order of the game's I2 source (its language list: English, Chinese (Simplified), ...).
    langs = ["en", "zh-CN", "ja", "ko", "fr", "de", "es", "ru", "pt-BR", "zh-TW", "es-US", "pl", "it", "th", "tr", "uk"]
    blocks = []
    for f in files:
        data = open(f, "rb").read()
        seq = list(strings(data))
        k = 0
        while k < len(seq):
            s = seq[k]
            if TERM.match(s) and "_" in s and s.lower() == s and k + 1 < len(seq) and not TERM.match(seq[k + 1]):
                tr = []
                j = k + 1
                while j < len(seq) and not (TERM.match(seq[j]) and "_" in seq[j] and seq[j].lower() == seq[j]) and len(tr) < 20:
                    tr.append(seq[j])
                    j += 1
                if len(tr) >= 8:
                    blocks.append((s, tr))
                k = j
            else:
                k += 1
    with open(OUT, "w", encoding="utf-8") as o:
        o.write("# languages: " + ", ".join(langs or []) + "\n")
        for term, tr in blocks:
            o.write(term + "\n")
            for idx, t in enumerate(tr):
                code = langs[idx] if langs and idx < len(langs) else str(idx)
                o.write("  " + code + ": " + t.replace("\n", "\\n") + "\n")
    print(len(blocks), "terms ->", OUT, "languages:", langs)


if __name__ == "__main__":
    main()
