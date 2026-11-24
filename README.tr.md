# BALL x PIT Access

**BALL x PIT** oyununu (Kenny Sun / Devolver Digital) görmeden oynanabilir hâle getiren bir mod. Tüm
menüleri ekran okuyucun aracılığıyla seslendirir, oyunun tamamını klavyeyle kullanmanı sağlar ve savaş
alanını sese dönüştürür: düşmanların, toplarının, düşman atışlarının, TP kristallerinin ve özel eşyaların
her birinin kendi sesi vardır ve bu ses, senin hangi tarafındaysa oradan gelir.

Sürüm 0.8.0, oyunun 1.308 sürümü için, Yükselen Babil'in Sekme Bahçeleri (Risen Ballbylon) DLC'si dahil.
Mod **oyunun sunduğu tüm dilleri** konuşur ve oyunun dil ayarını izler.

<!-- LANGS -->
[English](README.html) · [Español](README.es.html) · [Français](README.fr.html) · [Deutsch](README.de.html) · [Italiano](README.it.html) · [Português (Brasil)](README.pt-BR.html) · [Polski](README.pl.html) · **Türkçe** · [Русский](README.ru.html) · [Українська](README.uk.html) · [日本語](README.ja.html) · [한국어](README.ko.html) · [简体中文](README.zh-CN.html) · [繁體中文](README.zh-TW.html) · [ไทย](README.th.html)
<!-- /LANGS -->

