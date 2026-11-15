"""Print the summarized API of the named classes. Usage: python tools/api.py Class1 Class2 ..."""
import sys, re
lines = open('game/api/game.txt', encoding='utf-8').read().splitlines()
want = set(sys.argv[1:]); p = False
noise = re.compile(r'_\d+__\d+|IEnumerator|IDisposable|MoveNext|__\d+_\d+')
for l in lines:
    m = re.match(r'^(class|struct|enum|interface) ([\w\.]+)', l)
    if m: p = m.group(2).split('.')[-1] in want
    if p and not noise.search(l): print(l)
