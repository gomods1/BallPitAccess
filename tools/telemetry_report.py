"""Report on a battle recording (UserData/BallPitAccess/telemetry.tsv, Telemetry = true in MelonPreferences).
Answers: how busy the sound is, which cues mask which, whether hits were warned, how catching and item
collection went, what the assists did.
Usage: python tools/telemetry_report.py [telemetry.tsv]"""
import sys, math, collections, statistics

PATH = sys.argv[1] if len(sys.argv) > 1 else r'S:\steam\steamapps\common\BALLxPIT\UserData\BallPitAccess\telemetry.tsv'

CAT = [  # cue name prefix -> category (the same groups as the number-key filters)
    ('enemy', 'enemies'), ('boss', 'enemies'), ('danger', 'enemies'), ('charge', 'enemies'),
    ('catch', 'balls'), ('ball_', 'balls'), ('ready', 'balls'),
    ('pickup', 'rewards'), ('treasure', 'rewards'),
    ('shot', 'hazards'), ('beam', 'hazards'), ('bomb', 'hazards'), ('floor', 'hazards'),
    ('wall', 'edges'), ('hurt', 'health'), ('heal', 'health'), ('heart', 'health'), ('status', 'health'),
    ('retarget', 'aim'), ('xp', 'events'), ('levelup', 'events'), ('win', 'events'), ('lose', 'events'), ('tick', 'ui'),
]
WARN = ('shot_hit', 'bomb_hit', 'beam_hit', 'danger', 'charge', 'beam', 'bomb')


def cat(name):
    for p, c in CAT:
        if name.startswith(p):
            return c
    return 'other'


def fnum(s):
    try:
        return float(s)
    except ValueError:
        return float('nan')


cues, says, evs, snaps = [], [], [], []
loops = collections.defaultdict(list)          # name -> [(t, gain, pan, pitch)]
items_by_snap, enemies_n = [], []
cur_items = None
for line in open(PATH, encoding='utf-8', errors='replace'):
    p = line.rstrip('\n').split('\t')
    if len(p) < 3 or not p[0].isdigit():
        continue
    if len(p) < {'cue': 7, 'snap': 16, 'loop': 6, 'item': 6}.get(p[1], 0):
        continue      # a line cut off by copying the file while the game writes it
    t = int(p[0]) / 1000.0
    k = p[1]
    if k == 'cue':
        cues.append((t, p[2], fnum(p[3]), fnum(p[4]), fnum(p[5]), fnum(p[6])))
    elif k == 'say':
        says.append((t, p[2] if len(p) > 2 else ''))
    elif k == 'ev':
        evs.append((t, p[2], p[3:]))
    elif k == 'snap':
        snaps.append((t, [fnum(x) if i not in (11, 13) else x for i, x in enumerate(p[2:])]))
        cur_items = []
        items_by_snap.append((t, cur_items))
        enemies_n.append(0)
    elif k == 'loop':
        loops[p[2]].append((t, fnum(p[3]), fnum(p[4]), fnum(p[5])))
    elif k == 'item' and cur_items is not None:
        cur_items.append((p[2], p[3], fnum(p[4]), fnum(p[5])))
    elif k == 'enemy' and enemies_n:
        enemies_n[-1] += 1

# Battle time: snaps with playing = 1
play = [t for t, s in snaps if len(s) > 4 and s[4] == 1]
if not play:
    sys.exit('no battle in this recording')
t_start, t_end = play[0], play[-1]
dur = len(play) * 0.1
bcues = [c for c in cues if t_start <= c[0] <= t_end]
print(f'== Battle: {dur / 60:.1f} min playing ({(t_end - t_start) / 60:.1f} min wall), {len(bcues)} cues, '
      f'{sum(1 for s in says if t_start <= s[0] <= t_end)} spoken lines')

# ---------------- 1. Density ----------------
print('\n== 1. How busy the sound is')
by_cat = collections.Counter(cat(c[1]) for c in bcues)
by_name = collections.Counter(c[1] for c in bcues)
for c, n in by_cat.most_common():
    print(f'  {c:9s} {n:5d} cues  {n / dur * 60:6.1f}/min')
