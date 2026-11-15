"""Write the genny batch for every audio cue of the mod and render it.

Usage: python tools/make_cues.py [--no-render]
Output: src/BallPitAccess/Sounds/cues.json + one WAV per cue (48 kHz mono), embedded in the DLL.
A WAV with the same name in UserData/BallPitAccess/sounds/ overrides the embedded one at load.

Design rule (CUES.md): one physical family per category, so the timbre names the category and the
pitch / pan / rate carry the position:
  enemies = hand drum (conga), boss = big drum (taiko)        pitch = closeness
  your balls = steel ball clanks, catch = glove thup, ready = gun rack
  gems = glass tink, special pickups = kalimba sparkle          pitch = height up the field
  shots = air swoosh, lasers = electric buzz, bombs = whistle / fuse, floor = squelch
  you = punch (hurt), gulp (heal), heartbeat, woozy wobble (status)
  edges = door knock (wood at the sides, metal at front / back)
"""
import json
import os
import subprocess
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "src", "BallPitAccess", "Sounds")
STEPS = 10  # Cues.EnemySteps

MONO = {"type": "mono"}
sounds = []


def add(name, layers, fx=None, normalize=-6.0, duration=None, max_loudness=None):
    spec = {"out": name + ".wav", "layers": layers, "fx": (fx or []) + [MONO], "normalize": normalize}
    if duration:
        spec["duration"] = duration
    if max_loudness is not None:
        spec["max_loudness"] = max_loudness
    sounds.append(spec)


def fade(out):
    return {"type": "fade", "out": out}


# ---------------- Enemies: radar ----------------
# Conga, slapped so the attack is crisp (pans well), 150 Hz far .. ~420 Hz at the attack line.
for i in range(STEPS):
    tune = round(150 * 2 ** (i / (STEPS - 1) * 1.5))
    add(f"enemy{i}", [
        {"type": "drum", "kind": "conga", "params": {"tune": tune, "slap": 0.35}},
    ], fx=[fade(0.04)], duration=0.16, normalize=-9)
    # Boss: a big taiko with a stick crack on top, 55 .. ~125 Hz.
    btune = round(55 * 2 ** (i / (STEPS - 1) * 1.2))
    add(f"boss{i}", [
        {"type": "drum", "kind": "taiko", "params": {"tune": btune, "decay": 0.3}},
        {"type": "drum", "kind": "tom", "params": {"tune": btune * 2, "decay": 0.12}, "gain": 0.5},
        {"type": "drum", "kind": "rim", "gain": 0.25},
    ], fx=[fade(0.06)], duration=0.3, normalize=-5)

# An enemy is on you: a short monster snarl.
add("danger", [
    {"type": "sfx", "kind": "growl", "params": {"dur": 0.45, "size": 0.55, "seed": 3}},
    {"type": "drum", "kind": "conga", "params": {"tune": 420, "slap": 1.0}, "gain": 0.6},
], fx=[fade(0.08)], normalize=-1)

# An enemy winds up an attack: an energy whine climbing.
add("charge", [
    {"type": "sfx", "kind": "charge", "params": {"dur": 0.4, "seed": 2}},
], fx=[{"type": "lowpass", "cutoff": 3500}, fade(0.05)], normalize=-6)

# Boss arrives: two taiko hits, a gong and a low horn.
add("boss", [
    {"type": "drum", "kind": "taiko", "params": {"tune": 55, "decay": 0.9}},
    {"type": "drum", "kind": "taiko", "params": {"tune": 50, "decay": 1.0}, "at": 0.28},
    {"type": "drum", "kind": "gong", "params": {"tune": 80, "decay": 2.0}, "at": 0.28, "gain": 0.5},
    {"type": "seq", "inst": "french_horn", "steps": "D3:0.25 -:0.03 A2:0.9", "at": 0.05, "gain": 0.55},
], fx=[{"type": "reverb", "mix": 0.2, "size": 0.6}, fade(0.4)], normalize=-2, duration=1.8)

