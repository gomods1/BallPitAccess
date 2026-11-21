# BALL x PIT Access

Una mod che rende **BALL x PIT** (Kenny Sun / Devolver Digital) giocabile senza vista. Legge ogni menu
tramite il tuo screen reader, ti permette di usare tutto il gioco con la tastiera e trasforma il campo di
battaglia in suono: i nemici, le tue palle, i colpi nemici, i cristalli di PX e gli oggetti speciali hanno
ciascuno il proprio suono, che arriva dal loro lato rispetto a te.

Versione 0.6.4, per la versione 1.308 del gioco, incluso il DLC Pallilonia Risorta (Risen Ballbylon). La
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
- [Cambiare i tasti](#changing-keys)
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
| K | Esplora la base casella per casella |

- **Costruzione**: le frecce spostano l'edificio di una casella e dicono se ci sta, Invio costruisce,
  R ruota, Home salta a un posto libero, Backspace annulla.
- **Raccolta**: parte mirata su un edificio. Invio lancia i lavoratori, Tab mira a un altro edificio,
  freccia sinistra / destra ruotano la mira, tieni premuta la Barra spaziatrice per accelerare, Esc esce.
- **Costruzione**: gli edifici in costruzione o in potenziamento sono anche nella lista Tab della raccolta. Ogni lavoratore che ci rimbalza sopra li costruisce; la mod avvisa quando sono completati. Invio su uno di essi nella base avvia la raccolta mirata su di esso.
- **I lavoratori ci arriveranno?** Quando la raccolta mira a un edificio (Tab, o Invio su di esso nella base), la mod segue il percorso dei lavoratori come li muove il gioco e sceglie un angolo che ci arriva: "i lavoratori lo colpiscono direttamente", "i lavoratori lo raggiungono rimbalzando su una parete", "i lavoratori ci passano sopra" (campi di grano), oppure "nessun percorso libero, Foresta è in mezzo". Un edificio senza percorso libero si raggiunge solo per caso: spostalo, o sposta quello che ostacola. Sinistra / Destra dicono cosa colpisce la linea.
- **Dopo una raccolta** la mod dice cosa ha ricevuto ogni edificio in costruzione ("Casa stregata: 6 colpi, dal 40 al 55 per cento"). Il riepilogo del gioco conta solo le risorse: dice "Un'impresa infruttuosa" ogni volta che non si è raccolto nulla, anche se i lavoratori hanno costruito.
- **Espansione**: Tab sceglie un lotto, Invio lo compra.
- **Riposizionamento**: Tab sceglie un edificio, Invio lo solleva, le frecce lo spostano, Invio lo posa.
- **Esplora** (K): le frecce avanzano di una casella e dicono cosa c'è: libera (e quante caselle libere seguono a destra e in basso), un edificio con la sua dimensione, o terreno non acquistato. Tab / Maiusc+Tab saltano alla casella libera successiva / precedente. K, Backspace o Esc escono. Colonne e righe sono le stesse usate per piazzare un edificio.
- Il menu di costruzione dice quanto spazio occupa ogni edificio ("3 di larghezza, 2 di altezza"), e il piazzamento inizia con nome e dimensione.

## Battaglia {#battle}

### I tasti del gioco

| Tasto | Azione |
|---|---|
| W A S D | Movimento |
| Barra spaziatrice | Spara (tieni premuta) |
| F | Fuoco automatico attivo / disattivo |
| E | Apre un nuovo livello quando è pronto |
| = / - | Velocità di gioco più alta / più bassa (mancano in alcuni layout di tastiera: usa Num + / Num - della mod, più sotto) |
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
| U | Stati con i secondi rimasti, e se puoi muoverti, e se no perché (congelato, respinto da un colpo, nemici che avanzano). Quando finisce un congelamento la mod dice "Puoi muoverti di nuovo" |
| J | Nemici e boss |
| K | La tua posizione |
| G | Velocità di gioco |
| L | Livello, tempo, ondata, ingranaggi ottenuti |
| B | Le tue palle e passive |
| P | Oggetti sul campo |
| N | Pericoli (colpi, laser, bombe, terreno) |
| Q | Cos'era quel suono? I suoni che si sentono ora e gli ultimi, con il significato di ciascuno e il suo lato |
| Num + / Num - | Velocità di gioco più alta / più bassa, con qualsiasi layout di tastiera |

Ogni tasto della mod si può cambiare: vedi [Cambiare i tasti](#changing-keys).

Durante la battaglia vengono annunciati: il livello all'inizio, i boss e la loro salute al 75, 50 e 25 per
cento, i danni e la salute, gli effetti di stato, gli oggetti speciali quando cadono, "nuovi livelli
pronti, premi E", la fine della spedizione.

**Boss**: J dà la salute reale del boss, la stessa della sua barra nel gioco. Alcuni boss subiscono danni solo su un punto debole: il corpo del Re Scheletro non può essere ferito, solo la corona sulla sua testa, e il corpo la protegge davanti, quindi le palle la raggiungono dopo essere rimbalzate sulle pareti. J dice dov'è il punto debole, e l'assistenza alla mira lo prende di mira.

## Suoni di battaglia {#battle-sounds}

Tre regole valgono per ogni suono, sempre rispetto al tuo eroe:

- **Lato**: un suono arriva da dove si trova la cosa a cui si riferisce: da sinistra se è alla tua sinistra,
  da destra se è alla tua destra, dal centro se è dritto davanti.
- **Altezza**: più acuto vuol dire più vicino.
- **Ovattato** vuol dire che è dietro di te.

Le cose che si muovono di continuo (la tua palla, i colpi nemici) sono **suoni continui** che le seguono;
le cose che cambiano a scatti (i nemici che avanzano, gli oggetti a terra) sono suoni brevi.

| Suono | Significato |
|---|---|
| Tamburo a mano | Un nemico. Più acuto e veloce = più vicino. Quando i nemici si muovono suonano a turno i più vicini; finché nulla cambia, solo il più vicino. |
| Grande tamburo profondo | Un boss. Tamburi, gong e corno quando ne arriva uno. |
| Ringhio | Un nemico sta per raggiungerti. |
| Lamento crescente | Un nemico carica un attacco, dal suo lato. |
| **Ronzio rotolante** (continuo) | La palla che puoi prendere. Suona dal **punto in cui si trova la palla** e **sale man mano che la palla si avvicina a te**, così senti dove sta andando; pulsa più veloce quando la palla si avvicina alla tua linea ed è più tenue finché la palla può ancora rimbalzare su un nemico. |
| Clic | Sei fermo dove atterrerà la palla: resta lì. |
| Tonfo di guantone | Palla presa. |
| Tonfo sordo che sprofonda | Una palla ti è sfuggita. |
| Arma che si carica | Le tue palle sono tornate: puoi sparare (silenzioso con il fuoco automatico). |
| Tintinnio di vetro | Il cristallo di PX, l'oro o la risorsa verso cui andare: il più vicino, o un mucchio poco più lontano. Non si raccolgono da soli. |
| Scintillio di kalimba | Un oggetto speciale: reattore a fusione, progetto, uovo, ingranaggio, salute, calamita, bomba, sacchetto di gemme. |
| **Fischio d'aria** (continuo) | Un colpo nemico che vola vicino a te, più acuto man mano che arriva. Più forte quando ti colpirà. |
| Sibilo e schiocco | Un colpo ti colpirà; suona dal colpo: togliti dalla sua traiettoria. |
| Ronzio elettrico / due scariche | Un laser sta prendendo la mira / sei sulla sua linea; entrambi suonano dal laser: esci dalla sua linea. |
| Fischio in caduta / sfrigolio di miccia | Una bomba sta cadendo qui vicino / ti cadrà addosso; entrambi suonano dal punto in cui atterra: allontanati da lì. |
| Risucchio viscido | Sei finito su un pericolo del terreno (ne viene detto il nome). |
| Pugno / sorso | Danno / cura. |
| Battito cardiaco | Salute bassa; più veloce quando è molto bassa. |
| Oscillazione stordita | Un effetto di stato ti ha colpito. |
| Due note di campana in salita | Un nuovo livello è pronto (premi E). |
| Colpo di legno | Bordo laterale dell'arena (anche la fine di una lista di un menu). |
| Colpo metallico | Limite anteriore o posteriore della tua zona di movimento. |
| Clic di interruttore | L'assistenza alla mira è passata a un altro bersaglio, dal lato del nuovo. Con il fuoco automatico, non dopo un'uccisione. |

Tutti questi suoni si possono ascoltare in **F4 → Guida ai suoni**, con freccia sinistra / destra per
sentirli da ciascun lato.
In battaglia, **Q** dice quali suoni hai appena sentito.

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
  regolano con precisione. Mira a una parte del nemico che nessun altro copre, salta i nemici a un angolo in cui il tuo eroe non può tirare e, nella modalità predefinita, punta prima al boss, a meno che un altro nemico stia per raggiungerti.
- **Assistenza alla presa** (tieni premuto Maiusc): l'eroe va sotto la prossima palla che scende; se non
  arriva nessuna palla va al cristallo o all'oggetto più vicino; se un colpo sta per colpirti, prima si
  sposta dalla sua traiettoria. W / S ti muovono comunque avanti e indietro, A / D prendono sempre il
  controllo. In F4 può essere disattivata, "tieni premuto Maiusc" (predefinito) o automatica. Se sei ferito, va prima a prendere un oggetto di salute.

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
- Tasti (Invio li apre): vedi [Cambiare i tasti](#changing-keys)
- Guida ai suoni (Invio la apre)
- Registro di debug
- Registra le battaglie (disattivato di default): ogni battaglia viene scritta in `UserData\BallPitAccess\telemetry.tsv`, per regolare i suoni

## Cambiare i tasti {#changing-keys}

**F4 → Tasti** elenca ogni tasto della mod con il tasto che usa ora. Freccia su / giù per scegliere,
**Invio** e poi premi il nuovo tasto (Esc annulla), **Canc** rimette il tasto predefinito, e l'ultima riga
li ripristina tutti. Se il nuovo tasto appartiene già a un'altra azione della mod, i due si scambiano e la
mod dice dove è finita l'altra. I tasti che servono ai menu o al gioco (frecce, Invio, Tab, Esc,
Backspace, Barra spaziatrice, W A S D F E R X) non si possono prendere. L'aiuto di F1 finisce con i tasti
come sono impostati ora.

## Consigli per la prima spedizione {#tips-for-a-first-run}

- **Prendi al volo le tue palle.** Una palla presa si può rilanciare subito; una mancata impiega da 2 a 3
  secondi per tornare. Segui il ronzio rotolante finché non senti il clic, poi resta fermo.
- **Raccogli i tuoi cristalli di PX.** Non si raccolgono da soli. Segui il tintinnio di vetro; ovattato
  vuol dire che è dietro di te.
- **Prima di tutto sopravvivi.** All'inizio di una spedizione restare vivo conta più del danno.
- **Fondi ed evolvi** le tue palle quando il nuovo livello lo offre: le scelte dicono "fusione" o
  "evoluzione".
- **Rallenta per i boss** con Num - (o il tasto meno del gioco). La mod te lo ricorda quando un boss
  arriva a velocità alta.
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
- `UserData\BallPitAccess\telemetry.tsv`, se hai attivato F4 → Registra le battaglie: ogni suono, nemico,
  palla e colpo delle tue battaglie. L'aiuto migliore quando i suoni sono difficili da seguire.

Di' cosa hai fatto, cosa hai sentito e cosa ti aspettavi.

## Riconoscimenti {#credits}

- BALL x PIT di Kenny Sun, pubblicato da Devolver Digital. Questa mod non è ufficiale.
- MelonLoader (LavaGang) carica la mod. Prism invia la voce al tuo screen reader.
