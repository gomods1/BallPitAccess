# BALL x PIT Access

Ein Mod, der **BALL x PIT** (Kenny Sun / Devolver Digital) ohne Sehen spielbar macht. Er liest jedes Menü
über deinen Screenreader vor, lässt dich das ganze Spiel mit der Tastatur bedienen und verwandelt das
Schlachtfeld in Klang: Gegner, deine Bälle, gegnerische Schüsse, EP-Juwelen und besondere Gegenstände haben
jeweils ihr eigenes Geräusch, das von ihrer Seite aus zu dir kommt.

Version 0.5.0, für die Spielversion 1.308, einschließlich des DLC Aufgestiegenes Ballbylon (Risen Ballbylon).
Der Mod spricht **jede Sprache, die das Spiel anbietet**, und folgt der Spracheinstellung des Spiels.

<!-- LANGS -->
[English](README.html) · [Español](README.es.html) · [Français](README.fr.html) · **Deutsch** · [Italiano](README.it.html) · [Português (Brasil)](README.pt-BR.html) · [Polski](README.pl.html) · [Türkçe](README.tr.html) · [Русский](README.ru.html) · [Українська](README.uk.html) · [日本語](README.ja.html) · [한국어](README.ko.html) · [简体中文](README.zh-CN.html) · [繁體中文](README.zh-TW.html) · [ไทย](README.th.html)
<!-- /LANGS -->