# ---------------- Your balls ----------------
# A steel ball coming back: a light metal clank. "inline": it lands on you, a double clank, harder.
add("ball", [
    {"type": "sfx", "kind": "steel_ball", "params": {"surface": "metal", "force": 0.45, "seed": 4}},
], fx=[{"type": "highpass", "cutoff": 900}, {"type": "lowpass", "cutoff": 4500}, fade(0.03)], duration=0.09, normalize=-4)
add("ball_inline", [
    {"type": "sfx", "kind": "steel_ball", "params": {"surface": "metal", "force": 0.9, "seed": 5}},
    {"type": "sfx", "kind": "steel_ball", "params": {"surface": "metal", "force": 0.9, "seed": 6}, "at": 0.045},
], fx=[{"type": "highpass", "cutoff": 700}, {"type": "lowpass", "cutoff": 5000}, fade(0.03)], duration=0.13, normalize=-2)
# The ball to catch, continuous: a steel ball rolling plus a steady hum so its pitch is easy to follow.
# Played as a loop (Cues crossfades the tail into the head); the mod bends pitch, pan, volume and a throb.
# Layers run past the cut so the loop never contains an attack or a release.
add("loop_ball", [
    {"type": "sfx", "kind": "ball_roll", "params": {"speed": 0.75, "dur": 2.6, "seed": 4}, "gain": 0.8},
    {"type": "synth", "inst": "synth", "notes": "E4", "dur": 2.6, "vel": 0.6,
     "params": {"wave": "triangle", "attack": 0.01, "decay": 0.05, "sustain": 1.0, "release": 0.3, "cutoff": 2400, "detune": 0.0}},
    {"type": "synth", "inst": "sine", "notes": "E5", "dur": 2.6, "vel": 0.25},
], duration=2.05, normalize=-8)
# An enemy shot in the air, continuous: an airy whistle over a rush of wind (the shots' air family).
add("loop_shot", [
    {"type": "synth", "inst": "flute", "notes": "B5", "dur": 2.6, "vel": 0.6, "params": {"breath": 0.35}},
    {"type": "sfx", "kind": "wind_howl", "params": {"dur": 2.6, "gust": 0.2, "seed": 3}, "gain": 0.5},
], fx=[{"type": "highpass", "cutoff": 400}], duration=2.05, normalize=-10)
# Missed: the ball got past you and hit the floor: a dull steel-on-wood thud sinking away.
add("ball_miss", [
    {"type": "sfx", "kind": "steel_ball", "params": {"surface": "wood", "force": 0.6, "seed": 12}},
    {"type": "sfx", "kind": "sweep_down", "params": {"start": 500, "end": 160, "dur": 0.25, "wave": "triangle"}, "at": 0.03, "gain": 0.35},
], fx=[{"type": "lowpass", "cutoff": 1800}, fade(0.08)], duration=0.32, normalize=-5)
# Caught: a ball smacking into a glove.
add("catch", [
    {"type": "sfx", "kind": "steel_ball", "params": {"surface": "rubber", "force": 1.0, "seed": 2}},
    {"type": "sfx", "kind": "punch", "params": {"dur": 0.12}, "gain": 0.5},
    {"type": "sfx", "kind": "steel_ball", "params": {"surface": "metal", "force": 0.3, "seed": 9}, "at": 0.03, "gain": 0.4},
], fx=[fade(0.05)], normalize=-2)
# All balls are back, you can shoot: a gun rack.
add("ready", [
    {"type": "sfx", "kind": "reload", "params": {"action": "rack", "seed": 26}},
], fx=[fade(0.05)], normalize=-2)

# ---------------- Pickups ----------------
# Gems / gold / resources: one glass tink. Height 0 (your row) .. 2 (far up the field).
for i, note in enumerate(["A5", "D6", "G6"]):
    add(f"pickup{i}", [
        {"type": "synth", "inst": "glass", "notes": note, "dur": 0.08, "vel": 0.6},
        {"type": "sfx", "kind": "water_drip", "params": {"pitch": 1.6, "seed": i}, "gain": 0.25},
    ], fx=[{"type": "lowpass", "cutoff": 6000}, fade(0.08)], duration=0.3, normalize=-7)
