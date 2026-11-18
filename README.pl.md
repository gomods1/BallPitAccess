# BALL x PIT Access

Mod, który pozwala grać w **BALL x PIT** (Kenny Sun / Devolver Digital) bez wzroku. Odczytuje każde menu
przez twój czytnik ekranu, pozwala obsłużyć całą grę z klawiatury i zamienia pole bitwy w dźwięk: wrogowie,
twoje kule, wrogie pociski, kryształy PD i przedmioty specjalne mają własne dźwięki, dochodzące z tej strony,
po której się znajdują.

Wersja 0.6.2, dla wersji gry 1.308, razem z DLC Wyniesione Kulopolis (Risen Ballbylon). Mod mówi **w każdym
języku, który oferuje gra**, i stosuje się do ustawienia języka w grze.

<!-- LANGS -->
[English](README.html) · [Español](README.es.html) · [Français](README.fr.html) · [Deutsch](README.de.html) · [Italiano](README.it.html) · [Português (Brasil)](README.pt-BR.html) · **Polski** · [Türkçe](README.tr.html) · [Русский](README.ru.html) · [Українська](README.uk.html) · [日本語](README.ja.html) · [한국어](README.ko.html) · [简体中文](README.zh-CN.html) · [繁體中文](README.zh-TW.html) · [ไทย](README.th.html)
<!-- /LANGS -->