> **Stand: frühe Testversion.** Die Menüs, die Basis und die ersten Durchläufe wurden mit dem Mod gespielt.
> Die Kampfgeräusche und die neuesten Teile (durchgehendes Ballgeräusch, Fanghilfe, Soundfilter) wurden
> noch nicht mit Spielern abgestimmt. Bitte melde, was funktioniert und was nicht (siehe
> [Ein Problem melden](#reporting-a-problem)).

## Inhalt {#contents}

- [Was du brauchst](#what-you-need)
- [Installation](#installing)
- [Den Mod entfernen](#removing-the-mod)
- [Erste Schritte](#first-steps)
- [Tasten überall](#keys-everywhere)
- [Menüs](#menus)
- [Die Basis (Neu-Ballbylon)](#the-base-new-ballbylon)
- [Kampf](#battle)
- [Kampfgeräusche](#battle-sounds)
- [Soundfilter (Zifferntasten)](#sound-filters-number-keys)
- [Hilfen](#assists)
- [Mod-Einstellungen (F4)](#mod-settings-f4)
- [Tipps für den ersten Durchlauf](#tips-for-a-first-run)
- [Bekannte Einschränkungen](#known-limits)
- [Ein Problem melden](#reporting-a-problem)
- [Mitwirkende](#credits)

## Was du brauchst {#what-you-need}

- BALL x PIT unter Windows (Steam), Version 1.308.
- Einen Screenreader: Getestet ist NVDA. Andere, die von der Sprachbibliothek Prism unterstützt werden
  (JAWS, Sprachausgabe/Narrator, SAPI-Stimmen), sollten ebenfalls funktionieren.
- **MelonLoader 0.7.3** (x64), den Mod-Loader. Kostenlos, von seiner offiziellen Releases-Seite auf GitHub.
- Kopfhörer oder Stereolautsprecher: Links und rechts sind im Kampf sehr wichtig.

## Installation {#installing}

**Mit dem Installer (am einfachsten):** Lade `BallPitAccessibilityInstaller.exe` aus der
[neuesten Version](https://github.com/gomods1/BallPitAccess/releases/latest) herunter und starte ihn. Er findet das Spiel über Steam, installiert
MelonLoader 0.7.3, die Sprachbibliothek Prism und den Mod und kann sie später aktualisieren oder
entfernen. Starte danach das Spiel: Der erste Start dauert einige Minuten und bleibt stumm, während
MelonLoader die Spieldateien vorbereitet; dann beginnt dein Screenreader zu sprechen.

**Von Hand:** Lade `BallPitAccess-vX.Y.Z.zip` von derselben Seite herunter und entpacke alles in den
Spielordner (standardmäßig `C:\Program Files (x86)\Steam\steamapps\common\BALLxPIT`, der Ordner mit `Balls.exe`), sodass `version.dll` neben `Balls.exe`
liegt. Das Zip enthält MelonLoader bereits: Du musst ihn nicht extra installieren. Starte das Spiel; der
erste Start ist langsam und stumm, wie oben.

## Den Mod entfernen {#removing-the-mod}

Die Schaltfläche Deinstallieren im Installer entfernt den Mod und MelonLoader und behält deine
Einstellungen in `UserData`. Von Hand:

Lösche `Mods\BallPitAccess.dll` und `prism.dll`. Um auch MelonLoader zu entfernen, lösche `version.dll` und
die Ordner `MelonLoader`, `Mods`, `Plugins` und `UserData` aus dem Spielordner. Der Mod verändert keine
Datei des Spiels.

## Erste Schritte {#first-steps}

- Drücke jederzeit **F1** für die Hilfe: alle Teile, beginnend mit dem Teil für den Ort, an dem du gerade
  bist. Pfeil nach oben und unten lesen sie Zeile für Zeile, Tab springt zum nächsten Teil, Esc schließt.
- Drücke **F4** für die Einstellungen des Mods und öffne darin den **Klangführer**: Er spielt jedes
  Kampfgeräusch und sagt, was es bedeutet. Ihn einmal vor dem ersten Durchlauf anzuhören, hilft sehr.
- Das Spiel selbst wird mit Tastatur über die Maus bedient; mit dem Mod brauchst du die Maus nie, außer für
  den Leerlauf-Werfer in der Basis (siehe [Bekannte Einschränkungen](#known-limits)).

## Tasten überall {#keys-everywhere}

| Taste | Aktion |
|---|---|
| F1 | Hilfe (alle Teile; Tab / Umschalt+Tab zwischen den Teilen) |
| F2 | Den ganzen Bildschirm vorlesen |
| F3 | Das zuletzt Gesagte wiederholen |
| F4 | Mod-Einstellungen und Klangführer |
| F5 | Modus der Zielhilfe wechseln |
| F6 / F7 | Das Ziel um 10 Grad nach links / rechts drehen |
| F8 | Sagen, wohin du zielst |
| Strg+Pfeil nach oben | Die Details des fokussierten Eintrags lesen (Strg+Pfeil nach unten geht zurück) |
| Strg+Pfeil nach links / Strg+Pfeil nach rechts | Zwischen Details und den letzten Ereignissen wechseln |

Solange eine Liste des Mods (Hilfe, Einstellungen, Klangführer) offen ist, bekommt das Spiel deine Tasten
nicht.

## Menüs {#menus}

Jeder Bildschirm ist **eine einzige Liste in sinnvoller Reihenfolge**: zuerst seine Auswahlmöglichkeiten,
dann seine Schaltflächen, dann Schließen. Die Reihenfolge hängt nie davon ab, wo die Dinge auf dem
Bildschirm gezeichnet sind.

| Taste | Aktion |
|---|---|
| Pfeil nach oben / unten | Vorheriger / nächster Eintrag |
| Eingabe | Auswählen |
| Tab / Umschalt+Tab | Nächste / vorherige Gruppe des Bildschirms |
| Pos1 / Ende | Erster / letzter Eintrag der Gruppe |
| Pfeil nach links / rechts | Eine Listen- oder Schiebereglereinstellung ändern; auf einem Level seine Schwierigkeit ändern |
| Strg+Tab / Strg+Umschalt+Tab | Nächster / vorheriger Reiter auf Bildschirmen mit Reitern |
| Rücktaste oder Esc | Zurück |
| Bild auf / Bild ab | Set im Lexikon; Neues-Spiel-plus-Stufe in der Levelliste |
| Umschalt+Eingabe | Auf einem Level: seine Bestenliste öffnen |

- Eingabe auf einem Charakter bringt dich zu seiner Schaltfläche Auswählen.
- Wenn das geführte Tutorial auf etwas zeigt, springt der Fokus dorthin: Drücke Eingabe.
- Aufwertungen werden mit ihren Zahlen vorgelesen („Verursacht 10 bis 15 Schaden“).

## Die Basis (Neu-Ballbylon) {#the-base-new-ballbylon}

| Taste | Aktion |
|---|---|
| Pfeil nach oben / unten, Eingabe | Schaltflächen der Seitenleiste: Spielen, Ernten, Bauen, Erweitern, Neu anordnen, Optionen |
| Tab / Umschalt+Tab | Durch den Aufzug und deine Gebäude gehen |
| Eingabe auf einem Gebäude | Eine Ernte mit Ziel auf das Gebäude starten, wenn es bereit ist; sonst seine Aufwertung öffnen |
| Umschalt+Eingabe | Die Aufwertung eines Gebäudes öffnen |
| Eingabe auf dem Aufzug | Die Aufzugsaufwertung öffnen (hinab in die Grube oder hinauf zum Aufgestiegenen Ballbylon) |
| Entf | Ein Gebäude demontieren |
| H | Rohstoffe und Tag |
| J | Ernte und Arbeiter |

- **Bauen**: Die Pfeiltasten bewegen das Gebäude um ein Feld und sagen, ob es passt, Eingabe baut, R dreht,
  Pos1 springt zu einem freien Platz, Rücktaste bricht ab.
- **Ernte**: Sie beginnt mit Ziel auf ein Gebäude. Eingabe schickt die Arbeiter los, Tab zielt auf ein
  anderes Gebäude, Pfeil nach links / rechts drehen das Ziel, Leertaste halten beschleunigt, Esc verlässt den
  Modus.
- **Erweitern**: Tab wählt ein Grundstück, Eingabe kauft es.
- **Neu anordnen**: Tab wählt ein Gebäude, Eingabe hebt es auf, die Pfeiltasten bewegen es, Eingabe setzt es
  ab.

## Kampf {#battle}

### Die Tasten des Spiels

| Taste | Aktion |
|---|---|
| W A S D | Bewegen |
| Leertaste | Schießen (halten) |
| F | Autoschuss an / aus |
| E | Einen Stufenaufstieg öffnen, wenn einer bereit ist |
| = / - | Schnellere / langsamere Spielgeschwindigkeit |
| Esc | Pause |

### Tasten des Mods

| Taste | Aktion |
|---|---|
| Pfeil nach links / rechts | Das Ziel von Hand um 5 Grad drehen |
| Pfeil nach oben | Das Zielen an die Zielhilfe zurückgeben |
| Pfeil nach unten | Sagen, wohin du zielst |
| Umschalt (halten) | Fanghilfe (siehe [Hilfen](#assists)) |
| 1 bis 7, 0 | Soundfilter (siehe [Soundfilter](#sound-filters-number-keys)) |
| H | Leben und Statuseffekte |
| J | Gegner und Boss |
| K | Deine Position |
| G | Spielgeschwindigkeit |
| L | Stufe, Zeit, Welle, gewonnene Zahnräder |
| B | Deine Bälle und passiven Fähigkeiten |
| P | Gegenstände auf dem Feld |
| N | Gefahren (Schüsse, Laser, Bomben, Boden) |

Im Kampf wird angesagt: das Level zu Beginn, Bosse und ihr Leben bei 75, 50 und 25 Prozent, Schaden und
Leben, Statuseffekte, besondere Gegenstände, wenn sie fallen, „Stufenaufstiege bereit, drücke E“, das Ende
des Durchlaufs.

## Kampfgeräusche {#battle-sounds}

Vier Regeln gelten für jedes Geräusch:

- **Seite**: Ein Geräusch kommt von der Seite des Dings, um das es geht.
- **Tonhöhe**: Höher heißt näher.
- **Gedämpft** heißt, es ist hinter dir.
- Ein Geräusch, das dich zum **Gehen** auffordert, kommt von der Seite, zu der du gehen sollst.

Dinge, die sich ständig bewegen (dein Ball, gegnerische Schüsse), sind **durchgehende Geräusche**, die ihnen
folgen; Dinge, die sich schrittweise ändern (vorrückende Gegner, am Boden liegende Gegenstände), sind kurze
Geräusche.

| Geräusch | Bedeutung |
|---|---|
| Handtrommel | Ein Gegner. Höher und schneller = näher. Die drei nächsten spielen abwechselnd. |
| Große, tiefe Trommel | Ein Boss. Trommeln, Gong und Horn, wenn einer erscheint. |
| Knurren | Ein Gegner erreicht dich gleich. |
| Ansteigendes Jaulen | Ein Gegner lädt einen Angriff auf, auf seiner Seite. |
| **Rollendes Summen** (durchgehend) | Der Ball, den du fangen kannst. Geh zu seiner Seite; es **steigt, je näher du der Stelle kommst, an der der Ball landen wird**, pulsiert schneller, je näher der Ball dir kommt, und ist leiser, solange der Ball noch von einem Gegner abprallen kann. |
| Klick, wenn das Summen eine Stufe steigt | Du bist unter dem Ball: Bleib dort. |
| Handschuh-Plumps | Ball gefangen. |
| Dumpfer, absinkender Plumps | Ein Ball ist an dir vorbei. |
| Gewehrständer | Deine Bälle sind zurück: Du kannst schießen (stumm mit Autoschuss). |
| Glasklingen | Das nächste EP-Juwel, Gold oder der nächste Rohstoff, zu dem du gehen kannst. Sie sammeln sich nicht von selbst ein. |
| Kalimba-Funkeln | Ein besonderer Gegenstand: Fusionsreaktor, Bauplan, Ei, Zahnrad, Leben, Magnet, Bombe, Juwelenbeutel. |
| **Luftiges Pfeifen** (durchgehend) | Ein gegnerischer Schuss, der nah an dir vorbeifliegt, höher, je näher er kommt. Lauter, wenn er dich treffen wird. |
| Rauschen und Knall | Ein Schuss wird dich treffen: Geh zu der Seite, auf der es klingt. |
| Elektrisches Brummen / zweimal Knistern | Ein Laser zielt / du bist in seiner Linie: Geh zu der Seite, auf der es klingt. |
| Fallendes Pfeifen / Zündschnurzischen | Eine Bombe fällt in der Nähe / sie wird auf dir landen: Geh zu der Seite, auf der es klingt. |
| Quatschen | Du bist auf eine Bodengefahr getreten (ihr Name wird angesagt). |
| Schlag / Schlucken | Schaden / Heilung. |
| Herzschlag | Wenig Leben; schneller, wenn sehr wenig. |
| Taumelndes Wabern | Ein Statuseffekt hat dich erwischt. |
| Zwei Glockentöne aufwärts | Ein Stufenaufstieg ist bereit (drücke E). |
| Holzklopfen | Seitlicher Rand der Arena (auch das Ende einer Menüliste). |
| Metallklopfen | Vordere oder hintere Grenze deines Laufbereichs. |
| Schalterklick | Die Zielhilfe hat das Ziel gewechselt, auf der Seite des neuen Ziels. |

Jedes dieser Geräusche kannst du unter **F4 → Klangführer** hören, mit Pfeil nach links / rechts auf jeder
Seite.

## Soundfilter (Zifferntasten) {#sound-filters-number-keys}

Im Kampf schalten die Zifferntasten eine Gruppe von Geräuschen stumm oder wieder ein, damit du dich
konzentrieren kannst. Das Spiel benutzt keine dieser Tasten.

| Taste | Gruppe |
|---|---|
| 1 | Gegner: Trommeln, Knurren, Aufladen, Erscheinen eines Bosses |
| 2 | Bälle: rollendes Summen, Klick, Fang, Fehlgriff, Gewehrständer |
| 3 | Belohnungen: Juwelen, Gold, Rohstoffe, besondere Gegenstände |
| 4 | Schüsse und Gefahren |
| 5 | Arenaränder |
| 6 | Dein Leben: Schaden, Heilung, Herzschlag, Status |
| 7 | Klick der Zielhilfe |
| 0 | Alles wieder an |

- Jede Ziffer schaltet ihre eigene Gruppe um, unabhängig von den anderen.
- **Strg+Ziffer** behält nur diese Gruppe. Beispiel: nur Gegner und Belohnungen = Strg+1, dann 3.
- Deine Wahl bleibt für die nächste Sitzung erhalten. Wenn ein Kampf mit stummgeschalteten Gruppen beginnt,
  sagt der Mod, was stumm ist.
- Stummschalten ändert nur die Geräusche: Die Tasten H, J, P, N antworten weiterhin, und die Fanghilfe
  weicht weiterhin aus.

## Hilfen {#assists}

Alles, was eine Hilfe tut, kannst du auch nach Gehör tun; die Hilfen sind da, wenn du sie willst.

- **Zielhilfe** (F5 wechselt den Modus): zielt für dich auf den nächsten Gegner (Standard), den stärksten
  Gegner, den Gegner vor dir, einen festen Winkel, oder ist aus (Maus). Die Pfeiltasten justieren sie.
- **Fanghilfe** (Umschalt halten): Deine Figur läuft unter den nächsten herunterkommenden Ball; wenn kein
  Ball kommt, läuft sie zum nächsten Juwel oder Gegenstand; wenn dich gleich ein Schuss trifft, weicht sie
  zuerst aus. W / S bewegen dich weiterhin nach vorn und hinten, A / D übernehmen immer. In F4 kann sie aus,
  „Umschalt halten“ (Standard) oder automatisch sein.

## Mod-Einstellungen (F4) {#mod-settings-f4}

Pfeil nach oben / unten bewegen, Pfeil nach links / rechts oder Eingabe ändern, Esc schließt. Die
Einstellungen werden gespeichert.

- Modus der Zielhilfe
- Fanghilfe: aus, Umschalt halten, automatisch
- Audiosignale an / aus, Lautstärke der Signale
- Gegnerradar, Ballgeräusche, Arenaränder, Schüsse und Gefahren (an / aus)
- Geräusche für Gegenstände: aus, nur besondere Gegenstände, alle
- Lebensgeräusche, Klick der Zielhilfe (an / aus)
- Aufwertungsbeschreibungen beim Fokus vorlesen
- Klangführer (Eingabe öffnet)
- Debug-Protokoll

## Tipps für den ersten Durchlauf {#tips-for-a-first-run}

- **Fang deine Bälle.** Ein gefangener Ball kann sofort wieder abgeschossen werden; ein verpasster braucht 2
  bis 3 Sekunden, bis er zurückkommt. Folge dem rollenden Summen, bis du den Klick hörst, und bleib dann
  stehen.
- **Sammle deine EP-Juwelen ein.** Sie sammeln sich nicht von selbst ein. Folge dem Glasklingen; gedämpft
  heißt, es ist hinter dir.
- **Erst überleben.** Am Anfang eines Durchlaufs ist am Leben bleiben wichtiger als Schaden.
- **Fusioniere und entwickle** deine Bälle, wenn der Stufenaufstieg es anbietet: Die Auswahl sagt „Fusion“
  oder „Evolution“.
- **Werde bei Bossen langsamer**, mit der Minustaste. Der Mod erinnert dich daran, wenn ein Boss bei hoher
  Geschwindigkeit erscheint.
- Sterben ist normal: Das Gold, das du mitbringst, baut Neu-Ballbylon auf, und Gebäude machen jeden späteren
  Durchlauf stärker.

## Bekannte Einschränkungen {#known-limits}

- Der Leerlauf-Werfer in der Basis braucht noch die Maus.
- Bestenlisten und die Twitch-Abstimmung werden nur einfach vorgelesen. Der Abspann lässt sich nicht
  überspringen.
- Die Mod-Einstellungen liegen auf F4, nicht im Optionsbildschirm des Spiels.
- Die eigenen Texte des Mods in anderen Sprachen als Englisch und Spanisch wurden übersetzt, ohne dass sie
  bisher von Muttersprachlern geprüft wurden. Korrekturen sind willkommen. Namen aus dem Spiel (Gebäude,
  Bälle, Gegner, Aufwertungen) kommen immer aus der Übersetzung des Spiels selbst.
- Lautstärken, Wiederholraten und die Fangbreite wurden noch nicht mit Spielern abgestimmt.

## Ein Problem melden {#reporting-a-problem}

Nützliche Dateien im Spielordner:

- `UserData\BallPitAccess\speech.log`: alles, was der Mod gesagt hat.
- `MelonLoader\Latest.log`: das Protokoll des Mods (Fehler, Bildschirme, Fänge und Fehlgriffe).
- Im Kampf schreibt **Strg+Umschalt+D** die Datei `UserData\BallPitAccess\threats.txt` (Gegner, Bälle,
  Schüsse, Gegenstände mit Positionen) und die Liste des offenen Bildschirms in `Latest.log`: nützlich, wenn
  ein Geräusch in die falsche Richtung zeigt oder ein Menü in der falschen Reihenfolge vorgelesen wird.

Sag, was du getan hast, was du gehört hast und was du erwartet hast.

## Mitwirkende {#credits}

- BALL x PIT von Kenny Sun, veröffentlicht von Devolver Digital. Dieser Mod ist inoffiziell.
- MelonLoader (LavaGang) lädt den Mod. Prism schickt die Sprachausgabe an deinen Screenreader.