# Special pickups: a quick kalimba sparkle, three notes up, with a bag rustle under it.
for i, root in enumerate(["C5", "F5", "A#5"]):
    from_midi = {"C5": 72, "F5": 77, "A#5": 82}[root]
    steps = " ".join(f"{from_midi + d}:0.05" for d in (0, 4, 7)) + f" {from_midi + 12}:0.15"
    add(f"treasure{i}", [
        {"type": "seq", "inst": "kalimba", "steps": steps, "vel": 0.75},
        {"type": "sfx", "kind": "keys", "params": {"dur": 0.2, "shakes": 1, "seed": 12 + i}, "gain": 0.3},
    ], fx=[{"type": "reverb", "mix": 0.12, "size": 0.3}, fade(0.1)], duration=0.42, normalize=-6)

# ---------------- Hazards ----------------
# A shot passing near: air swoosh. "_hit": it is coming at you: swoosh + a crack, louder.
add("shot", [
    {"type": "sfx", "kind": "whoosh", "params": {"dur": 0.16, "low": 500, "high": 2800, "direction": "down"}},
], fx=[{"type": "lowpass", "cutoff": 4000}, fade(0.04)], normalize=-8)
add("shot_hit", [
    {"type": "sfx", "kind": "whoosh", "params": {"dur": 0.14, "low": 800, "high": 5000, "direction": "up"}},
    {"type": "drum", "kind": "snap", "at": 0.1},
    {"type": "sfx", "kind": "hit", "params": {"dur": 0.1, "tone": 300, "crunch": 0.6}, "at": 0.1, "gain": 0.6},
], fx=[{"type": "lowpass", "cutoff": 4500}, fade(0.04)], normalize=-2)
# A laser aiming: an electric hum. "_hit": you are in it: a crackling zap, twice.
add("beam", [
    {"type": "sfx", "kind": "zap", "params": {"dur": 0.3, "freq": 90}},
], fx=[{"type": "lowpass", "cutoff": 3000}, {"type": "fade", "in": 0.05, "out": 0.08}], normalize=-8)
add("beam_hit", [
    {"type": "sfx", "kind": "zap", "params": {"dur": 0.1, "freq": 160}},
    {"type": "sfx", "kind": "zap", "params": {"dur": 0.1, "freq": 160}, "at": 0.13},
], fx=[{"type": "lowpass", "cutoff": 4000}, fade(0.03)], normalize=-3)
# A bomb lobbed nearby: a falling whistle. "_hit": you are in its blast: a fuse sizzle and a thump.
add("bomb", [
    {"type": "sfx", "kind": "sweep_down", "params": {"start": 1700, "end": 700, "dur": 0.32, "wave": "sine"}},
], fx=[{"type": "vibrato", "rate": 7, "depth": 0.2}, fade(0.06)], normalize=-12, max_loudness=-14)
add("bomb_hit", [
    {"type": "sfx", "kind": "fire", "params": {"dur": 0.22, "crackle": 1.0, "seed": 4}},
    {"type": "sfx", "kind": "static", "params": {"dur": 0.2, "color": "white"}, "gain": 0.35,
     "fx": [{"type": "bandpass", "center": 4500, "q": 2}]},
    {"type": "drum", "kind": "kick", "params": {"tune": 50, "decay": 0.2}, "at": 0.18},
], fx=[fade(0.05)], normalize=-3)
# Standing on a floor hazard: a squelch.
add("floor", [
    {"type": "sfx", "kind": "footfall", "params": {"surface": "water", "steps": 1, "weight": 0.8}},
    {"type": "sfx", "kind": "bubble", "params": {"freq": 180}, "at": 0.04, "gain": 0.5},
], fx=[fade(0.06)], normalize=-2)

