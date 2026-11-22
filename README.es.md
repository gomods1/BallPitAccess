# BALL x PIT Access

Un mod que hace **BALL x PIT** (Kenny Sun / Devolver Digital) jugable sin ver. Lee todos los menús con tu
lector de pantalla, te deja usar todo el juego con el teclado y convierte el campo de batalla en sonido:
los enemigos, tus bolas, los disparos enemigos, los cristales de experiencia y los objetos especiales
tienen cada uno su sonido, que viene de su lado respecto a ti.

Versión 0.7.1, para la versión 1.308 del juego, incluido el DLC Bolabilonia Resurgida. El mod habla
**todos los idiomas del juego** y sigue el idioma que tengas puesto en él.

<!-- LANGS -->
[English](README.html) · **Español** · [Français](README.fr.html) · [Deutsch](README.de.html) · [Italiano](README.it.html) · [Português (Brasil)](README.pt-BR.html) · [Polski](README.pl.html) · [Türkçe](README.tr.html) · [Русский](README.ru.html) · [Українська](README.uk.html) · [日本語](README.ja.html) · [한국어](README.ko.html) · [简体中文](README.zh-CN.html) · [繁體中文](README.zh-TW.html) · [ไทย](README.th.html)
<!-- /LANGS -->

> **Estado: versión de prueba temprana.** Los menús, la base y las primeras partidas se han jugado con el
> mod. Los sonidos de combate y las partes más nuevas (sonido continuo de la bola, ayuda para atrapar,
> filtros de sonido) todavía no se han ajustado con jugadores. Por favor, cuenta qué funciona y qué no
> (mira [Informar de un problema](#informar-de-un-problema)).

## Contenido

- [Qué necesitas](#qué-necesitas)
- [Instalación](#instalación)
- [Quitar el mod](#quitar-el-mod)
- [Primeros pasos](#primeros-pasos)
- [Teclas en todas partes](#teclas-en-todas-partes)
- [Menús](#menús)
- [La base (Nueva Bolabilonia)](#la-base-nueva-bolabilonia)
- [Combate](#combate)
- [Sonidos de combate](#sonidos-de-combate)
- [Filtros de sonido (teclas de número)](#filtros-de-sonido-teclas-de-número)
- [Ayudas](#ayudas)
- [Ajustes del mod (F4)](#ajustes-del-mod-f4)
- [Cambiar las teclas](#changing-keys)
- [Mando](#controller)
- [Consejos para la primera partida](#consejos-para-la-primera-partida)
- [Límites conocidos](#límites-conocidos)
- [Informar de un problema](#informar-de-un-problema)
- [Créditos](#créditos)

## Qué necesitas

- BALL x PIT en Windows (Steam), versión 1.308.
- Un lector de pantalla: el probado es NVDA. Los demás que admite la biblioteca de voz Prism (JAWS,
  Narrador, voces SAPI) también deberían funcionar.
- **MelonLoader 0.7.3** (x64), el cargador de mods. Gratuito, en su página oficial de versiones en GitHub.
- Auriculares o altavoces estéreo: izquierda y derecha importan mucho en combate.

## Instalación

**Con el instalador (lo más fácil):** descarga `BallPitAccessibilityInstaller.exe` de la
[última versión](https://github.com/gomods1/BallPitAccess/releases/latest) y ejecútalo. Encuentra el juego a través de Steam, instala MelonLoader 0.7.3, la
biblioteca de voz Prism y el mod, y más adelante los actualiza o los quita. Después inicia el juego: el
primer inicio tarda unos minutos y es silencioso mientras MelonLoader prepara los archivos del juego;
luego tu lector de pantalla empieza a hablar.

**A mano:** descarga `BallPitAccess-vX.Y.Z.zip` de la misma página y extrae todo en la carpeta del juego
(por defecto `C:\Program Files (x86)\Steam\steamapps\common\BALLxPIT`, la carpeta que contiene `Balls.exe`), de modo que `version.dll` quede junto a
`Balls.exe`. El zip ya incluye MelonLoader: no hace falta instalarlo aparte. Inicia el juego; el primer
inicio es lento y silencioso, como antes.

## Quitar el mod

El botón Desinstalar del instalador quita el mod y MelonLoader y conserva tus ajustes en `UserData`.
A mano:

Borra `Mods\BallPitAccess.dll` y `prism.dll`. Para quitar también MelonLoader, borra `version.dll` y las
carpetas `MelonLoader`, `Mods`, `Plugins` y `UserData` de la carpeta del juego. El mod no cambia ningún
archivo del juego.

## Primeros pasos

- Pulsa **F1** cuando quieras para la ayuda: todas sus partes, empezando por la del sitio donde estás.
  Las flechas arriba y abajo la leen línea a línea, Tab salta a la parte siguiente, Escape la cierra.
- Pulsa **F4** para los ajustes del mod y, dentro, abre la **guía de sonidos**: reproduce cada sonido de
  combate y dice qué significa. Escucharla una vez antes de la primera partida ayuda mucho.
- Con teclado, el juego se maneja con el ratón; con el mod no necesitas el ratón nunca, salvo para el
  lanzador inactivo de la base (mira [Límites conocidos](#límites-conocidos)).

## Teclas en todas partes

| Tecla | Acción |
|---|---|
| F1 | Ayuda (todas las partes; Tab / Mayús+Tab entre partes) |
| F2 | Leer toda la pantalla |
| F3 | Repetir lo último que se dijo |
| F4 | Ajustes del mod y guía de sonidos |
| F5 | Cambiar el modo de la ayuda de apuntado |
| F6 / F7 | Girar la puntería 10 grados a la izquierda / derecha |
| F8 | Decir hacia dónde apuntas |
| Ctrl+Arriba | Leer los detalles del elemento enfocado (Ctrl+Abajo vuelve) |
| Ctrl+Izquierda / Ctrl+Derecha | Cambiar entre detalles y sucesos recientes |

Mientras hay una lista del mod abierta (ayuda, ajustes, guía de sonidos), el juego no recibe tus teclas.

## Menús

Cada pantalla es **una sola lista en un orden lógico**: primero sus opciones, luego sus botones, luego
Cerrar. El orden nunca depende de dónde se dibujan las cosas en la pantalla.

| Tecla | Acción |
|---|---|
| Arriba / Abajo | Elemento anterior / siguiente |
| Intro | Elegir |
| Tab / Mayús+Tab | Grupo siguiente / anterior de la pantalla |
| Inicio / Fin | Primer / último elemento del grupo |
| Izquierda / Derecha | Cambiar un ajuste de lista o deslizador; en un nivel, cambiar su dificultad |
| Ctrl+Tab / Ctrl+Mayús+Tab | Pestaña siguiente / anterior en las pantallas con pestañas |
| Retroceso o Escape | Volver |
| Re Pág / Av Pág | Conjunto en la enciclopedia; nivel de nueva partida+ en la lista de niveles |
| Mayús+Intro | En un nivel: abrir su clasificación |

- Intro en un personaje te lleva a su botón Seleccionar.
- Cuando el tutorial guiado señala algo, el foco salta allí: pulsa Intro.
- Las mejoras se leen con sus números ("Inflige de 10 a 15 de daño").

## La base (Nueva Bolabilonia)

| Tecla | Acción |
|---|---|
| Arriba / Abajo, Intro | Botones de la barra lateral: Jugar, Recolectar, Construir, Expandir, Reorganizar, Opciones |
| Tab / Mayús+Tab | Recorrer el ascensor y tus edificios |
| Intro en un edificio | Empezar una recolección apuntada a él si está listo; si no, abrir su mejora |
| Mayús+Intro | Abrir la mejora de un edificio |
| Intro en el ascensor | Abrir la mejora del ascensor (foso abajo, o arriba hacia Bolabilonia Resurgida) |
| Suprimir | Desmontar un edificio |
| H | Recursos y día |
| J | Recolección y trabajadores |
| K | Explorar la base casilla a casilla |
| I | Al construir o mover un edificio: a qué estorba este sitio |

- **Construir**: las flechas mueven el edificio una casilla y dicen si cabe, Intro construye, R gira,
  Inicio salta a un sitio libre, Retroceso cancela.
- **Recolectar**: empieza apuntando a un edificio. Intro lanza a los trabajadores, Tab apunta a otro
  edificio, Izquierda / Derecha giran la puntería, mantén Espacio para acelerar, Escape sale.
- **Construcción**: los edificios en construcción o mejorándose también están en la lista Tab de la recolección. Cada trabajador que rebota en uno lo construye; el mod avisa cuando está terminado. Intro sobre uno en la base empieza la recolección apuntada a él.
- **¿Llegarán los trabajadores?** Cuando la recolección apunta a un edificio (Tab, o Intro sobre él en la base), el mod sigue el camino de los trabajadores igual que los mueve el juego y elige un ángulo que llega: "los trabajadores le dan directamente", "los trabajadores llegan rebotando en una pared", "los trabajadores pasan por encima" (campos de trigo), o "no hay camino libre, Bosque está en medio". A un edificio sin camino libre solo se llega por suerte: muévelo, o mueve el que estorba. Izquierda / Derecha dicen en qué da la línea.
- **Después de una recolección** el mod dice qué recibió cada edificio en construcción ("Casa encantada: 6 golpes, del 40 al 55 por ciento"). El resumen del juego solo cuenta recursos: dice "Un esfuerzo infructuoso" siempre que no se recolectó nada, aunque los trabajadores hayan construido.
- **Expandir**: Tab elige una parcela, Intro la compra.
- **Reorganizar**: Tab elige un edificio, Intro lo coge, las flechas lo mueven, Intro lo suelta.
- **Explorar** (K): las flechas avanzan una casilla y dicen qué hay: libre (y cuántas casillas libres siguen a la derecha y hacia abajo), un edificio con su tamaño, o terreno sin comprar. Tab / Mayúsculas+Tab saltan a la casilla libre siguiente / anterior. K, Retroceso o Escape salen. Las columnas y filas son las mismas que al colocar un edificio.
- El menú de construcción dice cuánto espacio ocupa cada edificio ("3 de ancho, 2 de alto"), y al colocarlo se dice su nombre y su tamaño.
- **Construir o mover un edificio**: la posición dice qué la bloquea ("bloqueado por Bosque"). I dice si los trabajadores llegan al sitio y a qué edificios dejaría sin paso o solo alcanzables rebotando en una pared. Inicio busca el mejor sitio libre, uno al que los trabajadores le dan de frente y que no deja sin paso a nadie, y gira el edificio para que quepa. Tras R se dice la nueva forma: algunos edificios, como la casa encantada, no son rectángulos ("esquina abierta arriba a la izquierda"). Al reorganizar también se pueden girar.
- **Explorar**: Av Pág / Re Pág saltan al edificio siguiente / anterior y dicen dónde está, su tamaño y su forma.

## Combate

### Teclas del propio juego

| Tecla | Acción |
|---|---|
| W A S D | Moverse |
| Espacio | Disparar (mantener) |
| F | Disparo automático sí / no |
| E | Abrir una subida de nivel cuando hay una lista |
| = / - | Más rápido / más lento (no existen en algunas distribuciones de teclado: usa Num + / Num - del mod, abajo) |
| Escape | Pausa |

### Teclas del mod

| Tecla | Acción |
|---|---|
| Flecha izquierda / derecha | Girar la puntería 5 grados a mano |
| Flecha arriba | Devolver la puntería a la ayuda |
| Flecha abajo | Decir hacia dónde apuntas |
| Mayús (mantener) | Ayuda para atrapar (mira [Ayudas](#ayudas)) |
| 1 a 7, 0 | Filtros de sonido (mira [Filtros de sonido](#filtros-de-sonido-teclas-de-número)) |
| H | Salud y estados alterados |
| U | Estados con los segundos que les quedan, y si puedes moverte, y si no, por qué (congelado, empujado por un golpe, enemigos avanzando). Cuando se acaba una congelación el mod dice "Ya puedes moverte" |
| J | Enemigos y jefe |
| K | Tu posición |
| G | Velocidad del juego |
| L | Nivel, tiempo, oleada, engranajes ganados |
| B | Tus bolas y pasivas |
| P | Objetos en el campo |
| N | Peligros (disparos, láseres, bombas, suelo) |
| Q | ¿Qué fue ese sonido? Los sonidos que suenan ahora y los últimos, con lo que significa cada uno y su lado |
| Num + / Num - | Juego más rápido / más lento, con cualquier distribución de teclado |

Todas las teclas del mod se pueden cambiar: mira [Cambiar las teclas](#changing-keys).

Se anuncia durante el combate: el nivel al empezar, los jefes y su salud al 75, 50 y 25 por ciento, el
daño y la salud, los estados alterados, los objetos especiales cuando caen, "subidas de nivel listas,
pulsa E" y el final de la partida.

**Jefes**: J da la salud real del jefe, la misma que su barra en el juego. Algunos jefes solo reciben daño en un punto débil: al Rey Esqueleto no se le puede herir el cuerpo, solo la corona de la cabeza, y su cuerpo la protege por delante, así que las bolas llegan rebotando en las paredes. J dice dónde está el punto débil, y la ayuda de puntería va a por él.

## Sonidos de combate

Tres reglas valen para todos los sonidos, siempre respecto a tu héroe:

- **Lado**: un sonido viene de donde está aquello de lo que avisa: a tu izquierda suena a la izquierda, a
  tu derecha suena a la derecha, y en el centro si está justo delante.
- **Tono**: más agudo es más cerca.
- **Apagado** significa que está detrás de ti.

Lo que se mueve todo el tiempo (tu bola, los disparos enemigos) tiene un **sonido continuo** que lo sigue;
lo que cambia a saltos (enemigos que avanzan, objetos en el suelo) tiene sonidos cortos.

| Sonido | Significado |
|---|---|
| Tambor de mano | Un enemigo. Más agudo y rápido = más cerca. Cuando los enemigos se mueven suenan por turnos los más cercanos; mientras nada cambia, solo el más cercano. |
| Tambor grande y grave | Un jefe. Tambores, gong y trompa cuando llega uno. |
| Gruñido | Un enemigo está a punto de alcanzarte. |
| Zumbido que sube | Un enemigo prepara un ataque, a su lado. |
| **Zumbido rodante** (continuo) | La bola que puedes atrapar. Suena desde **donde está la bola** y **sube de tono cuanto más se acerca a ti**, así que oyes hacia dónde va; late más rápido cuando la bola se acerca a tu línea y suena más suave mientras todavía puede rebotar en un enemigo. |
| Clic | Estás donde va a caer la bola: quédate ahí. |
| Golpe en el guante | Bola atrapada. |
| Golpe sordo que se hunde | Una bola se te escapó. |
| Carga de arma | Tus bolas han vuelto: puedes disparar (no suena con disparo automático). |
| Tintineo de cristal | El cristal de experiencia, oro o recurso al que ir: el más cercano o un montón de ellos un poco más lejos. No se recogen solos. |
| Destello de kalimba | Un objeto especial: fusionador, plano, huevo, engranaje, salud, imán, bomba, bolsa de gemas. |
| **Silbido de aire** (continuo) | Un disparo enemigo cerca de ti, más agudo cuanto más se acerca. Más fuerte si te va a dar. |
| Silbido y chasquido | Un disparo te va a dar; suena desde el disparo: apártate de su camino. |
| Zumbido eléctrico / dos descargas | Un láser apunta / estás en su línea; los dos suenan desde el láser: sal de la línea. |
| Silbido que cae / mecha | Una bomba cae cerca / caerá sobre ti; los dos suenan desde donde caerá: aléjate de ahí. |
| Chapoteo | Pisaste un peligro del suelo (se dice su nombre). |
| Puñetazo / trago | Daño / curación. |
| Latido | Poca salud; más rápido con muy poca. |
| Ondulación mareante | Te ha caído un estado alterado. |
| Dos notas de campana que suben | Hay una subida de nivel lista (pulsa E). |
| Golpe de madera | Borde lateral de la arena (también el final de una lista de menú). |
| Golpe metálico | Límite delantero o trasero de tu zona. |
| Clic de interruptor | La ayuda de apuntado cambió a otro objetivo, al lado del nuevo. Con el disparo automático, no después de una baja. |

Todos se pueden escuchar en **F4 → Guía de sonidos**, con Izquierda / Derecha para oírlos en cada lado.
En combate, **Q** dice qué sonidos acabas de oír.

## Filtros de sonido (teclas de número)

En combate, las teclas de número silencian o activan un grupo de sonidos para que te concentres. El juego
no usa ninguna de estas teclas.

| Tecla | Grupo |
|---|---|
| 1 | Enemigos: tambores, gruñido, preparación de ataque, llegada del jefe |
| 2 | Bolas: zumbido rodante, clic, atrapada, perdida, carga de arma |
| 3 | Recompensas: cristales, oro, recursos, objetos especiales |
| 4 | Disparos y peligros |
| 5 | Bordes de la arena |
| 6 | Tu salud: daño, curación, latido, estados |
| 7 | Clic de la ayuda de apuntado |
| 0 | Todo activado otra vez |

- Cada número cambia su propio grupo, sin tocar los demás.
- **Ctrl+número** deja solo ese grupo. Ejemplo: solo enemigos y recompensas = Ctrl+1 y luego 3.
- Lo que elijas se guarda para la próxima vez. Cuando empieza un combate con algo silenciado, el mod dice
  qué está silenciado.
- Silenciar solo cambia los sonidos: las teclas H, J, P, N siguen respondiendo y la ayuda para atrapar
  sigue esquivando.

## Ayudas

Todo lo que hace una ayuda también se puede hacer de oído; las ayudas están para cuando las quieras.

- **Ayuda de apuntado** (F5 cambia el modo): apunta por ti al enemigo más cercano (por defecto), al más
  fuerte, al que tienes delante, a un ángulo fijo, o desactivada (ratón). Las flechas la ajustan. Apunta a una parte del enemigo que ningún otro tapa, salta los enemigos en un ángulo al que tu héroe no puede disparar y, en el modo por defecto, va primero a por el jefe salvo que otro enemigo esté a punto de alcanzarte.
- **Ayuda para atrapar** (mantén Mayús): el héroe va debajo de la próxima bola que baja; si no viene
  ninguna, va al cristal u objeto más cercano; si un disparo está a punto de darte, primero se aparta.
  W / S siguen moviéndote hacia delante y atrás, A / D siempre mandan. En F4 puede estar desactivada, en
  "mantener Mayúsculas" (por defecto) o automática. Si estás herido, primero va a por un objeto de salud.

## Ajustes del mod (F4)

Arriba / Abajo para moverse, Izquierda / Derecha o Intro para cambiar, Escape para cerrar. Los ajustes se
guardan.

- Modo de la ayuda de apuntado
- Ayuda para atrapar: desactivada, mantener Mayúsculas, automática
- Sonidos de ayuda sí / no, volumen de los sonidos
- Radar de enemigos, sonidos de bolas, bordes de la arena, disparos y peligros (sí / no)
- Sonidos de objetos: desactivados, solo objetos especiales, todos
- Sonidos de salud, clic de la ayuda de apuntado (sí / no)
- Leer la descripción de las mejoras al enfocarlas
- Mando
- Teclas (Intro las abre): mira [Cambiar las teclas](#changing-keys)
- Guía de sonidos (Intro la abre)
- Registro de depuración
- Grabar batallas (desactivado por defecto): cada batalla se guarda en `UserData\BallPitAccess\telemetry.tsv`, para ajustar los sonidos

## Cambiar las teclas {#changing-keys}

**F4 → Teclas** lista todas las teclas del mod con la tecla que tienen ahora. Arriba / Abajo para elegir,
**Intro** y luego pulsa la nueva tecla (Escape cancela), **Suprimir** devuelve la tecla de fábrica, y la
última línea las restaura todas. Si la nueva tecla ya era de otra acción del mod, las dos se intercambian y
el mod dice adónde fue la otra. Las teclas que necesitan los menús o el juego (flechas, Intro, Tab, Escape,
Retroceso, Espacio, W A S D F E R X) no se pueden usar. La ayuda de F1 termina con las teclas tal como
están ahora.

## Mando {#controller}

- Botones de Xbox; un mando de PlayStation o de Switch funciona con Steam Input activado para el juego
- Fuera de una batalla la cruceta o el stick izquierdo se mueven como las flechas, A es Intro y B vuelve atrás
- X e Y saltan al primer y al último elemento; al colocar un edificio X busca un sitio donde cabe e Y lo gira
- RB y LB son Tab y Mayúsculas Tab, RT y LT cambian las pestañas de la pantalla
- Start es la pausa del juego
- En una batalla el juego tiene los sticks, los gatillos y los botones
- Cruceta arriba dice los enemigos, abajo tu salud, izquierda tus estados, derecha tu progreso
- Mantén pulsado el stick izquierdo para atrapar, como Mayúsculas
- Si no tocas el stick derecho, la asistencia de puntería apunta por ti; muévelo para apuntar tú
- Mantén View para más: A ayuda, B ajustes del mod, X lee la pantalla, Y repite, Start cambia el modo de puntería
- View con LB y RB lee líneas más antiguas y más recientes de la lista de repaso, View con un clic de stick cambia entre detalles y eventos
- En una batalla View con la cruceta: arriba peligros, abajo equipo, izquierda objetos, derecha el último sonido; View con LT y RT baja y sube la velocidad del juego
- En la base View con la cruceta: arriba recolección, abajo recursos, izquierda modo explorar
- En los menús View con LT y RT es Re Pág y Av Pág

## Consejos para la primera partida

- **Atrapa tus bolas.** Una bola atrapada se puede disparar enseguida; una perdida tarda 2 o 3 segundos
  en volver. Sigue el zumbido rodante hasta oír el clic y quédate quieto.
- **Recoge tus cristales de experiencia.** No se recogen solos. Sigue el tintineo de cristal; apagado
  significa que está detrás de ti.
- **Primero sobrevive.** Al principio de una partida, seguir vivo importa más que el daño.
- **Fusiona y evoluciona** tus bolas cuando la subida de nivel lo ofrezca: las opciones dicen "fusión" o
  "evolución".
- **Ve más despacio con los jefes** con Num - (o la tecla menos del juego). El mod te lo recuerda cuando llega un jefe a
  velocidad alta.
- Morir es normal: el oro que traes construye Nueva Bolabilonia, y los edificios hacen más fuerte cada
  partida siguiente.

## Límites conocidos

- El lanzador inactivo de la base todavía necesita el ratón.
- Las clasificaciones y la votación de Twitch solo se leen de forma básica. Los créditos no se pueden
  saltar.
- Los ajustes del mod están en F4, no dentro de la pantalla de opciones del juego.
- Los textos propios del mod en idiomas distintos del inglés y el español se tradujeron sin revisión de
  un hablante nativo todavía. Se agradecen las correcciones. Los nombres del juego (edificios, bolas,
  enemigos, mejoras) siempre vienen de la traducción del propio juego.
- Los volúmenes, ritmos y el ancho para atrapar todavía no se han ajustado con jugadores.

## Informar de un problema

Archivos útiles, en la carpeta del juego:

- `UserData\BallPitAccess\speech.log`: todo lo que dijo el mod.
- `MelonLoader\Latest.log`: el registro del mod (errores, pantallas, bolas atrapadas y perdidas).
- En combate, **Ctrl+Mayús+D** escribe `UserData\BallPitAccess\threats.txt` (enemigos, bolas, disparos,
  objetos con sus posiciones) y la lista de la pantalla abierta en `Latest.log`: útil cuando un sonido
  señala mal o un menú se lee en mal orden.
- `UserData\BallPitAccess\telemetry.tsv`, si activaste F4 → Grabar batallas: cada sonido, enemigo, bola y
  disparo de tus batallas. La mejor ayuda cuando los sonidos cuestan de seguir.

Cuenta qué hiciste, qué oíste y qué esperabas.

## Créditos

- BALL x PIT es de Kenny Sun, publicado por Devolver Digital. Este mod no es oficial.
- MelonLoader (LavaGang) carga el mod. Prism envía la voz a tu lector de pantalla.