> **Durum: erken test sürümü.** Menüler, üs ve ilk turlar modla oynandı. Savaş sesleri ve en yeni
> bölümler (sürekli top sesi, yakalama yardımı, ses filtreleri) henüz oyuncularla birlikte ayarlanmadı.
> Lütfen neyin çalışıp neyin çalışmadığını bildir (bkz. [Sorun bildirme](#reporting-a-problem)).

## İçindekiler {#contents}

- [Neye ihtiyacın var](#what-you-need)
- [Kurulum](#installing)
- [Modu kaldırma](#removing-the-mod)
- [İlk adımlar](#first-steps)
- [Her yerde geçerli tuşlar](#keys-everywhere)
- [Menüler](#menus)
- [Üs (Yeni Babil'in Sekme Bahçeleri)](#the-base-new-ballbylon)
- [Savaş](#battle)
- [Savaş sesleri](#battle-sounds)
- [Ses filtreleri (rakam tuşları)](#sound-filters-number-keys)
- [Yardımlar](#assists)
- [Mod ayarları (F4)](#mod-settings-f4)
- [Tuşları değiştirme](#changing-keys)
- [Kumanda](#controller)
- [İlk tur için ipuçları](#tips-for-a-first-run)
- [Bilinen sınırlamalar](#known-limits)
- [Sorun bildirme](#reporting-a-problem)
- [Emeği geçenler](#credits)

## Neye ihtiyacın var {#what-you-need}

- Windows'ta (Steam) BALL x PIT, sürüm 1.308.
- Bir ekran okuyucu: test edilen NVDA'dır. Prism konuşma kitaplığının desteklediği diğerleri (JAWS,
  Ekran Okuyucu (Narrator), SAPI sesleri) de çalışmalıdır.
- Mod yükleyicisi **MelonLoader 0.7.3** (x64). Ücretsizdir, resmî GitHub sürümler sayfasından indirilir.
- Kulaklık ya da stereo hoparlör: savaşta sol ve sağ çok önemlidir.

## Kurulum {#installing}

**Yükleyiciyle (en kolayı):** `BallPitAccessibilityInstaller.exe` dosyasını
[son sürümden](https://github.com/gomods1/BallPitAccess/releases/latest) indir ve çalıştır. Oyunu Steam üzerinden bulur, MelonLoader 0.7.3'ü, Prism konuşma
kitaplığını ve modu kurar; daha sonra bunları günceller ya da kaldırır. Ardından oyunu başlat: ilk
açılış birkaç dakika sürer ve MelonLoader oyunun dosyalarını hazırlarken sessizdir; sonra ekran
okuyucun konuşmaya başlar.

**Elle:** aynı sayfadan `BallPitAccess-vX.Y.Z.zip` dosyasını indir ve her şeyi oyun klasörüne çıkar
(varsayılan olarak `C:\Program Files (x86)\Steam\steamapps\common\BALLxPIT`, `Balls.exe` dosyasının bulunduğu klasör); böylece `version.dll`, `Balls.exe`
dosyasının yanına gelir. Zip zaten MelonLoader'ı içerir: ayrıca kurman gerekmez. Oyunu başlat; ilk
açılış yukarıdaki gibi yavaş ve sessizdir.

## Modu kaldırma {#removing-the-mod}

Yükleyicinin Kaldır düğmesi modu ve MelonLoader'ı kaldırır, ayarlarını `UserData` içinde tutar.
Elle:

`Mods\BallPitAccess.dll` ve `prism.dll` dosyalarını sil. MelonLoader'ı da kaldırmak için oyun klasöründen
`version.dll` dosyasını ve `MelonLoader`, `Mods`, `Plugins` ve `UserData` klasörlerini sil. Mod, oyunun
hiçbir dosyasını değiştirmez.

## İlk adımlar {#first-steps}

- Yardım için istediğin zaman **F1** tuşuna bas: yardımın tüm bölümleri, bulunduğun yerle ilgili bölümden
  başlayarak açılır. Yukarı ve aşağı oklar satır satır okur, Tab sonraki bölüme atlar, Esc kapatır.
- Modun ayarları için **F4** tuşuna bas ve içinden **ses rehberini** aç: her savaş sesini çalar ve ne
  anlama geldiğini söyler. İlk turundan önce bir kez dinlemek çok işe yarar.
- Klavyeyle oynarken oyunun kendisi fareyle kullanılır; modla birlikte üsteki boşta fırlatıcı dışında
  fareye hiç ihtiyacın olmaz (bkz. [Bilinen sınırlamalar](#known-limits)).

## Her yerde geçerli tuşlar {#keys-everywhere}

| Tuş | İşlev |
|---|---|
| F1 | Yardım (tüm bölümler; bölümler arasında Tab / Shift+Tab) |
| F2 | Tüm ekranı oku |
| F3 | Son söyleneni tekrarla |
| F4 | Mod ayarları ve ses rehberi |
| F5 | Nişan yardımının modunu değiştir |
| F6 / F7 | Nişanı 10 derece sola / sağa çevir |
| F8 | Nereye nişan aldığını söyle |
| Ctrl+Yukarı ok | Odaklı ögenin ayrıntılarını oku (Ctrl+Aşağı ok geri döner) |
| Ctrl+Sol ok / Ctrl+Sağ ok | Ayrıntılar ile son olaylar arasında geçiş yap |

Bir mod listesi (yardım, ayarlar, ses rehberi) açıkken oyun tuşlarını almaz.

## Menüler {#menus}

Her ekran **mantıklı bir sırada tek bir listedir**: önce seçenekleri, sonra düğmeleri, en sonda Kapat.
Sıra hiçbir zaman ögelerin ekranda nereye çizildiğine bağlı değildir.

| Tuş | İşlev |
|---|---|
| Yukarı ok / Aşağı ok | Önceki / sonraki öge |
| Enter | Seç |
| Tab / Shift+Tab | Ekrandaki sonraki / önceki grup |
| Home / End | Grubun ilk / son ögesi |
| Sol ok / Sağ ok | Liste veya kaydırıcı ayarını değiştir; bir seviyede zorluğunu değiştir |
| Ctrl+Tab / Ctrl+Shift+Tab | Sekmeli ekranlarda sonraki / önceki sekme |
| Backspace veya Esc | Geri |
| Page Up / Page Down | Ansiklopedide set; seviye listesinde yeni oyun artı (Yeni Oyun+) kademesi |
| Shift+Enter | Bir seviyede: liderlik tablosunu aç |

- Bir karakterin üzerinde Enter seni onun Seç düğmesine götürür.
- Rehberli öğretici bir şeyi gösterdiğinde odak oraya atlar: Enter tuşuna bas.
- Geliştirme seçenekleri sayılarıyla birlikte okunur ("10 ile 15 arası hasar verir").

## Üs (Yeni Babil'in Sekme Bahçeleri) {#the-base-new-ballbylon}

| Tuş | İşlev |
|---|---|
| Yukarı ok / Aşağı ok, Enter | Kenar çubuğu düğmeleri: Oyna, Hasat, İnşa et, Genişlet, Yeniden düzenle, Seçenekler |
| Tab / Shift+Tab | Asansör ve binaların arasında gezin |
| Bir binada Enter | Hazırsa ona nişan alan bir hasat başlat; değilse geliştirmesini aç |
| Shift+Enter | Bir binanın geliştirmesini aç |
| Asansörde Enter | Asansör geliştirmesini aç (çukurun dibine doğru ya da Yükselen Babil'in Sekme Bahçeleri'ne doğru) |
| Delete | Bir binayı parçala |
| H | Kaynaklar ve gün |
| J | Hasat ve işçiler |
| K | Üssü kare kare keşfet |
| I | Bina yerleştirirken veya taşırken: bu yerin kimin yolunu kestiği |

- **İnşa**: oklar binayı bir kare taşır ve sığıp sığmadığını söyler, Enter inşa eder, R döndürür, Home boş
  bir yere atlar, Backspace iptal eder.
- **Hasat**: bir binaya nişan alarak başlar. Enter işçileri fırlatır, Tab başka bir binaya nişan alır,
  Sol ok / Sağ ok nişanı çevirir, hızlandırmak için Boşluk tuşunu basılı tut, Esc çıkar.
- **İnşaat**: inşaat hâlindeki veya geliştirilen binalar da hasadın Tab listesindedir. Ondan seken her işçi onu inşa eder; bina tamamlanınca mod söyler. Üste birinin üzerinde Enter, ona nişan alan hasadı başlatır.
- **İşçiler ulaşacak mı?** Hasat bir binayı hedeflediğinde (Tab ya da üste binanın üzerinde Enter), mod işçilerin yolunu oyunun onları hareket ettirdiği gibi izler ve ona ulaşan bir açı seçer: "işçiler doğrudan vurur", "işçiler bir duvardan sekerek ulaşır", "işçiler üzerinden geçer" (buğday tarlaları) ya da "açık yol yok, Orman önünde". Açık yolu olmayan bir binaya ancak şans eseri ulaşılır: onu ya da yoldaki binayı taşı. Sol / Sağ çizginin neye vurduğunu söyler.
- **Bir hasattan sonra** mod inşa hâlindeki her binanın ne aldığını söyler ("Perili Ev: 6 vuruş, yüzde 40 → yüzde 55"). Oyunun kendi özeti yalnızca kaynakları sayar: hiçbir şey toplanmadığında, işçiler inşa etmiş olsa bile "Nafile Bir Çaba" der.
- **Genişletme**: Tab bir arsa seçer, Enter satın alır.
- **Yeniden düzenleme**: Tab bir bina seçer, Enter onu kaldırır, oklar taşır, Enter bırakır.
- **Keşif** (K): oklar bir kare ilerler ve orada ne olduğunu söyler: boş (ve sağa ve aşağı kaç boş kare daha olduğu), boyutuyla bir bina ya da satın alınmamış arazi. Tab / Shift+Tab sonraki / önceki boş kareye atlar. K, Backspace veya Escape çıkar. Sütun ve satırlar bina yerleştirirken kullanılanlarla aynıdır.
- İnşa menüsü her binanın ne kadar yer kapladığını söyler ("3 genişlik, 2 yükseklik"), yerleştirme de adı ve boyutuyla başlar.
- **Bina yerleştirme veya taşıma**: konum onu neyin engellediğini söyler ("Orman engelliyor"). I, işçilerin bu yere ulaşıp ulaşmadığını ve hangi binaların yolunu keseceğini ya da onları yalnızca duvardan sekerek ulaşılabilir bırakacağını söyler. Home işçilerin doğrudan vurduğu ve kimsenin yolunu kesmeyen en iyi boş yeri bulur ve binayı sığacak şekilde döndürür. R'den sonra yeni şekil söylenir: perili ev gibi bazı binalar dikdörtgen değildir ("sol üst köşe açık"). Yeniden düzenlerken de binalar döndürülebilir.
- **Keşif**: Page Down / Page Up sonraki / önceki binaya atlar ve nerede olduğunu, boyutunu ve şeklini söyler.

## Savaş {#battle}

### Oyunun kendi tuşları {#the-games-own-keys}

| Tuş | İşlev |
|---|---|
| W A S D | Hareket et |
| Boşluk | Ateş et (basılı tut) |
| F | Oto saldırı açık / kapalı |
| E | Hazır olduğunda seviye atlamayı aç |
| = / - (eşittir / eksi) | Oyun hızını artır / azalt (bazı klavye düzenlerinde bulunmaz: modun aşağıdaki Num + / Num - tuşlarını kullan) |
| Esc | Duraklat |

### Mod tuşları {#mod-keys}

| Tuş | İşlev |
|---|---|
| Sol ok / Sağ ok | Nişanı elle 5 derece çevir |
| Yukarı ok | Nişanı yardıma geri ver |
| Aşağı ok | Nereye nişan aldığını söyle |
| Shift (basılı tut) | Yakalama yardımı (bkz. [Yardımlar](#assists)) |
| 1 ile 7 arası, 0 | Ses filtreleri (bkz. [Ses filtreleri](#sound-filters-number-keys)) |
| H | Can ve durum etkileri |
| U | Kalan saniyeleriyle durum etkileri, hareket edip edemeyeceğin ve edemiyorsan nedeni (dondun, bir darbeyle geri itildin, düşmanlar ilerliyor). Donma bitince mod "Yeniden hareket edebilirsin" der |
| J | Düşmanlar ve patron |
| K | Konumun |
| G | Oyun hızı |
| L | Seviye, süre, dalga, kazanılan dişliler |
| B | Topların ve pasiflerin |
| P | Sahadaki eşyalar |
| N | Tehlikeler (atışlar, lazerler, bombalar, zemin) |
| Q | O ses neydi? Şu an çalan sesler ve son birkaç ses, her birinin anlamı ve tarafıyla |
| M | Savaş haritası (duraklatır) |
| Num + / Num - | Oyun hızını artır / azalt, her klavye düzeninde |

Her mod tuşu değiştirilebilir: bkz. [Tuşları değiştirme](#changing-keys).

Savaş sırasında seslendirilenler: başlangıçta seviye, patronlar ve yüzde 75, 50 ve 25'teki canları, hasar ve
can, durum etkileri, düştüklerinde özel eşyalar, "seviye atlama hazır, E tuşuna bas" ve turun sonu.

**Boss'lar**: J, boss'un gerçek canını söyler; oyundaki boss çubuğuyla aynıdır. Bazı boss'lar yalnızca zayıf noktalarından hasar alır: İskelet Kral'ın gövdesi yaralanamaz, yalnızca başındaki taç yaralanabilir ve gövdesi onu önden korur; toplar ona duvarlardan sekerek ulaşır. J zayıf noktanın nerede olduğunu söyler ve nişan yardımı onu hedefler.

**Savaş haritası (duraklatır)**: M savaş haritasını açar ve oyunu duraklatır. Oklar alanı kare kare gezer ve orada ne olduğunu söyler: canlarıyla düşmanlar, topların, atışlar, lazerler, bombalar, zemin tehlikeleri ve eşyalar. Control ve yukarı ok bir düşmanın ne yaptığını, durumunu ve etkilerini söyler. Tab düşmandan düşmana atlar, önce en yakını; Page Down diğer şeylere; Home sana döner; End her şeyi sayar. Enter nişanı düşmana kilitler; düşman ölünce nişan en yakın düşmana döner. Delete kilidi açar. M veya Escape haritayı kapatır ve savaş devam eder.

## Savaş sesleri {#battle-sounds}

Her ses için, her zaman kahramanına göre, üç kural geçerlidir:

- **Taraf**: bir ses, ilgili olduğu şey neredeyse oradan gelir: solundaki soldan, sağındaki sağdan,
  dümdüz ilerideki ortadan duyulur.
- **Perde**: daha tiz ses daha yakın demektir.
- **Boğuk** ses arkanda demektir.

Sürekli hareket eden şeyler (topun, düşman atışları) onları izleyen **sürekli seslerdir**; adım adım
değişen şeyler (ilerleyen düşmanlar, yerde duran eşyalar) kısa seslerdir.

| Ses | Anlamı |
|---|---|
| El davulu | Bir düşman. Daha tiz ve daha hızlı = daha yakın. Düşmanlar hareket edince en yakınları sırayla çalar; hiçbir şey değişmezken yalnızca en yakını çalar. |
| Büyük, derin davul | Bir patron. Patron geldiğinde davullar, gong ve boru. |
| Hırlama | Bir düşman sana ulaşmak üzere. |
| Yükselen inilti | Bir düşman, bulunduğu tarafta saldırıya hazırlanıyor. |
| **Yuvarlanan uğultu** (sürekli) | Yakalayabileceğin top. **Topun bulunduğu yerden** duyulur ve **top sana yaklaştıkça tizleşir**, böylece nereye gittiğini duyarsın; top senin sırana yaklaştıkça daha hızlı titreşir ve top hâlâ bir düşmandan sekebilecekken daha yumuşaktır. |
| Tık | Topun düşeceği yerde duruyorsun: orada kal. |
| Eldivene tok vuruş | Top yakalandı. |
| Batan boğuk güm | Bir top yanından geçip gitti. |
| Silah doldurma sesi | Topların döndü: ateş edebilirsin (oto saldırı açıkken sessiz). |
| Cam şıngırtısı | Yürüyerek gideceğin TP kristali, altın ya da kaynak: en yakını ya da biraz ötedeki bir yığın. Kendiliğinden toplanmazlar. |
| Kalimba pırıltısı | Özel bir eşya: füzyon reaktörü, taslak, yumurta, dişli, can, mıknatıs, bomba, mücevher kesesi. |
| **Hafif hava ıslığı** (sürekli) | Yakınında uçan bir düşman atışı, yaklaştıkça tizleşir. Sana isabet edecekse daha gür çıkar. |
| Vınlama ve şaklama | Bir atış sana isabet edecek; ses atıştan gelir: yolundan çekil. |
| Elektrik uğultusu / iki elektrik çıtırtısı | Bir lazer nişan alıyor / onun hattındasın; ikisi de lazerden gelir: hattan çekil. |
| Alçalan ıslık / fitil cızırtısı | Yakına bir bomba düşüyor / üstüne düşecek; ikisi de düşeceği yerden gelir: oradan uzaklaş. |
| Şapırtı | Bir zemin tehlikesine bastın (adı seslendirilir). |
| Yumruk / yudum | Hasar / iyileşme. |
| Kalp atışı | Can düşük; çok düşükken daha hızlı. |
| Sersem bir dalgalanma | Üstüne bir durum etkisi geldi. |
| Yükselen iki çan notası | Seviye atlama hazır (E tuşuna bas). |
| Tahtaya vuruş | Arenanın yan kenarı (aynı zamanda bir menü listesinin sonu). |
| Metale vuruş | Yürüme alanının ön ya da arka sınırı. |
| Şalter tıkı | Nişan yardımı başka bir hedefe geçti, yeni hedefin tarafından duyulur. Otomatik ateş açıkken bir öldürmeden sonra çalmaz. |

Bunların her biri **F4 → Ses rehberi** içinde dinlenebilir; Sol ok / Sağ ok ile her iki tarafta da
duyabilirsin.
Savaşta **Q**, az önce duyduğun sesleri söyler.

## Ses filtreleri (rakam tuşları) {#sound-filters-number-keys}

Savaşta rakam tuşları, dikkatini toplayabilmen için bir ses grubunu kapatır veya açar. Oyun bu tuşların
hiçbirini kullanmaz.

| Tuş | Grup |
|---|---|
| 1 | Düşmanlar: davullar, hırlama, saldırı hazırlığı, patronun gelişi |
| 2 | Toplar: yuvarlanan uğultu, tık, yakalama, kaçırma, silah doldurma |
| 3 | Ödüller: kristaller, altın, kaynaklar, özel eşyalar |
| 4 | Atışlar ve tehlikeler |
| 5 | Arena kenarları |
| 6 | Canın: hasar, iyileşme, kalp atışı, durum |
| 7 | Nişan yardımı tıkı |
| 0 | Her şeyi yeniden aç |

- Her rakam, diğerlerinden bağımsız olarak kendi grubunu açar veya kapatır.
- **Ctrl+rakam** yalnızca o grubu açık bırakır. Örnek: yalnızca düşmanlar ve ödüller = Ctrl+1, sonra 3.
- Seçimin bir sonraki oturum için saklanır. Bir savaş, kapalı bir şeyle başladığında mod neyin kapalı
  olduğunu söyler.
- Sesleri kapatmak yalnızca sesleri değiştirir: H, J, P, N tuşları yine yanıt verir ve yakalama yardımı
  yine atışlardan kaçar.

## Yardımlar {#assists}

Bir yardımın yaptığı her şey kulakla da yapılabilir; yardımlar, istediğinde kullanman için oradadır.

- **Nişan yardımı** (F5 modu değiştirir): senin yerine en yakın düşmana (varsayılan), en güçlü düşmana,
  önündeki düşmana ya da sabit bir açıya nişan alır, ya da kapalıdır (fare). Oklarla ince ayar yapılır. Düşmanın başka hiçbir düşmanın örtmediği bir yerine nişan alır, kahramanının ateş edemediği bir açıdaki düşmanları atlar ve varsayılan modda, başka bir düşman sana ulaşmak üzere değilse önce patrona gider.
- **Yakalama yardımı** (Shift basılı tut): kahraman aşağı inen sıradaki topun altına yürür; gelen top
  yoksa en yakın kristale ya da eşyaya yürür; bir atış sana isabet etmek üzereyse önce yolundan çekilir.
  W / S seni yine ileri ve geri götürür, A / D her zaman kontrolü devralır. F4 içinde kapalı, "Shift
  basılı tutulunca" (varsayılan) ya da otomatik olarak ayarlanabilir. Yaralıysan önce bir can eşyasına gider.

## Mod ayarları (F4) {#mod-settings-f4}

Yukarı ok / Aşağı ok gezinir, Sol ok / Sağ ok veya Enter değiştirir, Esc kapatır. Ayarlar kaydedilir.

- Nişan yardımı modu
- Yakalama yardımı: kapalı, Shift basılı tutulunca, otomatik
- Sesli ipuçları açık / kapalı, ipucu ses düzeyi
- Düşman radarı, top sesleri, arena kenarları, atışlar ve tehlikeler (açık / kapalı)
- Eşya sesleri: kapalı, yalnızca özel eşyalar, tümü
- Can sesleri, nişan yardımı tıkı (açık / kapalı)
- Odaklanınca geliştirme açıklamalarını oku
- Kumanda
- Tuşlar (Enter açar): bkz. [Tuşları değiştirme](#changing-keys)
- Ses rehberi (Enter açar)
- Hata ayıklama kaydı
- Savaşları kaydet (varsayılan kapalı): her savaş `UserData\BallPitAccess\telemetry.tsv` dosyasına yazılır, sesleri ayarlamak için

## Tuşları değiştirme {#changing-keys}

**F4 → Tuşlar** her mod tuşunu şu an bulunduğu tuşla birlikte listeler. Yukarı ok / Aşağı ok ile seç,
**Enter** tuşuna bas ve ardından yeni tuşa bas (Esc iptal eder); **Delete** varsayılan tuşu geri getirir,
son satır ise hepsini sıfırlar. Yeni tuş zaten başka bir mod eylemine aitse ikisi yer değiştirir ve mod
diğerinin nereye gittiğini söyler. Menülerin veya oyunun ihtiyaç duyduğu tuşlar (oklar, Enter, Tab, Esc,
Backspace, Boşluk, W A S D F E R X) alınamaz. F1 yardımı, tuşların şu anki haliyle biter.

## Kumanda {#controller}

- Xbox düğmeleri; PlayStation veya Switch kumandası, oyun için Steam Input açıkken çalışır
- Savaş dışında yön tuşları veya sol analog oklar gibi hareket eder, A Enter'dır, B geri gider
- X ve Y ilk ve son öğeye atlar; bina yerleştirirken X sığdığı bir yer bulur, Y onu döndürür
- RB ve LB Tab ve Shift Tab'dır, RT ve LT ekranın sekmelerini değiştirir
- Start oyunun duraklatmasıdır
- Savaşta analoglar, tetikler ve düğmeler oyundadır
- Yön yukarı düşmanları söyler, aşağı sağlığını, sol durumlarını, sağ ilerlemeni
- Yakalamak için sol analoğa basılı tut, Shift gibi
- Sağ analoğa dokunmazsan nişan yardımı senin yerine nişan alır; kendin nişan almak için onu hareket ettir
- Daha fazlası için View'ı basılı tut: A yardım, B mod ayarları, X ekranı okur, Y tekrarlar, Start nişan modunu değiştirir
- View ile LB ve RB gözden geçirme listesinin daha eski ve daha yeni satırlarını okur, View ile analog tıklaması ayrıntılar ve olaylar arasında geçiş yapar
- Savaşta View ile yön tuşları: yukarı tehlikeler, aşağı ekipman, sol eşyalar, sağ son ses; View ile LT ve RT oyunu yavaşlatır ve hızlandırır
- Üste View ile yön tuşları: yukarı hasat, aşağı kaynaklar, sol keşif modu
- Menülerde View ile LT ve RT Page Up ve Page Down'dır

Savaşta sağ çubuğa basmak savaş haritasını açar; duraklatma menüsünde Görünüm ve yön tuşu sağ. Haritada yön tuşu gezer, RB ve LB düşmanlar arasında atlar, A nişanı kilitler ve B kapatır.

## İlk tur için ipuçları {#tips-for-a-first-run}

- **Toplarını yakala.** Yakalanan bir top hemen yeniden fırlatılabilir; kaçırılan bir topun geri gelmesi 2
  ila 3 saniye sürer. Tıkı duyana kadar yuvarlanan uğultuyu izle, sonra olduğun yerde kal.
- **TP kristallerini topla.** Kendiliğinden toplanmazlar. Cam şıngırtısını izle; boğuk ses arkanda demektir.
- **Önce hayatta kal.** Turun başında hayatta kalmak hasardan daha önemlidir.
- Seviye atlama sunduğunda toplarını **birleştir ve evrimleştir**: seçenekler "füzyon" ya da "evrim" der.
- **Patronlarda yavaşla**: Num - tuşunu (ya da oyunun eksi tuşunu) kullan. Bir patron yüksek hızda
  geldiğinde mod sana hatırlatır.
- Ölmek normaldir: geri getirdiğin altın Yeni Babil'in Sekme Bahçeleri'ni inşa eder, binalar da sonraki
  her turu güçlendirir.

## Bilinen sınırlamalar {#known-limits}

- Üsteki boşta fırlatıcı hâlâ fare gerektirir.
- Liderlik tabloları ve Twitch oylaması yalnızca temel düzeyde okunur. Emeği geçenler ekranı geçilemez.
- Mod ayarları oyunun kendi seçenekler ekranında değil, F4 tuşundadır.
- Modun kendi metinleri, İngilizce ve İspanyolca dışındaki dillerde (Türkçe dahil), henüz anadili olan
  biri tarafından gözden geçirilmeden çevrildi. Düzeltmeler memnuniyetle karşılanır. Oyundan gelen adlar
  (binalar, toplar, düşmanlar, geliştirmeler) her zaman oyunun kendi çevirisinden alınır.
- Ses düzeyleri, hızları ve yakalama genişliği henüz oyuncularla birlikte ayarlanmadı.

## Sorun bildirme {#reporting-a-problem}

Oyun klasöründeki yararlı dosyalar:

- `UserData\BallPitAccess\speech.log`: modun söylediği her şey.
- `MelonLoader\Latest.log`: modun günlüğü (hatalar, ekranlar, yakalamalar ve kaçırmalar).
- Savaşta **Ctrl+Shift+D**, `UserData\BallPitAccess\threats.txt` dosyasını (konumlarıyla birlikte
  düşmanlar, toplar, atışlar, eşyalar) yazar ve açık ekranın listesini `Latest.log` dosyasına ekler: bir
  ses yanlış yönü gösterdiğinde ya da bir menü yanlış sırada okunduğunda işe yarar.
- `UserData\BallPitAccess\telemetry.tsv`, F4 → Savaşları kaydet açıksa: savaşlarındaki her ses, düşman, top ve
  atış. Sesleri takip etmek zor olduğunda en iyi yardım.

Ne yaptığını, ne duyduğunu ve ne beklediğini anlat.

## Emeği geçenler {#credits}

- BALL x PIT, Kenny Sun tarafından yapıldı ve Devolver Digital tarafından yayımlandı. Bu mod resmî değildir.
- Modu MelonLoader (LavaGang) yükler. Prism, konuşmayı ekran okuyucuna gönderir.