# ---------------- You ----------------
add("hurt", [
    {"type": "sfx", "kind": "punch", "params": {"dur": 0.2}},
    {"type": "sfx", "kind": "hit", "params": {"dur": 0.22, "tone": 140, "crunch": 0.4}, "gain": 0.6},
], fx=[fade(0.05)], normalize=-2)
add("heal", [
    {"type": "sfx", "kind": "gulp", "params": {"seed": 1}},
    {"type": "sfx", "kind": "magic", "params": {"dur": 0.35, "density": 6, "freq": "E5"}, "at": 0.2, "gain": 0.4},
], fx=[fade(0.08)], normalize=-3)
add("heart", [
    {"type": "sfx", "kind": "heartbeat", "params": {"rate": 1.4, "dur": 0.6}},
], fx=[{"type": "lowpass", "cutoff": 900}, fade(0.08)], normalize=-2)
# A status effect landed: a woozy falling wobble.
add("status", [
    {"type": "sfx", "kind": "sweep_down", "params": {"start": 700, "end": 250, "dur": 0.35, "wave": "sine"}},
    {"type": "sfx", "kind": "bubble", "params": {"freq": 300}, "gain": 0.4},
], fx=[{"type": "vibrato", "rate": 9, "depth": 0.8}, {"type": "lowpass", "cutoff": 2500}, fade(0.08)], normalize=-6)

# ---------------- Progress ----------------
# A level up is waiting: two soft vibraphone notes up.
add("xp", [
    {"type": "seq", "inst": "vibraphone", "steps": "G5:0.09 C6:0.3", "vel": 0.7},
], fx=[{"type": "reverb", "mix": 0.15, "size": 0.35}], normalize=-6, duration=0.6)
# The level-up menu opened: a harp sweep up.
add("levelup", [
    {"type": "seq", "inst": "harp", "steps": "C5:0.035 E5:0.035 G5:0.035 C6:0.035 E6:0.3", "vel": 0.7},
], fx=[{"type": "reverb", "mix": 0.2, "size": 0.45}], normalize=-6, duration=0.8)
add("win", [
    {"type": "seq", "inst": "brass", "steps": "C4:0.12 E4:0.12 G4:0.12 [C4,E4,G4,C5]:0.9", "vel": 0.85},
    {"type": "drum", "kind": "taiko", "params": {"tune": 65, "decay": 0.8}, "at": 0.36, "gain": 0.6},
    {"type": "drum", "kind": "crash", "at": 0.36, "gain": 0.3},
], fx=[{"type": "reverb", "mix": 0.22, "size": 0.6}, fade(0.4)], normalize=-2, duration=2.0)
add("lose", [
    {"type": "seq", "inst": "felt_piano", "steps": "G4:0.3 D#4:0.3 C4:0.3 [G3,C4,D#4]:1.2", "vel": 0.75},
    {"type": "drum", "kind": "gong", "params": {"tune": 70, "decay": 2.0}, "at": 0.9, "gain": 0.4},
], fx=[{"type": "reverb", "mix": 0.25, "size": 0.7}, fade(0.5)], normalize=-2, duration=2.4)

# ---------------- Edges and UI ----------------
add("wall", [
    {"type": "sfx", "kind": "knock", "params": {"count": 1, "surface": "wood", "force": 0.7, "seed": 10}},
], fx=[fade(0.05)], normalize=-5, duration=0.25)
add("wall_v", [
    {"type": "sfx", "kind": "knock", "params": {"count": 1, "surface": "metal", "force": 0.6, "seed": 11}},
], fx=[fade(0.05)], normalize=-6, duration=0.3)
add("retarget", [
    {"type": "sfx", "kind": "switch", "params": {"state": "on", "seed": 7}},
], normalize=-6)
add("tick", [
    {"type": "sfx", "kind": "solenoid", "params": {"size": 0.0, "metal": 0.2, "thump": 0.3}},
], fx=[fade(0.03)], normalize=-2, duration=0.12)

batch = {"out_dir": OUT, "sr": 48000, "sounds": sounds}
os.makedirs(OUT, exist_ok=True)
path = os.path.join(OUT, "cues.json")
with open(path, "w", encoding="utf-8") as f:
    json.dump(batch, f, indent=1)
print(f"{len(sounds)} specs -> {path}")
if "--no-render" not in sys.argv:
    sys.exit(subprocess.call(["genny", "render", path, "--quiet"]))
