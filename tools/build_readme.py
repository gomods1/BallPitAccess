"""Build the player README in every language: refresh the language links in each README*.md, then make
the HTML with pandoc. Usage: python tools/build_readme.py

README.md is English (the reference); README.<code>.md is each other language, code = the game's I2
language code (es, fr, de, it, ja, ko, pl, pt-BR, ru, th, tr, uk, zh-CN, zh-TW). Latin American Spanish
(es-US) reads README.es. The links go between the <!-- LANGS --> and <!-- /LANGS --> lines.
"""
import os
import re
import subprocess

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

LANGS = [
    ("en", "English"), ("es", "Español"), ("fr", "Français"), ("de", "Deutsch"), ("it", "Italiano"),
    ("pt-BR", "Português (Brasil)"), ("pl", "Polski"), ("tr", "Türkçe"), ("ru", "Русский"),
    ("uk", "Українська"), ("ja", "日本語"), ("ko", "한국어"), ("zh-CN", "简体中文"), ("zh-TW", "繁體中文"),
    ("th", "ไทย"),
]


def md_name(code):
    return "README.md" if code == "en" else f"README.{code}.md"


def html_name(code):
    return "README.html" if code == "en" else f"README.{code}.html"


def links(current):
    parts = []
    for code, name in LANGS:
        if code == current:
            parts.append(f"**{name}**")
        elif os.path.exists(os.path.join(ROOT, md_name(code))):
            parts.append(f"[{name}]({html_name(code)})")
    return " · ".join(parts)


BLOCK = re.compile(r"<!-- LANGS -->.*?<!-- /LANGS -->", re.S)


def main():
    built = []
    for code, _ in LANGS:
        md = os.path.join(ROOT, md_name(code))
        if not os.path.exists(md):
            continue
        s = open(md, encoding="utf-8").read()
        if not BLOCK.search(s):
            print(f"{md_name(code)}: no <!-- LANGS --> block, links not added")
        else:
            s = BLOCK.sub("<!-- LANGS -->\n" + links(code) + "\n<!-- /LANGS -->", s)
            open(md, "w", encoding="utf-8", newline="\n").write(s)
        subprocess.check_call(["pandoc", md, "-s", "-o", os.path.join(ROOT, html_name(code)),
                               "-M", "pagetitle=BALL x PIT Access", "-M", f"lang={code}"])
        built.append(code)
    print("built:", ", ".join(built))


main()
