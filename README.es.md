# BALL x PIT Access

Un mod que hace **BALL x PIT** (Kenny Sun / Devolver Digital) jugable sin ver. Lee todos los menús con tu
lector de pantalla, te deja usar todo el juego con el teclado y convierte el campo de batalla en sonido:
los enemigos, tus bolas, los disparos enemigos, los cristales de experiencia y los objetos especiales
tienen cada uno su sonido, que viene de su lado respecto a ti.

Versión 0.5.0, para la versión 1.308 del juego, incluido el DLC Bolabilonia Resurgida. El mod habla
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

- **Construir**: las flechas mueven el edificio una casilla y dicen si cabe, Intro construye, R gira,
  Inicio salta a un sitio libre, Retroceso cancela.
- **Recolectar**: empieza apuntando a un edificio. Intro lanza a los trabajadores, Tab apunta a otro
  edificio, Izquierda / Derecha giran la puntería, mantén Espacio para acelerar, Escape sale.
- **Expandir**: Tab elige una parcela, Intro la compra.
- **Reorganizar**: Tab elige un edificio, Intro lo coge, las flechas lo mueven, Intro lo suelta.

## Combate

### Teclas del propio juego

| Tecla | Acción |
|---|---|
| W A S D | Moverse |
| Espacio | Disparar (mantener) |
| F | Disparo automático sí / no |
| E | Abrir una subida de nivel cuando hay una lista |
| = / - | Más rápido / más lento |
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
| J | Enemigos y jefe |
| K | Tu posición |
| G | Velocidad del juego |
| L | Nivel, tiempo, oleada, engranajes ganados |
| B | Tus bolas y pasivas |
| P | Objetos en el campo |
| N | Peligros (disparos, láseres, bombas, suelo) |

Se anuncia durante el combate: el nivel al empezar, los jefes y su salud al 75, 50 y 25 por ciento, el
daño y la salud, los estados alterados, los objetos especiales cuando caen, "subidas de nivel listas,
pulsa E" y el final de la partida.

## Sonidos de combate

Cuatro reglas valen para todos los sonidos:

- **Lado**: un sonido viene del lado de aquello de lo que avisa.
- **Tono**: más agudo es más cerca.
- **Apagado** significa que está detrás de ti.
- Un sonido que te pide **moverte** viene del lado hacia el que ir.

Lo que se mueve todo el tiempo (tu bola, los disparos enemigos) tiene un **sonido continuo** que lo sigue;
lo que cambia a saltos (enemigos que avanzan, objetos en el suelo) tiene sonidos cortos.

| Sonido | Significado |
|---|---|
| Tambor de mano | Un enemigo. Más agudo y rápido = más cerca. Suenan por turnos los tres más cercanos. |
| Tambor grande y grave | Un jefe. Tambores, gong y trompa cuando llega uno. |
| Gruñido | Un enemigo está a punto de alcanzarte. |
| Zumbido que sube | Un enemigo prepara un ataque, a su lado. |
| **Zumbido rodante** (continuo) | La bola que puedes atrapar. Camina hacia su lado; **sube de tono al acercarte a donde caerá la bola**, late más rápido cuando la bola se acerca y suena más suave mientras todavía puede rebotar en un enemigo. |
| Clic y el zumbido sube un escalón | Estás debajo de la bola: quédate ahí. |
| Golpe en el guante | Bola atrapada. |
| Golpe sordo que se hunde | Una bola se te escapó. |
| Carga de arma | Tus bolas han vuelto: puedes disparar (no suena con disparo automático). |
| Tintineo de cristal | El cristal de experiencia, oro o recurso más cercano al que puedes llegar. No se recogen solos. |
| Destello de kalimba | Un objeto especial: fusionador, plano, huevo, engranaje, salud, imán, bomba, bolsa de gemas. |
| **Silbido de aire** (continuo) | Un disparo enemigo cerca de ti, más agudo cuanto más se acerca. Más fuerte si te va a dar. |
| Silbido y chasquido | Un disparo te va a dar: muévete hacia el lado donde suena. |
| Zumbido eléctrico / dos descargas | Un láser apunta / estás en su línea: muévete hacia el lado donde suena. |
| Silbido que cae / mecha | Una bomba cae cerca / caerá sobre ti: muévete hacia el lado donde suena. |
| Chapoteo | Pisaste un peligro del suelo (se dice su nombre). |
| Puñetazo / trago | Daño / curación. |
| Latido | Poca salud; más rápido con muy poca. |
| Ondulación mareante | Te ha caído un estado alterado. |
| Dos notas de campana que suben | Hay una subida de nivel lista (pulsa E). |
| Golpe de madera | Borde lateral de la arena (también el final de una lista de menú). |
| Golpe metálico | Límite delantero o trasero de tu zona. |
| Clic de interruptor | La ayuda de apuntado cambió de objetivo, al lado del nuevo. |

Todos se pueden escuchar en **F4 → Guía de sonidos**, con Izquierda / Derecha para oírlos en cada lado.

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
  fuerte, al que tienes delante, a un ángulo fijo, o desactivada (ratón). Las flechas la ajustan.
- **Ayuda para atrapar** (mantén Mayús): el héroe va debajo de la próxima bola que baja; si no viene
  ninguna, va al cristal u objeto más cercano; si un disparo está a punto de darte, primero se aparta.
  W / S siguen moviéndote hacia delante y atrás, A / D siempre mandan. En F4 puede estar desactivada, en
  "mantener Mayúsculas" (por defecto) o automática.

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
- Guía de sonidos (Intro la abre)
- Registro de depuración

## Consejos para la primera partida

- **Atrapa tus bolas.** Una bola atrapada se puede disparar enseguida; una perdida tarda 2 o 3 segundos
  en volver. Sigue el zumbido rodante hasta oír el clic y quédate quieto.
- **Recoge tus cristales de experiencia.** No se recogen solos. Sigue el tintineo de cristal; apagado
  significa que está detrás de ti.
- **Primero sobrevive.** Al principio de una partida, seguir vivo importa más que el daño.
- **Fusiona y evoluciona** tus bolas cuando la subida de nivel lo ofrezca: las opciones dicen "fusión" o
  "evolución".
- **Ve más despacio con los jefes** con la tecla menos. El mod te lo recuerda cuando llega un jefe a
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

Cuenta qué hiciste, qué oíste y qué esperabas.

## Créditos

- BALL x PIT es de Kenny Sun, publicado por Devolver Digital. Este mod no es oficial.
- MelonLoader (LavaGang) carga el mod. Prism envía la voz a tu lector de pantalla.