print('  top cues:', ', '.join(f'{n} {k}' for k, n in by_name.most_common(12)))
# cues per 1 s window
win = collections.Counter(int(c[0]) for c in bcues)
secs = range(int(t_start), int(t_end) + 1)
counts = [win.get(s, 0) for s in secs]
if counts:
    print(f'  one-shots per second: median {statistics.median(counts):.0f}, 90th pct {sorted(counts)[int(len(counts) * .9)]}, max {max(counts)}')
    for th in (3, 5, 8):
        print(f'    seconds with >= {th} one-shots: {sum(1 for x in counts if x >= th) / len(counts) * 100:.0f}%')
# categories heard in the same second
cats_per_sec = collections.defaultdict(set)
for c in bcues:
    cats_per_sec[int(c[0])].add(cat(c[1]))
multi = [len(cats_per_sec.get(s, ())) for s in secs]
print(f'  categories in the same second: median {statistics.median(multi):.0f}, seconds with >= 3: '
      f'{sum(1 for x in multi if x >= 3) / max(1, len(multi)) * 100:.0f}%')
# loops audible
for name, rows in loops.items():
    rows = [r for r in rows if t_start <= r[0] <= t_end]
    on = sum(1 for r in rows if r[1] > 0.02) * 0.1
    print(f'  loop {name}: audible {on / dur * 100:.0f}% of the time, mean gain {statistics.mean([r[1] for r in rows if r[1] > 0.02] or [0]):.2f}')

# ---------------- 2. Overlap / masking ----------------
print('\n== 2. Cues that land on top of each other (within 80 ms, different categories)')
pairs = collections.Counter()
srt = sorted(bcues)
for i, a in enumerate(srt):
    j = i + 1
    while j < len(srt) and srt[j][0] - a[0] < 0.08:
        b = srt[j]
        ca, cb = cat(a[1]), cat(b[1])
        if ca != cb:
            pairs[tuple(sorted((ca, cb)))] += 1
        j += 1
for (a, b), n in pairs.most_common(10):
    print(f'  {a} + {b}: {n}')
crit = [c for c in bcues if c[1] in ('shot_hit', 'bomb_hit', 'beam_hit', 'danger')]
masked = 0
for c in crit:
    near = [d for d in srt if abs(d[0] - c[0]) < 0.15 and d is not c and cat(d[1]) != 'hazards']
    if len(near) >= 2:
        masked += 1
print(f'  warning cues (shot/bomb/beam hit, snarl) with 2+ other cues within 150 ms: {masked} of {len(crit)}')

# ---------------- 3. Damage and warnings ----------------
print('\n== 3. Damage: was it warned?')
hits = [(t, a) for t, w, a in evs if w == 'health' and len(a) > 1 and fnum(a[1]) < 0 and t_start <= t <= t_end]
warned, lead = 0, []
unwarned = []
for t, a in hits:
    prior = [c for c in bcues if t - 2.5 <= c[0] <= t and c[1] in WARN]
    if prior:
        warned += 1
        lead.append(t - prior[0][0])
    else:
        unwarned.append(t)
print(f'  hits taken: {len(hits)} (total {-sum(fnum(a[1]) for _, a in hits):.0f} health)')
if hits:
    print(f'  warned within 2.5 s: {warned} ({warned / len(hits) * 100:.0f}%), median lead {statistics.median(lead) if lead else 0:.2f} s')
for t in unwarned[:8]:
    before = [f'{c[1]}@{t - c[0]:.1f}s' for c in bcues if t - 2.5 <= c[0] <= t][-6:]
    sn = min(snaps, key=lambda s: abs(s[0] - t))[1]
    print(f'    unwarned hit at +{t - t_start:.0f}s, hero x {sn[0]:.2f}; cues before: {", ".join(before) or "none"}')
heals = [(t, a) for t, w, a in evs if w == 'health' and len(a) > 1 and fnum(a[1]) > 0 and t_start <= t <= t_end]
print(f'  heals: {len(heals)} (+{sum(fnum(a[1]) for _, a in heals):.0f})')

