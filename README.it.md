# BALL x PIT Access

Una mod che rende **BALL x PIT** (Kenny Sun / Devolver Digital) giocabile senza vista. Legge ogni menu
tramite il tuo screen reader, ti permette di usare tutto il gioco con la tastiera e trasforma il campo di
battaglia in suono: i nemici, le tue palle, i colpi nemici, i cristalli di PX e gli oggetti speciali hanno
ciascuno il proprio suono, che arriva dal loro lato rispetto a te.

Versione 0.5.0, per la versione 1.308 del gioco, incluso il DLC Pallilonia Risorta (Risen Ballbylon). La
mod parla **tutte le lingue offerte dal gioco** e segue l'impostazione della lingua del gioco.

<!-- LANGS -->
[English](README.html) · [Español](README.es.html) · [Français](README.fr.html) · [Deutsch](README.de.html) · **Italiano** · [Português (Brasil)](README.pt-BR.html) · [Polski](README.pl.html) · [Türkçe](README.tr.html) · [Русский](README.ru.html) · [Українська](README.uk.html) · [日本語](README.ja.html) · [한국어](README.ko.html) · [简体中文](README.zh-CN.html) · [繁體中文](README.zh-TW.html) · [ไทย](README.th.html)
<!-- /LANGS -->

> **Stato: prima versione di prova.** I menu, la base e le prime spedizioni sono stati giocati con la
> mod. I suoni di battaglia e le parti più recenti (suono continuo della palla, assistenza alla presa,
> filtri dei suoni) non sono ancora stati regolati insieme ai giocatori. Segnala cosa funziona e cosa no
> (vedi [Segnalare un problema](#reporting-a-problem)).

## Contenuto {#contents}

- [Cosa ti serve](#what-you-need)
- [Installazione](#installing)
- [Rimuovere la mod](#removing-the-mod)
- [Primi passi](#first-steps)
- [Tasti validi ovunque](#keys-everywhere)
- [Menu](#menus)
- [La base (Nuova Pallilonia)](#the-base-new-ballbylon)
- [Battaglia](#battle)
- [Suoni di battaglia](#battle-sounds)
- [Filtri dei suoni (tasti numerici)](#sound-filters-number-keys)
- [Assistenze](#assists)
- [Impostazioni della mod (F4)](#mod-settings-f4)
- [Consigli per la prima spedizione](#tips-for-a-first-run)
- [Limiti noti](#known-limits)
- [Segnalare un problema](#reporting-a-problem)
- [Riconoscimenti](#credits)

## Cosa ti serve {#what-you-need}

- BALL x PIT su Windows (Steam), versione 1.308.
- Uno screen reader: quello provato è NVDA. Anche gli altri supportati dalla libreria vocale Prism (JAWS,
  Assistente vocale, voci SAPI) dovrebbero funzionare.
- **MelonLoader 0.7.3** (x64), il caricatore di mod. Gratuito, dalla sua pagina ufficiale delle release
  su GitHub.
- Cuffie o altoparlanti stereo: in battaglia sinistra e destra contano molto.

## Installazione {#installing}

**Con l'installer (il modo più semplice):** scarica `BallPitAccessibilityInstaller.exe` dall'
[ultima versione](https://github.com/gomods1/BallPitAccess/releases/latest) e avvialo. Trova il gioco tramite Steam, installa MelonLoader 0.7.3, la
libreria vocale Prism e la mod, e in seguito li aggiorna o li rimuove. Poi avvia il gioco: il primo avvio
richiede qualche minuto ed è silenzioso mentre MelonLoader prepara i file del gioco; dopo, il tuo screen
reader inizia a parlare.

**A mano:** scarica `BallPitAccess-vX.Y.Z.zip` dalla stessa pagina ed estrai tutto nella cartella del
gioco (di solito `C:\Program Files (x86)\Steam\steamapps\common\BALLxPIT`, la cartella che contiene `Balls.exe`), in modo che `version.dll` finisca accanto
a `Balls.exe`. Lo zip contiene già MelonLoader: non serve installarlo a parte. Avvia il gioco; il primo
avvio è lento e silenzioso, come sopra.

## Rimuovere la mod {#removing-the-mod}

Il pulsante Disinstalla dell'installer rimuove la mod e MelonLoader e conserva le tue impostazioni in
`UserData`. A mano:

Elimina `Mods\BallPitAccess.dll` e `prism.dll`. Per rimuovere anche MelonLoader, elimina `version.dll` e
le cartelle `MelonLoader`, `Mods`, `Plugins` e `UserData` dalla cartella del gioco. La mod non modifica
nessun file del gioco.

## Primi passi {#first-steps}

- Premi **F1** in qualsiasi momento per l'aiuto: tutte le sue parti, a partire da quella del punto in
  cui ti trovi. Freccia su e freccia giù lo leggono riga per riga, Tab salta alla parte successiva, Esc
  chiude.
- Premi **F4** per le impostazioni della mod e, al loro interno, apri la **guida ai suoni**: riproduce
  ogni suono di battaglia e dice cosa significa. Ascoltarla una volta prima della prima spedizione aiuta
  molto.
- Con la tastiera il gioco in sé si usa con il mouse; con la mod il mouse non serve mai, tranne che per il
  lanciatore automatico della base (vedi [Limiti noti](#known-limits)).

## Tasti validi ovunque {#keys-everywhere}

| Tasto | Azione |
|---|---|
| F1 | Aiuto (tutte le parti; Tab / Maiusc+Tab tra le parti) |
| F2 | Legge tutta la schermata |
| F3 | Ripete l'ultima cosa detta |
| F4 | Impostazioni della mod e guida ai suoni |
| F5 | Cambia la modalità dell'assistenza alla mira |
| F6 / F7 | Ruota la mira di 10 gradi a sinistra / a destra |
| F8 | Dice dove stai mirando |
| Ctrl+freccia su | Legge i dettagli dell'elemento con il focus (Ctrl+freccia giù torna indietro) |
| Ctrl+freccia sinistra / Ctrl+freccia destra | Passa tra i dettagli e gli eventi recenti |

Mentre è aperta una lista della mod (aiuto, impostazioni, guida ai suoni), il gioco non riceve i tuoi
tasti.

## Menu {#menus}

Ogni schermata è **un'unica lista in un ordine logico**: prima le sue scelte, poi i suoi pulsanti, infine
Chiudi. L'ordine non dipende mai da dove le cose sono disegnate sullo schermo.

| Tasto | Azione |
|---|---|
| Freccia su / freccia giù | Elemento precedente / successivo |
| Invio | Sceglie |
| Tab / Maiusc+Tab | Gruppo successivo / precedente della schermata |
| Home / Fine | Primo / ultimo elemento del gruppo |
| Freccia sinistra / freccia destra | Cambia un'impostazione a lista o a cursore; su un livello, ne cambia la difficoltà |
| Ctrl+Tab / Ctrl+Maiusc+Tab | Scheda successiva / precedente nelle schermate con schede |
| Backspace o Esc | Indietro |
| Pagina su / Pagina giù | Serie nell'enciclopedia; grado di Nuova partita+ nella lista dei livelli |
| Maiusc+Invio | Su un livello: apre la sua classifica |

- Invio su un personaggio ti porta sul suo pulsante Seleziona.
- Quando il tutorial guidato indica qualcosa, il focus salta lì: premi Invio.
- Le scelte di potenziamento vengono lette con i loro numeri ("Infligge da 10 a 15 danni").

## La base (Nuova Pallilonia) {#the-base-new-ballbylon}

| Tasto | Azione |
|---|---|
| Freccia su / freccia giù, Invio | Pulsanti della barra laterale: Gioca, Raccolta, Costruisci, Espandi, Riposiziona, Opzioni |
| Tab / Maiusc+Tab | Scorre il montacarichi e i tuoi edifici |
| Invio su un edificio | Avvia una raccolta mirata su di esso se è pronto; altrimenti apre il suo potenziamento |
| Maiusc+Invio | Apre il potenziamento di un edificio |
| Invio sul montacarichi | Apre il potenziamento del montacarichi (giù nel baratro, o su verso Pallilonia Risorta) |
| Canc | Smantella un edificio |
| H | Risorse e giorno |
| J | Raccolta e lavoratori |

- **Costruzione**: le frecce spostano l'edificio di una casella e dicono se ci sta, Invio costruisce,
  R ruota, Home salta a un posto libero, Backspace annulla.
- **Raccolta**: parte mirata su un edificio. Invio lancia i lavoratori, Tab mira a un altro edificio,
  freccia sinistra / destra ruotano la mira, tieni premuta la Barra spaziatrice per accelerare, Esc esce.
- **Espansione**: Tab sceglie un lotto, Invio lo compra.
- **Riposizionamento**: Tab sceglie un edificio, Invio lo solleva, le frecce lo spostano, Invio lo posa.

## Battaglia {#battle}

### I tasti del gioco

| Tasto | Azione |
|---|---|
| W A S D | Movimento |
| Barra spaziatrice | Spara (tieni premuta) |
| F | Fuoco automatico attivo / disattivo |
| E | Apre un nuovo livello quando è pronto |
| = / - | Velocità di gioco più alta / più bassa |
| Esc | Pausa |

### Tasti della mod

| Tasto | Azione |
|---|---|
| Freccia sinistra / freccia destra | Ruota la mira di 5 gradi a mano |
| Freccia su | Restituisce la mira all'assistenza |
| Freccia giù | Dice dove stai mirando |
| Maiusc (tieni premuto) | Assistenza alla presa (vedi [Assistenze](#assists)) |
| Da 1 a 7, 0 | Filtri dei suoni (vedi [Filtri dei suoni](#sound-filters-number-keys)) |
| H | Salute ed effetti di stato |
| J | Nemici e boss |
| K | La tua posizione |
| G | Velocità di gioco |
| L | Livello, tempo, ondata, ingranaggi ottenuti |
| B | Le tue palle e passive |
| P | Oggetti sul campo |
| N | Pericoli (colpi, laser, bombe, terreno) |

Durante la battaglia vengono annunciati: il livello all'inizio, i boss e la loro salute al 75, 50 e 25 per
cento, i danni e la salute, gli effetti di stato, gli oggetti speciali quando cadono, "nuovi livelli
pronti, premi E", la fine della spedizione.

## Suoni di battaglia {#battle-sounds}

Quattro regole valgono per ogni suono:

- **Lato**: un suono arriva dal lato della cosa a cui si riferisce.
- **Altezza**: più acuto vuol dire più vicino.
- **Ovattato** vuol dire che è dietro di te.
- Un suono che ti chiede di **muoverti** arriva dal lato verso cui andare.

Le cose che si muovono di continuo (la tua palla, i colpi nemici) sono **suoni continui** che le seguono;
le cose che cambiano a scatti (i nemici che avanzano, gli oggetti a terra) sono suoni brevi.

| Suono | Significato |
|---|---|
| Tamburo a mano | Un nemico. Più acuto e veloce = più vicino. I tre più vicini suonano a turno. |
| Grande tamburo profondo | Un boss. Tamburi, gong e corno quando ne arriva uno. |
| Ringhio | Un nemico sta per raggiungerti. |
| Lamento crescente | Un nemico carica un attacco, dal suo lato. |
| **Ronzio rotolante** (continuo) | La palla che puoi prendere. Cammina verso il suo lato; **sale man mano che ti avvicini al punto in cui atterrerà la palla**, pulsa più veloce quando la palla si avvicina a te ed è più tenue finché la palla può ancora rimbalzare su un nemico. |
| Clic mentre il ronzio sale di un gradino | Sei sotto la palla: resta lì. |
| Tonfo di guantone | Palla presa. |
| Tonfo sordo che sprofonda | Una palla ti è sfuggita. |
| Arma che si carica | Le tue palle sono tornate: puoi sparare (silenzioso con il fuoco automatico). |
| Tintinnio di vetro | Il cristallo di PX, l'oro o la risorsa più vicina che puoi raggiungere a piedi. Non si raccolgono da soli. |
| Scintillio di kalimba | Un oggetto speciale: reattore a fusione, progetto, uovo, ingranaggio, salute, calamita, bomba, sacchetto di gemme. |
| **Fischio d'aria** (continuo) | Un colpo nemico che vola vicino a te, più acuto man mano che arriva. Più forte quando ti colpirà. |
| Sibilo e schiocco | Un colpo ti colpirà: spostati verso il lato da cui suona. |
| Ronzio elettrico / due scariche | Un laser sta prendendo la mira / sei sulla sua linea: spostati verso il lato da cui suona. |
| Fischio in caduta / sfrigolio di miccia | Una bomba sta cadendo qui vicino / ti cadrà addosso: spostati verso il lato da cui suona. |
| Risucchio viscido | Sei finito su un pericolo del terreno (ne viene detto il nome). |
| Pugno / sorso | Danno / cura. |
| Battito cardiaco | Salute bassa; più veloce quando è molto bassa. |
| Oscillazione stordita | Un effetto di stato ti ha colpito. |
| Due note di campana in salita | Un nuovo livello è pronto (premi E). |
| Colpo di legno | Bordo laterale dell'arena (anche la fine di una lista di un menu). |
| Colpo metallico | Limite anteriore o posteriore della tua zona di movimento. |
| Clic di interruttore | L'assistenza alla mira ha cambiato bersaglio, dal lato del nuovo. |

Tutti questi suoni si possono ascoltare in **F4 → Guida ai suoni**, con freccia sinistra / destra per
sentirli da ciascun lato.

## Filtri dei suoni (tasti numerici) {#sound-filters-number-keys}

In battaglia, i tasti numerici silenziano o riattivano un gruppo di suoni per aiutarti a concentrarti. Il
gioco non usa nessuno di questi tasti.

| Tasto | Gruppo |
|---|---|
| 1 | Nemici: tamburi, ringhio, carica dell'attacco, arrivo del boss |
| 2 | Palle: ronzio rotolante, clic, presa, palla mancata, arma che si carica |
| 3 | Ricompense: cristalli, oro, risorse, oggetti speciali |
| 4 | Colpi e pericoli |
| 5 | Bordi dell'arena |
| 6 | La tua salute: danno, cura, battito, stati |
| 7 | Clic dell'assistenza alla mira |
| 0 | Riattiva tutto |

- Ogni numero attiva o disattiva il proprio gruppo, indipendentemente dagli altri.
- **Ctrl+numero** lascia attivo solo quel gruppo. Esempio: solo nemici e ricompense = Ctrl+1, poi 3.
- La tua scelta viene conservata per la sessione successiva. Quando una battaglia inizia con qualcosa di
  silenziato, la mod dice cosa è silenziato.
- Silenziare cambia solo i suoni: i tasti H, J, P, N rispondono comunque e l'assistenza alla presa
  continua a schivare.

## Assistenze {#assists}

Tutto ciò che fa un'assistenza si può fare anche a orecchio; le assistenze ci sono quando le vuoi.

- **Assistenza alla mira** (F5 cambia la modalità): mira per te al nemico più vicino (predefinito), al
  nemico più forte, al nemico davanti a te, a un angolo fisso, oppure è disattivata (mouse). Le frecce la
  regolano con precisione.
- **Assistenza alla presa** (tieni premuto Maiusc): l'eroe va sotto la prossima palla che scende; se non
  arriva nessuna palla va al cristallo o all'oggetto più vicino; se un colpo sta per colpirti, prima si
  sposta dalla sua traiettoria. W / S ti muovono comunque avanti e indietro, A / D prendono sempre il
  controllo. In F4 può essere disattivata, "tieni premuto Maiusc" (predefinito) o automatica.

## Impostazioni della mod (F4) {#mod-settings-f4}

Freccia su / giù si spostano, freccia sinistra / destra o Invio cambiano, Esc chiude. Le impostazioni
vengono salvate.

- Modalità dell'assistenza alla mira
- Assistenza alla presa: disattivata, tieni premuto Maiusc, automatica
- Segnali audio attivi / disattivi, volume dei segnali
- Radar dei nemici, suoni delle palle, bordi dell'arena, colpi e pericoli (attivo / disattivo)
- Suoni degli oggetti: disattivati, solo oggetti speciali, tutti
- Suoni della salute, clic dell'assistenza alla mira (attivo / disattivo)
- Lettura delle descrizioni dei potenziamenti al focus
- Guida ai suoni (Invio la apre)
- Registro di debug

## Consigli per la prima spedizione {#tips-for-a-first-run}

- **Prendi al volo le tue palle.** Una palla presa si può rilanciare subito; una mancata impiega da 2 a 3
  secondi per tornare. Segui il ronzio rotolante finché non senti il clic, poi resta fermo.
- **Raccogli i tuoi cristalli di PX.** Non si raccolgono da soli. Segui il tintinnio di vetro; ovattato
  vuol dire che è dietro di te.
- **Prima di tutto sopravvivi.** All'inizio di una spedizione restare vivo conta più del danno.
- **Fondi ed evolvi** le tue palle quando il nuovo livello lo offre: le scelte dicono "fusione" o
  "evoluzione".
- **Rallenta per i boss** con il tasto meno. La mod te lo ricorda quando un boss arriva a velocità alta.
- Morire è normale: l'oro che riporti costruisce Nuova Pallilonia, e gli edifici rendono più forte ogni
  spedizione successiva.

## Limiti noti {#known-limits}

- Il lanciatore automatico della base richiede ancora il mouse.
- Le classifiche e le votazioni di Twitch vengono lette solo in modo basilare. I riconoscimenti non si
  possono saltare.
- Le impostazioni della mod sono su F4, non nella schermata delle opzioni del gioco.
- I testi della mod in lingue diverse dall'inglese e dallo spagnolo, italiano compreso, sono stati
  tradotti senza ancora una revisione da parte di un madrelingua. Le correzioni sono benvenute. I nomi
  del gioco (edifici, palle, nemici, potenziamenti) arrivano sempre dalla traduzione ufficiale del gioco.
- I volumi dei suoni, le frequenze e l'ampiezza della presa non sono ancora stati regolati insieme ai
  giocatori.

## Segnalare un problema {#reporting-a-problem}

File utili, nella cartella del gioco:

- `UserData\BallPitAccess\speech.log`: tutto ciò che la mod ha detto.
- `MelonLoader\Latest.log`: il log della mod (errori, schermate, palle prese e mancate).
- In battaglia, **Ctrl+Maiusc+D** scrive `UserData\BallPitAccess\threats.txt` (nemici, palle, colpi,
  oggetti con le loro posizioni) e la lista della schermata aperta in `Latest.log`: utile quando un suono
  indica la direzione sbagliata o un menu viene letto nell'ordine sbagliato.

Di' cosa hai fatto, cosa hai sentito e cosa ti aspettavi.

## Riconoscimenti {#credits}

- BALL x PIT di Kenny Sun, pubblicato da Devolver Digital. Questa mod non è ufficiale.
- MelonLoader (LavaGang) carica la mod. Prism invia la voce al tuo screen reader.
