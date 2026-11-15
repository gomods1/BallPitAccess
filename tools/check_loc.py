"""Check the mod's string tables (Core/Lang/*.cs against En.cs): duplicate keys (fatal at startup),
missing keys, keys not in English, {0}-placeholder mismatches, keys used in code but not defined.
Usage: python tools/check_loc.py"""
import io, re, glob, os, collections

root = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'src', 'BallPitAccess')
lang_dir = os.path.join(root, 'Core', 'Lang')


def keys(t):
    return re.findall(r'^\s*\["([A-Za-z0-9_]+)"\] =', t, re.M)


def table(name):
    return io.open(os.path.join(lang_dir, name + '.cs'), encoding='utf-8').read()


ENTRY = re.compile(r'^\s*\["([A-Za-z0-9_]+)"\] = "((?:[^"\\]|\\.)*)"', re.M)

en = keys(table('En'))
en_text = dict(ENTRY.findall(table('En')))
print('duplicates en:', [k for k, v in collections.Counter(en).items() if v > 1])
for f in sorted(glob.glob(os.path.join(lang_dir, '*.cs'))):
    name = os.path.splitext(os.path.basename(f))[0]
    if name == 'En':
        continue
    t = table(name)
    ks = keys(t)
    text = dict(ENTRY.findall(t))
    dup = [k for k, v in collections.Counter(ks).items() if v > 1]
    missing = sorted(set(en) - set(ks))
    extra = sorted(set(ks) - set(en))
    # Placeholders {0} {1}... must be the same set as in English.
    ph = [k for k in ks if k in en_text and sorted(set(re.findall(r'\{\d+\}', en_text[k]))) != sorted(set(re.findall(r'\{\d+\}', text.get(k, ''))))]
    print(f'{name}: {len(ks)} keys, duplicates {dup}, missing {len(missing)}{" " + str(missing[:8]) if missing and len(missing) < len(en) else ""}, not in En {extra}, placeholder mismatch {ph}')

used = set()
for f in glob.glob(os.path.join(root, '**', '*.cs'), recursive=True):
    parts = f.replace(os.sep, '/').split('/')
    if 'obj' in parts or 'bin' in parts:
        continue
    t = io.open(f, encoding='utf-8').read()
    used |= set(re.findall(r'Loc\.(?:Get|F|Has)\("([A-Za-z0-9_]+)"', t))
    for m in re.findall(r'Loc\.(?:Get|F)\([^"\n]*\?\s*"([A-Za-z0-9_]+)"\s*:\s*"([A-Za-z0-9_]+)"', t):
        used |= set(m)
    for m in re.findall(r'\?\s*Loc\.Get\("([A-Za-z0-9_]+)"\)', t):
        used.add(m)
print('used, not defined:', sorted(k for k in used if k not in en and not k.endswith('_')))
print(len(en), 'keys')