# ---------------- 4. Balls ----------------
print('\n== 4. Catching')
ncatch, nmiss = by_name.get('catch', 0), by_name.get('ball_miss', 0)
print(f'  caught {ncatch}, missed {nmiss}' + (f' ({ncatch / (ncatch + nmiss) * 100:.0f}% caught)' if ncatch + nmiss else ''))
coming = [s for t, s in snaps if t_start <= t <= t_end and not math.isnan(s[5])]
if coming:
    offs = [abs(s[5] - s[0]) for s in coming]
    print(f'  while a ball was coming ({len(coming) / len(play) * 100:.0f}% of the time): hero within 0.5 of the landing x '
          f'{sum(1 for o in offs if o < 0.5) / len(offs) * 100:.0f}% of the time')
    clear = [s for s in coming if s[7] == 1]
    print(f'  ball clear of enemies (final landing spot) {len(clear) / len(coming) * 100:.0f}% of that time')
assist = [s for t, s in snaps if t_start <= t <= t_end and s[10] == 1]
print(f'  catch assist steering {len(assist) / len(play) * 100:.0f}% of the time; dodging {sum(1 for t, s in snaps if t_start <= t <= t_end and s[8] == 1) / len(play) * 100:.0f}%')

# ---------------- 5. Items ----------------
print('\n== 5. Items')
seen = {}
for t, its in items_by_snap:
    if not (t_start <= t <= t_end):
        continue
    for typ, st, x, y in its:
        key = (typ, round(x, 1), round(y, 1))
    # identity is not stable across frames (items drift): track per type counts instead
counts_t = collections.defaultdict(list)
for t, its in items_by_snap:
    if t_start <= t <= t_end:
        c = collections.Counter(typ for typ, st, x, y in its)
        counts_t[t] = c
types = set(k for c in counts_t.values() for k in c)
for typ in sorted(types):
    ser = [c.get(typ, 0) for t, c in sorted(counts_t.items())]
    print(f'  {typ:12s} on the field: mean {statistics.mean(ser):.1f}, max {max(ser)}')
idle_far = 0
for t, its in items_by_snap:
    if t_start <= t <= t_end:
        idle_far += sum(1 for typ, st, x, y in its if st == 'kIdle')
print(f'  item-snapshots lying idle: {idle_far} (x 0.1 s = {idle_far * 0.1:.0f} item-seconds waiting to be collected)')

# ---------------- 6. Speech ----------------
print('\n== 6. Speech in battle')
bs = [s for s in says if t_start <= s[0] <= t_end]
print(f'  {len(bs)} lines, {len(bs) / dur * 60:.1f}/min, {sum(1 for s in bs if s[1].startswith("!"))} interrupting')
kinds = collections.Counter(s[1].lstrip('!').split(',')[0].split('.')[0][:28] for s in bs)
for k, n in kinds.most_common(12):
    print(f'  {n:4d}  {k}')

# ---------------- 7. Aim ----------------
print('\n== 7. Aim')
tg = [s for t, s in snaps if t_start <= t <= t_end and s[13]]
changes = sum(1 for a, b in zip(tg, tg[1:]) if a[13].split('@')[0] != b[13].split('@')[0] or abs(fnum(a[13].split('@')[1].split(',')[0]) - fnum(b[13].split('@')[1].split(',')[0])) > 0.6)
print(f'  target set {len(tg) / len(play) * 100:.0f}% of the time; target changes {changes} ({changes / dur * 60:.1f}/min); retarget clicks {by_name.get("retarget", 0)}')
kills = [(t, a) for t, w, a in evs if w == 'kills' and t_start <= t <= t_end]
print(f'  kills {sum(int(fnum(a[1])) for _, a in kills)}')

# ---------------- 8. State timeline ----------------
print('\n== 8. Timeline')
for t, w, a in evs:
    if w in ('state', 'level', 'start'):
        print(f'  {t - t_start:+7.0f}s {w} {" ".join(a)}')