> **Stan: wczesna wersja testowa.** Menu, baza i pierwsze rozgrywki zostały przetestowane z modem. Dźwięki
> walki i najnowsze elementy (ciągły dźwięk kuli, pomoc w łapaniu, filtry dźwięków) nie zostały jeszcze
> dostrojone z udziałem graczy. Zgłaszaj, co działa, a co nie (zobacz [Zgłaszanie problemu](#reporting-a-problem)).

## Spis treści

- [Czego potrzebujesz](#what-you-need)
- [Instalacja](#installing)
- [Usuwanie moda](#removing-the-mod)
- [Pierwsze kroki](#first-steps)
- [Klawisze dostępne wszędzie](#keys-everywhere)
- [Menu](#menus)
- [Baza (Nowe Kulopolis)](#the-base-new-ballbylon)
- [Walka](#battle)
- [Dźwięki walki](#battle-sounds)
- [Filtry dźwięków (klawisze cyfr)](#sound-filters-number-keys)
- [Pomoce](#assists)
- [Ustawienia moda (F4)](#mod-settings-f4)
- [Zmiana klawiszy](#changing-keys)
- [Wskazówki na pierwszą rozgrywkę](#tips-for-a-first-run)
- [Znane ograniczenia](#known-limits)
- [Zgłaszanie problemu](#reporting-a-problem)
- [Autorzy](#credits)

## Czego potrzebujesz {#what-you-need}

- BALL x PIT na Windows (Steam), wersja 1.308.
- Czytnik ekranu: przetestowany jest NVDA. Inne obsługiwane przez bibliotekę mowy Prism (JAWS, Narrator,
  głosy SAPI) też powinny działać.
- **MelonLoader 0.7.3** (x64), program ładujący mody. Darmowy, do pobrania z oficjalnej strony wydań na GitHubie.
- Słuchawki lub głośniki stereo: lewa i prawa strona mają w walce ogromne znaczenie.

## Instalacja {#installing}

**Instalatorem (najprościej):** pobierz `BallPitAccessibilityInstaller.exe` z
[najnowszego wydania](https://github.com/gomods1/BallPitAccess/releases/latest) i uruchom go. Znajduje grę przez Steam, instaluje MelonLoader 0.7.3,
bibliotekę mowy Prism i mod, a później je aktualizuje lub usuwa. Potem uruchom grę: pierwsze
uruchomienie trwa kilka minut i jest ciche, gdy MelonLoader przygotowuje pliki gry; potem twój czytnik
ekranu zaczyna mówić.

**Ręcznie:** pobierz `BallPitAccess-vX.Y.Z.zip` z tej samej strony i rozpakuj wszystko do folderu gry
(domyślnie `C:\Program Files (x86)\Steam\steamapps\common\BALLxPIT`, folder, w którym jest `Balls.exe`), tak aby `version.dll` znalazł się obok `Balls.exe`.
Zip zawiera już MelonLoader: nie trzeba go instalować osobno. Uruchom grę; pierwsze uruchomienie jest
powolne i ciche, jak wyżej.

## Usuwanie moda {#removing-the-mod}

Przycisk Odinstaluj w instalatorze usuwa mod i MelonLoader, a twoje ustawienia zostają w `UserData`.
Ręcznie:

Usuń `Mods\BallPitAccess.dll` i `prism.dll`. Aby usunąć także MelonLoadera, usuń z folderu gry `version.dll`
oraz foldery `MelonLoader`, `Mods`, `Plugins` i `UserData`. Mod nie zmienia żadnego pliku gry.

## Pierwsze kroki {#first-steps}

- Naciśnij **F1** w dowolnej chwili, aby otworzyć pomoc: wszystkie jej części, zaczynając od tej dotyczącej
  miejsca, w którym jesteś. Strzałki w górę i w dół czytają ją wiersz po wierszu, Tab przechodzi do następnej
  części, Escape zamyka.
- Naciśnij **F4**, aby otworzyć ustawienia moda, a w nich otwórz **przewodnik po dźwiękach**: odtwarza każdy
  dźwięk walki i mówi, co oznacza. Przesłuchanie go raz przed pierwszą rozgrywką bardzo pomaga.
- Sama gra przy klawiaturze jest obsługiwana myszą; z modem mysz nie jest potrzebna nigdy, poza bezczynną
  wyrzutnią w bazie (zobacz [Znane ograniczenia](#known-limits)).

## Klawisze dostępne wszędzie {#keys-everywhere}

| Klawisz | Działanie |
|---|---|
| F1 | Pomoc (wszystkie części; Tab / Shift+Tab między częściami) |
| F2 | Odczytaj cały ekran |
| F3 | Powtórz ostatni komunikat |
| F4 | Ustawienia moda i przewodnik po dźwiękach |
| F5 | Zmień tryb pomocy w celowaniu |
| F6 / F7 | Obróć celownik o 10 stopni w lewo / w prawo |
| F8 | Powiedz, gdzie celujesz |
| Ctrl+strzałka w górę | Odczytaj szczegóły zaznaczonego elementu (Ctrl+strzałka w dół wraca) |
| Ctrl+strzałka w lewo / Ctrl+strzałka w prawo | Przełączaj między szczegółami a ostatnimi zdarzeniami |

Gdy otwarta jest lista moda (pomoc, ustawienia, przewodnik po dźwiękach), gra nie otrzymuje twoich klawiszy.

## Menu {#menus}

Każdy ekran to **jedna lista w sensownej kolejności**: najpierw jego opcje, potem przyciski, na końcu Zamknij.
Kolejność nigdy nie zależy od tego, gdzie elementy są narysowane na ekranie.

| Klawisz | Działanie |
|---|---|
| Strzałka w górę / w dół | Poprzedni / następny element |
| Enter | Wybierz |
| Tab / Shift+Tab | Następna / poprzednia grupa ekranu |
| Home / End | Pierwszy / ostatni element grupy |
| Strzałka w lewo / w prawo | Zmień ustawienie typu lista lub suwak; na poziomie zmień jego trudność |
| Ctrl+Tab / Ctrl+Shift+Tab | Następna / poprzednia zakładka na ekranach z zakładkami |
| Backspace lub Escape | Wstecz |
| Page Up / Page Down | Zestaw w encyklopedii; poziom nowej gry plus na liście poziomów |
| Shift+Enter | Na poziomie: otwórz jego tablicę rekordów |

- Enter na postaci przenosi cię na jej przycisk Wybierz.
- Gdy samouczek coś wskazuje, fokus przeskakuje na to miejsce: naciśnij Enter.
- Opcje ulepszeń są odczytywane razem z liczbami („Zadaje od 10 do 15 obrażeń”).

## Baza (Nowe Kulopolis) {#the-base-new-ballbylon}

| Klawisz | Działanie |
|---|---|
| Strzałka w górę / w dół, Enter | Przyciski panelu bocznego: Graj, Zbiory, Buduj, Powiększ, Układaj, Opcje |
| Tab / Shift+Tab | Przechodzenie po windzie i twoich budowlach |
| Enter na budowli | Rozpocznij zbiory wycelowane w nią, jeśli jest gotowa; w przeciwnym razie otwórz jej ulepszenie |
| Shift+Enter | Otwórz ulepszenie budowli |
| Enter na windzie | Otwórz ulepszenie windy (w głąb czeluści albo w górę do Wyniesionego Kulopolis) |
| Delete | Wyburz budowlę |
| H | Zasoby i dzień |
| J | Zbiory i robotnicy |
| K | Zwiedzanie bazy pole po polu |

- **Budowanie**: strzałki przesuwają budowlę o jedno pole i mówią, czy się mieści, Enter buduje, R obraca,
  Home przeskakuje na wolne miejsce, Backspace anuluje.
- **Zbiory**: zaczynają się wycelowane w budowlę. Enter wysyła robotników, Tab celuje w inną budowlę,
  strzałki w lewo / w prawo obracają celownik, przytrzymanie spacji przyspiesza, Escape wychodzi.
- **Powiększanie**: Tab wybiera działkę, Enter ją kupuje.
- **Układanie**: Tab wybiera budowlę, Enter ją podnosi, strzałki ją przesuwają, Enter ją upuszcza.
- **Eksploracja** (K): strzałki przechodzą o jedno pole i mówią, co tam jest: wolne (i ile wolnych pól jest dalej w prawo i w dół), budynek z rozmiarem albo teren niekupiony. Tab / Shift+Tab skaczą do następnego / poprzedniego wolnego pola. K, Backspace lub Escape wychodzą. Kolumny i rzędy są te same co przy stawianiu budynku.
- Menu budowy mówi, ile miejsca zajmuje każdy budynek („3 szerokości, 2 wysokości”), a stawianie zaczyna się od nazwy i rozmiaru.

## Walka {#battle}

### Klawisze samej gry

| Klawisz | Działanie |
|---|---|
| W A S D | Ruch |
| Spacja | Strzał (przytrzymaj) |
| F | Automatyczny ostrzał wł. / wył. |
| E | Otwórz nowy poziom, gdy jest gotowy |
| = / - | Szybsza / wolniejsza prędkość gry (na niektórych układach klawiatury ich brak: użyj Num + / Num - moda, niżej) |
| Escape | Pauza |

### Klawisze moda

| Klawisz | Działanie |
|---|---|
| Strzałka w lewo / w prawo | Obróć celownik ręcznie o 5 stopni |
| Strzałka w górę | Oddaj celowanie pomocy |
| Strzałka w dół | Powiedz, gdzie celujesz |
| Shift (przytrzymaj) | Pomoc w łapaniu (zobacz [Pomoce](#assists)) |
| 1 do 7, 0 | Filtry dźwięków (zobacz [Filtry dźwięków](#sound-filters-number-keys)) |
| H | Zdrowie i efekty statusu |
| J | Wrogowie i boss |
| K | Twoja pozycja |
| G | Prędkość gry |
| L | Poziom, czas, fala, zdobyte koła zębate |
| B | Twoje kule i zdolności pasywne |
| P | Przedmioty na polu |
| N | Zagrożenia (pociski, lasery, bomby, ziemia) |
| Q | Co to był za dźwięk? Dźwięki, które grają teraz, i kilka ostatnich, z tym, co każdy oznacza, i jego stroną |
| Num + / Num - | Szybsza / wolniejsza prędkość gry, przy każdym układzie klawiatury |

Każdy klawisz moda można zmienić: zobacz [Zmiana klawiszy](#changing-keys).

W czasie walki mod mówi: poziom na początku, bossów i ich zdrowie przy 75, 50 i 25 procentach, obrażenia
i zdrowie, efekty statusu, przedmioty specjalne, gdy wypadną, „gotowe nowe poziomy, naciśnij E” oraz koniec
rozgrywki.

## Dźwięki walki {#battle-sounds}

Każdy dźwięk podlega trzem zasadom, zawsze względem twojego bohatera:

- **Strona**: dźwięk dochodzi stamtąd, gdzie jest to, czego dotyczy: z lewej, gdy jest po twojej lewej, z
  prawej, gdy jest po twojej prawej, ze środka, gdy jest prosto przed tobą.
- **Wysokość**: wyższy oznacza bliżej.
- **Stłumiony** oznacza, że jest za tobą.

Rzeczy, które ciągle się poruszają (twoja kula, wrogie pociski), mają **dźwięki ciągłe**, które za nimi
podążają; rzeczy, które zmieniają się skokowo (nacierający wrogowie, przedmioty leżące na ziemi), mają krótkie
dźwięki.

| Dźwięk | Znaczenie |
|---|---|
| Bębenek ręczny | Wróg. Wyżej i szybciej = bliżej. Gdy wrogowie się ruszają, najbliżsi grają po kolei; dopóki nic się nie zmienia, gra tylko najbliższy. |
| Duży, głęboki bęben | Boss. Bębny, gong i róg, gdy się pojawia. |
| Warknięcie | Wróg zaraz do ciebie dotrze. |
| Narastający pisk | Wróg szykuje atak, po swojej stronie. |
| **Toczące się buczenie** (ciągłe) | Kula, którą możesz złapać. Słychać je **z miejsca, w którym jest kula**, i **staje się wyższe, im bliżej ciebie jest kula**, więc słyszysz, dokąd leci; pulsuje szybciej, gdy kula zbliża się do twojej linii, i jest cichsze, dopóki kula może jeszcze odbić się od wroga. |
| Kliknięcie | Stoisz tam, gdzie kula wyląduje: zostań tam. |
| Plaśnięcie rękawicy | Kula złapana. |
| Głuche, opadające uderzenie | Kula cię minęła. |
| Przeładowanie broni | Twoje kule wróciły: możesz strzelać (ciche przy automatycznym ostrzale). |
| Brzęk szkła | Kryształ PD, złoto lub surowiec, do którego warto podejść: najbliższy albo kupka trochę dalej. Nie zbierają się same. |
| Migotanie kalimby | Przedmiot specjalny: reaktor fuzji, schemat, jajo, koło zębate, zdrowie, magnes, bomba, sakiewka klejnotów. |
| **Powietrzny gwizd** (ciągły) | Wrogi pocisk lecący blisko ciebie, wyższy, gdy się zbliża. Głośniejszy, gdy cię trafi. |
| Świst i trzask | Pocisk cię trafi; dźwięk dochodzi od pocisku: zejdź mu z drogi. |
| Elektryczne buczenie / dwa trzaski | Laser celuje / jesteś na jego linii; oba dźwięki dochodzą od lasera: zejdź z jego linii. |
| Opadający gwizd / syk lontu | W pobliżu spada bomba / spadnie na ciebie; oba dźwięki dochodzą z miejsca, w którym wyląduje: odsuń się od niego. |
| Chlupot | Stoisz na zagrożeniu na ziemi (mod mówi jego nazwę). |
| Cios / łyk | Obrażenia / leczenie. |
| Bicie serca | Mało zdrowia; szybsze, gdy zdrowia jest bardzo mało. |
| Chwiejne falowanie | Dotknął cię efekt statusu. |
| Dwa rosnące dźwięki dzwonka | Nowy poziom jest gotowy (naciśnij E). |
| Drewniane stuknięcie | Boczna krawędź areny (także koniec listy w menu). |
| Metalowe stuknięcie | Przednia lub tylna granica obszaru, po którym chodzisz. |
| Kliknięcie przełącznika | Wspomaganie celowania przełączyło się na inny cel, po jego stronie. Przy automatycznym strzelaniu nie po zabiciu wroga. |

Każdy z tych dźwięków można usłyszeć w **F4 → Przewodnik po dźwiękach**, a strzałkami w lewo / w prawo
odtworzyć go po każdej stronie.
W walce **Q** mówi, jakie dźwięki właśnie było słychać.

## Filtry dźwięków (klawisze cyfr) {#sound-filters-number-keys}

W walce klawisze cyfr wyciszają lub przywracają grupę dźwięków, żeby łatwiej było się skupić. Gra nie używa
żadnego z tych klawiszy.

| Klawisz | Grupa |
|---|---|
| 1 | Wrogowie: bębny, warknięcie, szykowanie ataku, pojawienie się bossa |
| 2 | Kule: toczące się buczenie, kliknięcie, złapanie, chybienie, przeładowanie broni |
| 3 | Nagrody: kryształy, złoto, zasoby, przedmioty specjalne |
| 4 | Pociski i zagrożenia |
| 5 | Krawędzie areny |
| 6 | Twoje zdrowie: obrażenia, leczenie, bicie serca, stan |
| 7 | Kliknięcie pomocy w celowaniu |
| 0 | Wszystko z powrotem włączone |

- Każda cyfra przełącza swoją grupę, niezależnie od pozostałych.
- **Ctrl+cyfra** zostawia tylko tę grupę. Przykład: tylko wrogowie i nagrody = Ctrl+1, potem 3.
- Twój wybór zostaje zapamiętany na następną sesję. Gdy walka zaczyna się z czymś wyciszonym, mod mówi,
  co jest wyciszone.
- Wyciszenie zmienia tylko dźwięki: klawisze H, J, P, N nadal odpowiadają, a pomoc w łapaniu nadal robi uniki.

## Pomoce {#assists}

Wszystko, co robi pomoc, można też zrobić na słuch; pomoce są do dyspozycji, kiedy ich chcesz.

- **Pomoc w celowaniu** (F5 zmienia tryb): celuje za ciebie w najbliższego wroga (domyślnie), najsilniejszego
  wroga, wroga przed tobą, pod stałym kątem albo jest wyłączona (mysz). Strzałki pozwalają ją dostroić. Celuje w część wroga, której nie zasłania żaden inny, pomija wrogów pod kątem, pod którym twój bohater nie może strzelać, a w trybie domyślnym najpierw bierze bossa, chyba że inny wróg zaraz do ciebie dotrze.
- **Pomoc w łapaniu** (przytrzymaj Shift): bohater idzie pod następną spadającą kulę; gdy żadna kula nie
  nadlatuje, idzie do najbliższego kryształu lub przedmiotu; jeśli pocisk zaraz cię trafi, najpierw schodzi
  mu z drogi. W / S nadal przesuwają cię do przodu i do tyłu, A / D zawsze przejmują sterowanie. W F4 można ją
  wyłączyć, ustawić na „przytrzymaj Shift” (domyślnie) albo na tryb automatyczny. Gdy jesteś ranny, najpierw idzie po przedmiot leczący.

## Ustawienia moda (F4) {#mod-settings-f4}

Strzałki w górę / w dół przechodzą, strzałki w lewo / w prawo lub Enter zmieniają, Escape zamyka. Ustawienia
są zapisywane.

- Tryb pomocy w celowaniu
- Pomoc w łapaniu: wyłączona, przytrzymaj Shift, automatyczna
- Dźwięki pomocnicze wł. / wył., głośność dźwięków pomocniczych
- Radar wrogów, dźwięki kul, krawędzie areny, pociski i zagrożenia (wł. / wył.)
- Dźwięki przedmiotów: wyłączone, tylko przedmioty specjalne, wszystkie
- Dźwięki zdrowia, kliknięcie pomocy w celowaniu (wł. / wył.)
- Czytanie opisów ulepszeń po zaznaczeniu
- Klawisze (Enter otwiera): zobacz [Zmiana klawiszy](#changing-keys)
- Przewodnik po dźwiękach (Enter otwiera)
- Dziennik debugowania
- Nagrywanie walk (domyślnie wyłączone): każda walka jest zapisywana do `UserData\BallPitAccess\telemetry.tsv`, do strojenia dźwięków

## Zmiana klawiszy {#changing-keys}

**F4 → Klawisze** wymienia każdy klawisz moda razem z klawiszem, na którym jest teraz. Strzałki w górę / w
dół wybierają, **Enter**, a potem naciśnij nowy klawisz (Escape anuluje), **Delete** przywraca klawisz
domyślny, a ostatnia pozycja przywraca wszystkie. Jeśli nowy klawisz należy już do innej akcji moda, oba
się zamieniają, a mod mówi, dokąd trafiła ta druga. Klawiszy potrzebnych menu lub grze (strzałki, Enter,
Tab, Escape, Backspace, Spacja, W A S D F E R X) nie można zająć. Pomoc F1 kończy się klawiszami w obecnym
ustawieniu.

## Wskazówki na pierwszą rozgrywkę {#tips-for-a-first-run}

- **Łap swoje kule.** Złapaną kulę można od razu wystrzelić ponownie; chybiona wraca po 2 do 3 sekund.
  Podążaj za toczącym się buczeniem, aż usłyszysz kliknięcie, a potem zostań na miejscu.
- **Zbieraj kryształy PD.** Nie zbierają się same. Podążaj za brzękiem szkła; stłumiony oznacza, że kryształ
  jest za tobą.
- **Najpierw przetrwaj.** Na początku rozgrywki pozostanie przy życiu jest ważniejsze niż obrażenia.
- **Łącz w fuzje i rozwijaj w ewolucje** swoje kule, gdy nowy poziom to oferuje: opcje mówią „fuzja” albo
  „ewolucja”.
- **Zwolnij przy bossach** klawiszem Num - (albo klawiszem minus gry). Mod przypomina o tym, gdy boss
  pojawia się przy dużej prędkości.
- Śmierć to normalna rzecz: złoto, które przynosisz, rozbudowuje Nowe Kulopolis, a budowle wzmacniają każdą
  kolejną rozgrywkę.

## Znane ograniczenia {#known-limits}

- Bezczynna wyrzutnia w bazie nadal wymaga myszy.
- Tablice rekordów i głosowanie na Twitchu są odczytywane tylko w podstawowym zakresie. Nie da się pominąć
  napisów końcowych.
- Ustawienia moda są pod F4, a nie na ekranie opcji samej gry.
- Własne teksty moda w językach innych niż angielski i hiszpański zostały przetłumaczone bez sprawdzenia przez
  rodzimego użytkownika języka. Poprawki są mile widziane. Nazwy z gry (budowle, kule, wrogowie, ulepszenia)
  zawsze pochodzą z oficjalnego tłumaczenia gry.
- Głośność dźwięków, ich tempo i szerokość łapania nie zostały jeszcze dostrojone z udziałem graczy.

## Zgłaszanie problemu {#reporting-a-problem}

Przydatne pliki w folderze gry:

- `UserData\BallPitAccess\speech.log`: wszystko, co powiedział mod.
- `MelonLoader\Latest.log`: dziennik moda (błędy, ekrany, złapania i chybienia).
- W walce **Ctrl+Shift+D** zapisuje `UserData\BallPitAccess\threats.txt` (wrogowie, kule, pociski, przedmioty
  wraz z pozycjami) oraz listę otwartego ekranu do `Latest.log`: przydatne, gdy dźwięk wskazuje zły kierunek
  albo menu czyta elementy w złej kolejności.
- `UserData\BallPitAccess\telemetry.tsv`, jeśli włączono F4 → Nagrywanie walk: każdy dźwięk, wróg, kula i
  pocisk z twoich walk. Najlepsza pomoc, gdy trudno nadążyć za dźwiękami.

Napisz, co zrobiłeś, co usłyszałeś i czego się spodziewałeś.

## Autorzy {#credits}

- BALL x PIT stworzył Kenny Sun, wydało Devolver Digital. Ten mod jest nieoficjalny.
- MelonLoader (LavaGang) ładuje mod. Prism przekazuje mowę do twojego czytnika ekranu.
