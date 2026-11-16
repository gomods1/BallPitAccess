"""How balls end, from telemetry (bst / rm / ball records): for every ball, its state changes and its
removal, with where it was relative to the hero. Answers "what is a catch in this game".
Usage: python tools/ball_lifecycle.py [telemetry.tsv]"""
import sys, collections, statistics

PATH = sys.argv[1] if len(sys.argv) > 1 else r'S:\steam\steamapps\common\BALLxPIT\UserData\BallPitAccess\telemetry.tsv'
life = collections.defaultdict(list)     # id -> [(t, kind, fields)]
track = collections.defaultdict(list)    # id -> [(t, x, y, state)]
hero = []
for line in open(PATH, encoding='utf-8', errors='replace'):
    p = line.rstrip('\n').split('\t')
    if len(p) < 3:
        continue
    t = int(p[0]) / 1000
    if p[1] in ('bst', 'rm'):
        life[p[2]].append((t, p[1], p[3:]))
    elif p[1] == 'ball':
        track[p[2]].append((t, float(p[3]), float(p[4]), p[5]))
    elif p[1] == 'snap' and len(p) > 3:
        hero.append((t, float(p[2]), float(p[3])))

ends = collections.Counter()
ret_dist, ret_time, norm_rm = [], [], []
examples = collections.defaultdict(list)
for bid, ev in life.items():
    ev.sort()
    t_ret = None
    for t, k, f in ev:
        if k == 'bst':
            frm, to, side, x, y, hx, hy = f[0], f[1], f[2], float(f[3]), float(f[4]), float(f[5]), float(f[6])
            if to == 'kReturning':
                t_ret = t
                ret_dist.append((x - hx, y - hy))
                if len(examples['ret']) < 8: examples['ret'].append(f'{bid} {frm}->{to} at dx {x - hx:+.2f} dy {y - hy:+.2f}')
        else:
            rt, st, side, x, y, hx, hy = f[0], f[1], f[2], float(f[3]), float(f[4]), float(f[5]), float(f[6])
            key = f'{rt} from {st}' + (' side' if side == '1' else '')
            ends[key] += 1
            if len(examples[key]) < 6: examples[key].append(f'{bid} at dx {x - hx:+.2f} dy {y - hy:+.2f}' + (f', {t - t_ret:.2f}s after returning' if t_ret else ''))
            if t_ret: ret_time.append(t - t_ret)
            if st == 'kNormal': norm_rm.append((x - hx, y - hy))
print('== How balls are removed')
for k, n in ends.most_common():
    print(f'  {n:4d}  {k}')
    for e in examples[k]:
        print('        ', e)
print('\n== Where a ball starts flying home (relative to the hero)')
if ret_dist:
    print(f'  {len(ret_dist)} times; dy median {statistics.median(d[1] for d in ret_dist):+.2f}, |dx| median {statistics.median(abs(d[0]) for d in ret_dist):.2f}')
    for e in examples['ret']:
        print('        ', e)
if ret_time:
    print(f'  time from "returning" to removal: median {statistics.median(ret_time):.2f}s, max {max(ret_time):.2f}s')
if norm_rm:
    print(f'\n== Removed while still in normal flight: {len(norm_rm)}; dy median {statistics.median(d[1] for d in norm_rm):+.2f}, |dx| median {statistics.median(abs(d[0]) for d in norm_rm):.2f}')
trans = collections.Counter()
for bid, ev in life.items():
    for t, k, f in ev:
        if k == 'bst':
            trans[f'{f[0]} -> {f[1]}'] += 1
print('\n== State changes')
for k, n in trans.most_common():
    print(f'  {n:4d}  {k}')

# ---- Catch width: where each ball crossed the hero's row, and whether it then turned home right there.
import bisect
ht = [h[0] for h in hero]
def hero_at(t):
    i = min(max(bisect.bisect_left(ht, t), 0), len(hero) - 1)
    return hero[i]
caught_off, missed_off = [], []
for bid, ev in life.items():
    rets = [(t, f) for t, k, f in ev if k == 'bst' and f[1] == 'kReturning']
    tr = sorted(track.get(bid, []))
    for t_ret, f in rets:
        dy = float(f[4]) - float(f[6])
        # last sample of this flight above the hero row before it turned home, and the next one below it
        before = [s for s in tr if s[0] <= t_ret and s[3] == 'kNormal']
        if len(before) < 2:
            continue
        hx, hy = hero_at(t_ret)[1:]
        cross = None
        for a, b in zip(before, before[1:]):
            if a[2] >= hy > b[2] or (a[2] >= hy and b is before[-1]):
                k = (a[2] - hy) / (a[2] - b[2]) if a[2] != b[2] else 0
                cross = a[1] + (b[1] - a[1]) * k
        if cross is None:
            cross = float(f[3])
        off = cross - hero_at(t_ret)[1]
        (caught_off if dy > -1.0 else missed_off).append(off)
def show(name, xs):
    if xs:
        a = sorted(abs(x) for x in xs)
        print(f'  {name}: {len(xs)}, |offset| median {statistics.median(a):.2f}, min {a[0]:.2f}, max {a[-1]:.2f}, '
              f'within 0.4: {sum(1 for x in a if x < 0.4)}, within 0.7: {sum(1 for x in a if x < 0.7)}')
print('\n== Catch width (offset of the ball from the hero when it crossed the hero row)')
show('caught (turned home within 1 unit of the hero)', caught_off)
show('passed (turned home at the floor)', missed_off)
