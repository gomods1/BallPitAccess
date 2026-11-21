# BALL x PIT Access

Ein Mod, der **BALL x PIT** (Kenny Sun / Devolver Digital) ohne Sehen spielbar macht. Er liest jedes Menü
über deinen Screenreader vor, lässt dich das ganze Spiel mit der Tastatur bedienen und verwandelt das
Schlachtfeld in Klang: Gegner, deine Bälle, gegnerische Schüsse, EP-Juwelen und besondere Gegenstände haben
jeweils ihr eigenes Geräusch, das von ihrer Seite aus zu dir kommt.

Version 0.7.0, für die Spielversion 1.308, einschließlich des DLC Aufgestiegenes Ballbylon (Risen Ballbylon).
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
- [Die Tasten ändern](#changing-keys)
- [Controller](#controller)
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
| K | Die Basis Feld für Feld erkunden |

- **Bauen**: Die Pfeiltasten bewegen das Gebäude um ein Feld und sagen, ob es passt, Eingabe baut, R dreht,
  Pos1 springt zu einem freien Platz, Rücktaste bricht ab.
- **Ernte**: Sie beginnt mit Ziel auf ein Gebäude. Eingabe schickt die Arbeiter los, Tab zielt auf ein
  anderes Gebäude, Pfeil nach links / rechts drehen das Ziel, Leertaste halten beschleunigt, Esc verlässt den
  Modus.
- **Bau**: Gebäude im Bau oder in der Aufwertung stehen auch in der Tab-Liste der Ernte. Jeder Arbeiter, der von einem abprallt, baut daran; die Mod sagt, wenn es fertig ist. Eingabe auf einem davon in der Basis startet die Ernte mit Ziel darauf.
- **Erreichen die Arbeiter es?** Zielt die Ernte auf ein Gebäude (Tab, oder Eingabe darauf in der Basis), verfolgt die Mod den Weg der Arbeiter so, wie das Spiel sie bewegt, und wählt einen Winkel, der es erreicht: „Arbeiter treffen es direkt“, „Arbeiter erreichen es über eine Wand“, „Arbeiter laufen darüber“ (Weizenfelder) oder „kein freier Weg, Wald steht im Weg“. Ein Gebäude ohne freien Weg wird nur zufällig erreicht: versetze es oder das Gebäude im Weg. Links / Rechts sagen, was die Linie trifft.
- **Nach einer Ernte** sagt die Mod, was jedes Gebäude im Bau bekommen hat („Geisterhaus: 6 Treffer, von 40 auf 55 Prozent“). Die Zusammenfassung des Spiels zählt nur Rohstoffe: Sie sagt „Vergebene Mühe“, sobald nichts geerntet wurde, auch wenn die Arbeiter gebaut haben.
- **Erweitern**: Tab wählt ein Grundstück, Eingabe kauft es.
- **Neu anordnen**: Tab wählt ein Gebäude, Eingabe hebt es auf, die Pfeiltasten bewegen es, Eingabe setzt es
  ab.
- **Erkunden** (K): Die Pfeiltasten gehen ein Feld weiter und sagen, was dort ist: frei (und wie viele freie Felder nach rechts und nach unten folgen), ein Gebäude mit seiner Größe oder noch nicht gekauftes Land. Tab / Umschalt+Tab springen zum nächsten / vorigen freien Feld. K, Rücktaste oder Escape beenden. Spalten und Reihen sind dieselben wie beim Platzieren eines Gebäudes.
- Das Baumenü sagt, wie viel Platz jedes Gebäude braucht („3 breit, 2 hoch“), und beim Platzieren werden zuerst Name und Größe gesagt.

## Kampf {#battle}

### Die Tasten des Spiels

| Taste | Aktion |
|---|---|
| W A S D | Bewegen |
| Leertaste | Schießen (halten) |
| F | Autoschuss an / aus |
| E | Einen Stufenaufstieg öffnen, wenn einer bereit ist |
| = / - | Schnellere / langsamere Spielgeschwindigkeit (fehlen auf manchen Tastaturlayouts: nimm Num + / Num - des Mods, unten) |
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
| U | Statuseffekte mit den verbleibenden Sekunden und ob du dich bewegen kannst, und wenn nicht, warum (eingefroren, von einem Treffer zurückgestoßen, Gegner rücken vor). Endet ein Einfrieren, sagt die Mod „Du kannst dich wieder bewegen“ |
| J | Gegner und Boss |
| K | Deine Position |
| G | Spielgeschwindigkeit |
| L | Stufe, Zeit, Welle, gewonnene Zahnräder |
| B | Deine Bälle und passiven Fähigkeiten |
| P | Gegenstände auf dem Feld |
| N | Gefahren (Schüsse, Laser, Bomben, Boden) |
| Q | Was war das für ein Geräusch? Die Geräusche, die gerade laufen, und die letzten, jeweils mit Bedeutung und Seite |
| Num + / Num - | Schnellere / langsamere Spielgeschwindigkeit, mit jedem Tastaturlayout |

Jede Mod-Taste lässt sich ändern: siehe [Die Tasten ändern](#changing-keys).

Im Kampf wird angesagt: das Level zu Beginn, Bosse und ihr Leben bei 75, 50 und 25 Prozent, Schaden und
Leben, Statuseffekte, besondere Gegenstände, wenn sie fallen, „Stufenaufstiege bereit, drücke E“, das Ende
des Durchlaufs.

**Bosse**: J nennt das echte Leben des Bosses, dasselbe wie seine Leiste im Spiel. Manche Bosse nehmen nur an einem Schwachpunkt Schaden: Der Körper des Skelettkönigs kann nicht verletzt werden, nur die Krone auf seinem Kopf, und sein Körper schützt sie von vorn, also erreichen Kugeln sie erst nach Abprallern an den Wänden. J sagt, wo der Schwachpunkt ist, und die Zielhilfe nimmt ihn ins Visier.

## Kampfgeräusche {#battle-sounds}

Drei Regeln gelten für jedes Geräusch, immer von deiner Figur aus gesehen:

- **Seite**: Ein Geräusch kommt von dort, wo das Ding ist, um das es geht: links von dir von links,
  rechts von dir von rechts, aus der Mitte, wenn es geradeaus vor dir ist.
- **Tonhöhe**: Höher heißt näher.
- **Gedämpft** heißt, es ist hinter dir.

Dinge, die sich ständig bewegen (dein Ball, gegnerische Schüsse), sind **durchgehende Geräusche**, die ihnen
folgen; Dinge, die sich schrittweise ändern (vorrückende Gegner, am Boden liegende Gegenstände), sind kurze
Geräusche.

| Geräusch | Bedeutung |
|---|---|
| Handtrommel | Ein Gegner. Höher und schneller = näher. Wenn sich die Gegner bewegen, spielen die nächsten abwechselnd; solange sich nichts ändert, nur der nächste. |
| Große, tiefe Trommel | Ein Boss. Trommeln, Gong und Horn, wenn einer erscheint. |
| Knurren | Ein Gegner erreicht dich gleich. |
| Ansteigendes Jaulen | Ein Gegner lädt einen Angriff auf, auf seiner Seite. |
| **Rollendes Summen** (durchgehend) | Der Ball, den du fangen kannst. Es klingt von dort, **wo der Ball gerade ist**, und **steigt, je näher der Ball dir kommt**, so hörst du, wohin er fliegt; es pulsiert schneller, je näher der Ball deiner Linie kommt, und ist leiser, solange der Ball noch von einem Gegner abprallen kann. |
| Klick | Du stehst dort, wo der Ball landen wird: Bleib dort. |
| Handschuh-Plumps | Ball gefangen. |
| Dumpfer, absinkender Plumps | Ein Ball ist an dir vorbei. |
| Gewehrständer | Deine Bälle sind zurück: Du kannst schießen (stumm mit Autoschuss). |
| Glasklingen | Das EP-Juwel, Gold oder der Rohstoff, zu dem du gehen sollst: das nächste oder ein Haufen etwas weiter weg. Sie sammeln sich nicht von selbst ein. |
| Kalimba-Funkeln | Ein besonderer Gegenstand: Fusionsreaktor, Bauplan, Ei, Zahnrad, Leben, Magnet, Bombe, Juwelenbeutel. |
| **Luftiges Pfeifen** (durchgehend) | Ein gegnerischer Schuss, der nah an dir vorbeifliegt, höher, je näher er kommt. Lauter, wenn er dich treffen wird. |
| Rauschen und Knall | Ein Schuss wird dich treffen; es klingt vom Schuss her: Geh ihm aus dem Weg. |
| Elektrisches Brummen / zweimal Knistern | Ein Laser zielt / du bist in seiner Linie; beides klingt vom Laser her: Geh aus der Linie. |
| Fallendes Pfeifen / Zündschnurzischen | Eine Bombe fällt in der Nähe / sie wird auf dir landen; beides klingt von der Stelle, an der sie landet: Geh von dort weg. |
| Quatschen | Du bist auf eine Bodengefahr getreten (ihr Name wird angesagt). |
| Schlag / Schlucken | Schaden / Heilung. |
| Herzschlag | Wenig Leben; schneller, wenn sehr wenig. |
| Taumelndes Wabern | Ein Statuseffekt hat dich erwischt. |
| Zwei Glockentöne aufwärts | Ein Stufenaufstieg ist bereit (drücke E). |
| Holzklopfen | Seitlicher Rand der Arena (auch das Ende einer Menüliste). |
| Metallklopfen | Vordere oder hintere Grenze deines Laufbereichs. |
| Schalterklick | Die Zielhilfe hat zu einem anderen Ziel gewechselt, auf der Seite des neuen Ziels. Mit Dauerfeuer nicht nach einem Abschuss. |

Jedes dieser Geräusche kannst du unter **F4 → Klangführer** hören, mit Pfeil nach links / rechts auf jeder
Seite.
Im Kampf sagt **Q**, welche Geräusche du gerade gehört hast.

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
  Gegner, den Gegner vor dir, einen festen Winkel, oder ist aus (Maus). Die Pfeiltasten justieren sie. Sie zielt auf einen Teil des Gegners, den kein anderer verdeckt, überspringt Gegner in einem Winkel, in den dein Held nicht schießen kann, und nimmt im Standardmodus zuerst den Boss, außer ein anderer Gegner erreicht dich gleich.
- **Fanghilfe** (Umschalt halten): Deine Figur läuft unter den nächsten herunterkommenden Ball; wenn kein
  Ball kommt, läuft sie zum nächsten Juwel oder Gegenstand; wenn dich gleich ein Schuss trifft, weicht sie
  zuerst aus. W / S bewegen dich weiterhin nach vorn und hinten, A / D übernehmen immer. In F4 kann sie aus,
  „Umschalt halten“ (Standard) oder automatisch sein. Bist du verletzt, holt sie zuerst einen Heilgegenstand.

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
- Controller
- Tasten (Eingabe öffnet): siehe [Die Tasten ändern](#changing-keys)
- Klangführer (Eingabe öffnet)
- Debug-Protokoll
- Kämpfe aufzeichnen (standardmäßig aus): jeder Kampf wird in `UserData\BallPitAccess\telemetry.tsv` geschrieben, zum Abstimmen der Klänge

## Die Tasten ändern {#changing-keys}

**F4 → Tasten** listet jede Mod-Taste mit der Taste, auf der sie gerade liegt. Pfeil nach oben / unten
wählen, **Eingabe** und dann die neue Taste drücken (Esc bricht ab), **Entf** stellt die Standardtaste
wieder her, und die letzte Zeile setzt alle zurück. Gehört die neue Taste schon zu einer anderen
Mod-Aktion, tauschen die beiden, und der Mod sagt, wohin die andere gewandert ist. Tasten, die die Menüs
oder das Spiel brauchen (Pfeiltasten, Eingabe, Tab, Esc, Rücktaste, Leertaste, W A S D F E R X), können
nicht belegt werden. Die F1-Hilfe endet mit den Tasten, wie sie gerade belegt sind.

## Controller {#controller}

- Xbox-Tasten; ein PlayStation- oder Switch-Controller funktioniert, wenn Steam Input für das Spiel eingeschaltet ist
- Außerhalb eines Kampfes bewegen Steuerkreuz oder linker Stick wie die Pfeiltasten, A ist Eingabe und B geht zurück
- X und Y springen zum ersten und letzten Eintrag; beim Platzieren eines Gebäudes sucht X eine Stelle, an der es passt, und Y dreht es
- RB und LB sind Tab und Umschalt Tab, RT und LT wechseln die Reiter des Bildschirms
- Start ist die Pause des Spiels
- Im Kampf hat das Spiel die Sticks, die Trigger und die Tasten
- Steuerkreuz oben sagt die Gegner, unten deine Gesundheit, links deinen Status, rechts deinen Fortschritt
- Halte den linken Stick gedrückt, um zu fangen, wie Umschalt
- Lässt du den rechten Stick los, zielt die Zielhilfe für dich; bewege ihn, um selbst zu zielen
- Halte View für mehr: A Hilfe, B Mod-Einstellungen, X liest den Bildschirm, Y wiederholt, Start wechselt den Zielmodus
- View mit LB und RB liest ältere und neuere Zeilen der Rückschau, View mit einem Stick-Klick wechselt zwischen Details und Ereignissen
- Im Kampf View mit dem Steuerkreuz: oben Gefahren, unten Ausrüstung, links Gegenstände, rechts der letzte Ton; View mit LT und RT macht das Spiel langsamer und schneller
- In der Basis View mit dem Steuerkreuz: oben Ernte, unten Ressourcen, links Erkundungsmodus
- In Menüs ist View mit LT und RT Bild auf und Bild ab

## Tipps für den ersten Durchlauf {#tips-for-a-first-run}

- **Fang deine Bälle.** Ein gefangener Ball kann sofort wieder abgeschossen werden; ein verpasster braucht 2
  bis 3 Sekunden, bis er zurückkommt. Folge dem rollenden Summen, bis du den Klick hörst, und bleib dann
  stehen.
- **Sammle deine EP-Juwelen ein.** Sie sammeln sich nicht von selbst ein. Folge dem Glasklingen; gedämpft
  heißt, es ist hinter dir.
- **Erst überleben.** Am Anfang eines Durchlaufs ist am Leben bleiben wichtiger als Schaden.
- **Fusioniere und entwickle** deine Bälle, wenn der Stufenaufstieg es anbietet: Die Auswahl sagt „Fusion“
  oder „Evolution“.
- **Werde bei Bossen langsamer**, mit Num - (oder der Minustaste des Spiels). Der Mod erinnert dich daran,
  wenn ein Boss bei hoher Geschwindigkeit erscheint.
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
- `UserData\BallPitAccess\telemetry.tsv`, wenn du F4 → Kämpfe aufzeichnen eingeschaltet hast: jeder Klang,
  Gegner, Ball und Schuss deiner Kämpfe. Die beste Hilfe, wenn die Klänge schwer zu verfolgen sind.

Sag, was du getan hast, was du gehört hast und was du erwartet hast.

## Mitwirkende {#credits}

- BALL x PIT von Kenny Sun, veröffentlicht von Devolver Digital. Dieser Mod ist inoffiziell.
- MelonLoader (LavaGang) lädt den Mod. Prism schickt die Sprachausgabe an deinen Screenreader.
