"""Condense ilspycmd output of Il2CppInterop assemblies into one-line-per-member API text.
Usage: python summarize_api.py <decompiled dir> <out file>"""
import os, re, sys
src, out = sys.argv[1], sys.argv[2]
decl = re.compile(r'^\s*(public|internal|protected|private)[^=;(]*\b(class|struct|enum|interface)\s+(\w+)')
member = re.compile(r'^\s*public\s+(unsafe\s+)?(static\s+)?(virtual\s+|override\s+|abstract\s+)?([\w\.<>\[\],\s]+?)\s+(\w+)\s*(\(.*\))?\s*$')
enumval = re.compile(r'^\s*(\w+)\s*=\s*(-?\w+),?\s*$')
skip = re.compile(r'NativeFieldInfoPtr|NativeMethodInfoPtr|_PDM_|Method_Internal|Method_Private|MethodInfoStore|__\d__|_d__\d|_c__DisplayClass|System\.IntPtr pointer|^\s*public\s+\w+\(\)$')
with open(out, 'w', encoding='utf-8') as fo:
    for root, _, files in os.walk(src):
        for f in sorted(files):
            if not f.endswith('.cs'): continue
            lines = open(os.path.join(root, f), encoding='utf-8', errors='replace').read().splitlines()
            buf = []; inenum = False
            for i, l in enumerate(lines):
                m = decl.match(l)
                if m and m.group(2) != 'enum' and inenum: inenum = False
                if skip.search(l): continue
                if m:
                    if 'sealed class _' in l or '<>' in l: continue
                    inenum = m.group(2) == 'enum'
                    ns = next((x[10:].strip(' ;') for x in lines if x.startswith('namespace ')), '')
                    buf.append(f"{m.group(2)} {ns}.{m.group(3)}" + (l.split(':',1)[1].strip().join([' : ','']) if ':' in l and m.group(2)!='enum' else ''))
                    continue
                if inenum:
                    e = enumval.match(l)
                    if e: buf.append(f"    {e.group(1)}={e.group(2)}")
                    continue
                mm = member.match(l)
                if mm and mm.group(5) not in ('get','set'):
                    kind = 'M' if mm.group(6) else 'P'
                    st = 'static ' if mm.group(2) else ''
                    buf.append(f"    {kind} {st}{mm.group(4).strip()} {mm.group(5)}{mm.group(6) or ''}")
            if buf: fo.write('\n'.join(buf) + '\n')
