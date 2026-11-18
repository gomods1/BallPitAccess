# BALL x PIT Access

Un mod qui rend **BALL x PIT** (Kenny Sun / Devolver Digital) jouable sans la vue. Il lit tous les menus
avec votre lecteur d'écran, vous permet d'utiliser tout le jeu au clavier et transforme le champ de
bataille en sons : les ennemis, vos boules, les tirs ennemis, les cristaux d'EXP et les objets spéciaux
ont chacun leur son, qui vient du côté où ils se trouvent par rapport à vous.

Version 0.6.2, pour la version 1.308 du jeu, DLC Baboulone émergée compris. Le mod parle **toutes les
langues proposées par le jeu** et suit la langue choisie dans le jeu.

<!-- LANGS -->
[English](README.html) · [Español](README.es.html) · **Français** · [Deutsch](README.de.html) · [Italiano](README.it.html) · [Português (Brasil)](README.pt-BR.html) · [Polski](README.pl.html) · [Türkçe](README.tr.html) · [Русский](README.ru.html) · [Українська](README.uk.html) · [日本語](README.ja.html) · [한국어](README.ko.html) · [简体中文](README.zh-CN.html) · [繁體中文](README.zh-TW.html) · [ไทย](README.th.html)
<!-- /LANGS -->

> **État : première version de test.** Les menus, la base et les premières expéditions ont été joués avec
> le mod. Les sons de combat et les parties les plus récentes (son continu de la boule, aide au rattrapage,
> filtres de son) n'ont pas encore été réglés avec des joueurs. Dites-nous ce qui marche et ce qui ne
> marche pas (voir [Signaler un problème](#reporting-a-problem)).

## Sommaire {#contents}

- [Ce qu'il vous faut](#what-you-need)
- [Installation](#installing)
- [Désinstaller le mod](#removing-the-mod)
- [Premiers pas](#first-steps)
- [Touches valables partout](#keys-everywhere)
- [Menus](#menus)
- [La base (Nouvelle-Baboulone)](#the-base-new-ballbylon)
- [Combat](#battle)
- [Sons de combat](#battle-sounds)
- [Filtres de son (touches numériques)](#sound-filters-number-keys)
- [Aides](#assists)
- [Paramètres du mod (F4)](#mod-settings-f4)
- [Changer les touches](#changing-keys)
- [Conseils pour une première expédition](#tips-for-a-first-run)
- [Limites connues](#known-limits)
- [Signaler un problème](#reporting-a-problem)
- [Crédits](#credits)

## Ce qu'il vous faut {#what-you-need}

- BALL x PIT sous Windows (Steam), version 1.308.
- Un lecteur d'écran : NVDA est celui qui a été testé. Les autres pris en charge par la bibliothèque de
  synthèse vocale Prism (JAWS, Narrateur, voix SAPI) devraient aussi fonctionner.
- **MelonLoader 0.7.3** (x64), le chargeur de mods. Gratuit, sur sa page officielle de versions GitHub.
- Un casque ou des enceintes stéréo : la gauche et la droite comptent beaucoup en combat.

## Installation {#installing}

**Avec l'installateur (le plus simple) :** téléchargez `BallPitAccessibilityInstaller.exe` depuis la
[dernière version](https://github.com/gomods1/BallPitAccess/releases/latest) et lancez-le. Il trouve le jeu via Steam, installe MelonLoader 0.7.3, la
bibliothèque vocale Prism et le mod, puis les met à jour ou les retire plus tard. Lancez ensuite le jeu :
le premier lancement prend quelques minutes et reste silencieux pendant que MelonLoader prépare les
fichiers du jeu, puis votre lecteur d'écran se met à parler.

**À la main :** téléchargez `BallPitAccess-vX.Y.Z.zip` sur la même page et extrayez tout dans le dossier
du jeu (par défaut `C:\Program Files (x86)\Steam\steamapps\common\BALLxPIT`, le dossier qui contient `Balls.exe`), de sorte que `version.dll` se retrouve à
côté de `Balls.exe`. Le zip contient déjà MelonLoader : inutile de l'installer à part. Lancez le jeu ; le
premier lancement est lent et silencieux, comme ci-dessus.

## Désinstaller le mod {#removing-the-mod}

Le bouton Désinstaller de l'installateur retire le mod et MelonLoader et garde vos réglages dans
`UserData`. À la main :

Supprimez `Mods\BallPitAccess.dll` et `prism.dll`. Pour enlever aussi MelonLoader, supprimez `version.dll`
et les dossiers `MelonLoader`, `Mods`, `Plugins` et `UserData` du dossier du jeu. Le mod ne modifie aucun
fichier du jeu.

## Premiers pas {#first-steps}

- Appuyez sur **F1** à tout moment pour l'aide : toutes ses parties, en commençant par celle qui
  correspond à l'endroit où vous êtes. Les flèches haut et bas la lisent ligne par ligne, Tab passe à la
  partie suivante, Échap ferme.
- Appuyez sur **F4** pour les paramètres du mod, et ouvrez-y le **guide des sons** : il joue chaque son
  de combat et dit ce qu'il signifie. L'écouter une fois avant votre première expédition aide beaucoup.
- Au clavier, le jeu lui-même se joue à la souris ; avec le mod, vous n'avez jamais besoin de la souris,
  sauf pour le lanceur automatique de la base (voir [Limites connues](#known-limits)).

## Touches valables partout {#keys-everywhere}

| Touche | Action |
|---|---|
| F1 | Aide (toutes les parties ; Tab / Maj+Tab pour passer d'une partie à l'autre) |
| F2 | Lire tout l'écran |
| F3 | Répéter la dernière annonce |
| F4 | Paramètres du mod et guide des sons |
| F5 | Changer le mode de l'aide à la visée |
| F6 / F7 | Tourner la visée de 10 degrés à gauche / à droite |
| F8 | Dire où vous visez |
| Ctrl+flèche haut | Lire les détails de l'élément sélectionné (Ctrl+flèche bas revient en arrière) |
| Ctrl+flèche gauche / Ctrl+flèche droite | Passer des détails aux événements récents et inversement |

Tant qu'une liste du mod (aide, paramètres, guide des sons) est ouverte, le jeu ne reçoit pas vos touches.

## Menus {#menus}

Chaque écran est **une seule liste dans un ordre logique** : d'abord ses choix, puis ses boutons, puis
Fermer. L'ordre ne dépend jamais de l'endroit où les éléments sont dessinés à l'écran.

| Touche | Action |
|---|---|
| Flèche haut / flèche bas | Élément précédent / suivant |
| Entrée | Choisir |
| Tab / Maj+Tab | Groupe suivant / précédent de l'écran |
| Début / Fin | Premier / dernier élément du groupe |
| Flèche gauche / flèche droite | Changer un réglage de type liste ou curseur ; sur un niveau, changer sa difficulté |
| Ctrl+Tab / Ctrl+Maj+Tab | Onglet suivant / précédent sur les écrans à onglets |
| Retour arrière ou Échap | Retour |
| Page précédente / Page suivante | Série dans l'encyclopédie ; palier de Nouvelle partie plus dans la liste des niveaux |
| Maj+Entrée | Sur un niveau : ouvrir son classement |

- Entrée sur un personnage vous place sur son bouton Sélectionner.
- Quand le tutoriel guidé montre quelque chose, le focus y saute : appuyez sur Entrée.
- Les choix d'amélioration sont lus avec leurs chiffres (« Inflige de 10 à 15 dégâts »).

## La base (Nouvelle-Baboulone) {#the-base-new-ballbylon}

| Touche | Action |
|---|---|
| Flèche haut / flèche bas, Entrée | Boutons de la barre latérale : Jouer, Récolter, Construire, Agrandir, Disposition, Options |
| Tab / Maj+Tab | Parcourir l'ascenseur et vos bâtiments |
| Entrée sur un bâtiment | Lancer une récolte visant ce bâtiment s'il est prêt ; sinon, ouvrir son amélioration |
| Maj+Entrée | Ouvrir l'amélioration d'un bâtiment |
| Entrée sur l'ascenseur | Ouvrir l'amélioration de l'ascenseur (vers le fond de la fosse, ou vers Baboulone émergée) |
| Suppr | Démonter un bâtiment |
| H | Ressources et jour |
| J | Récolte et ouvriers |
| K | Explorer la base case par case |

- **Construction** : les flèches déplacent le bâtiment d'une case et disent s'il rentre, Entrée
  construit, R fait pivoter, Début va à un emplacement libre, Retour arrière annule.
- **Récolte** : elle commence en visant un bâtiment. Entrée lance les ouvriers, Tab vise un autre
  bâtiment, flèche gauche / flèche droite tournent la visée, maintenez Espace pour accélérer, Échap quitte.
- **Extension** : Tab choisit une parcelle, Entrée l'achète.
- **Disposition** : Tab choisit un bâtiment, Entrée le soulève, les flèches le déplacent, Entrée le pose.
- **Explorer** (K) : les flèches avancent d'une case et disent ce qu'il y a : libre (et combien de cases libres suivent vers la droite et vers le bas), un bâtiment avec sa taille, ou un terrain non acheté. Tab / Maj+Tab sautent à la case libre suivante / précédente. K, Retour arrière ou Échap quittent. Les colonnes et rangées sont les mêmes que pour placer un bâtiment.
- Le menu de construction dit la place que prend chaque bâtiment (« 3 de large, 2 de haut »), et le placement commence par son nom et sa taille.

## Combat {#battle}

### Les touches du jeu {#the-games-own-keys}

| Touche | Action |
|---|---|
| W A S D | Se déplacer |
| Espace | Tirer (maintenir) |
| F | Tir auto activé / désactivé |
| E | Ouvrir un gain de niveau quand il y en a un de prêt |
| = / - | Vitesse du jeu plus rapide / plus lente (absentes de certaines dispositions de clavier : utilisez Num + / Num - du mod, plus bas) |
| Échap | Pause |

### Les touches du mod {#mod-keys}

| Touche | Action |
|---|---|
| Flèche gauche / flèche droite | Tourner la visée de 5 degrés à la main |
| Flèche haut | Rendre la visée à l'aide |
| Flèche bas | Dire où vous visez |
| Maj (maintenir) | Aide au rattrapage (voir [Aides](#assists)) |
| 1 à 7, 0 | Filtres de son (voir [Filtres de son](#sound-filters-number-keys)) |
| H | Vie et effets d'état |
| J | Ennemis et boss |
| K | Votre position |
| G | Vitesse du jeu |
| L | Niveau, temps, vague, rouages gagnés |
| B | Vos boules et passifs |
| P | Objets sur le terrain |
| N | Dangers (tirs, lasers, bombes, sol) |
| Q | Quel était ce son ? Les sons qui jouent maintenant et les derniers, avec le sens de chacun et son côté |
| Num + / Num - | Vitesse du jeu plus rapide / plus lente, quelle que soit la disposition du clavier |

Chaque touche du mod peut être changée : voir [Changer les touches](#changing-keys).

Annoncé pendant le combat : le niveau au début, les boss et leur vie à 75, 50 et 25 pour cent, les dégâts
et les soins, les effets d'état, les objets spéciaux quand ils tombent, « gains de niveau prêts, appuyez
sur E », la fin de l'expédition.

## Sons de combat {#battle-sounds}

Trois règles valent pour tous les sons, toujours par rapport à votre personnage :

- **Côté** : un son vient de l'endroit où se trouve ce qu'il annonce : à votre gauche il sonne à gauche,
  à votre droite il sonne à droite, au centre quand c'est droit devant.
- **Hauteur** : plus aigu veut dire plus proche.
- **Étouffé** veut dire que c'est derrière vous.

Ce qui bouge tout le temps (votre boule, les tirs ennemis) a un **son continu** qui le suit ; ce qui
change par étapes (les ennemis qui avancent, les objets posés au sol) a des sons courts.

| Son | Signification |
|---|---|
| Tambour à main | Un ennemi. Plus aigu et plus rapide = plus proche. Quand les ennemis bougent, les plus proches jouent à tour de rôle ; tant que rien ne change, seul le plus proche joue. |
| Gros tambour grave | Un boss. Tambours, gong et cor quand il arrive. |
| Grognement | Un ennemi est sur le point de vous atteindre. |
| Gémissement montant | Un ennemi prépare une attaque, de son côté. |
| **Bourdonnement roulant** (continu) | La boule que vous pouvez rattraper. Il sonne depuis **l'endroit où se trouve la boule** et **monte à mesure qu'elle se rapproche de vous**, pour que vous entendiez où elle va ; il pulse plus vite quand la boule arrive sur votre ligne, et il est plus doux tant que la boule peut encore rebondir sur un ennemi. |
| Clic | Vous êtes là où la boule va tomber : restez là. |
| Choc dans un gant | Boule rattrapée. |
| Choc sourd qui retombe | Une boule vous a échappé. |
| Bruit de culasse | Vos boules sont revenues : vous pouvez tirer (silencieux avec le tir auto). |
| Tintement de verre | Le cristal d'EXP, l'or ou la ressource vers lequel marcher : le plus proche, ou un tas un peu plus loin. Ils ne se ramassent pas tout seuls. |
| Scintillement de kalimba | Un objet spécial : réacteur nucléaire, plan, œuf, rouage, soin, aimant, bombe, sac de gemmes. |
| **Sifflement aérien** (continu) | Un tir ennemi qui vole près de vous, plus aigu à mesure qu'il approche. Plus fort quand il va vous toucher. |
| Souffle et claquement | Un tir va vous toucher ; le son vient du tir : écartez-vous de sa trajectoire. |
| Bourdonnement électrique / deux décharges | Un laser vise / vous êtes dans son axe ; les deux viennent du laser : sortez de l'axe. |
| Sifflement descendant / grésillement de mèche | Une bombe tombe tout près / elle va tomber sur vous ; les deux viennent de l'endroit où elle tombe : éloignez-vous-en. |
| Bruit visqueux | Vous avez marché sur un danger au sol (son nom est annoncé). |
| Coup de poing / gorgée | Dégâts / soin. |
| Battement de cœur | Vie basse ; plus rapide quand elle est très basse. |
| Ondulation vertigineuse | Un effet d'état vous a touché. |
| Deux notes de cloche montantes | Un gain de niveau est prêt (appuyez sur E). |
| Toc sur du bois | Bord latéral de l'arène (aussi la fin d'une liste de menu). |
| Toc sur du métal | Limite avant ou arrière de votre zone de déplacement. |
| Clic d'interrupteur | L'aide à la visée est passée à une autre cible, du côté de la nouvelle. Avec le tir automatique, pas après une élimination. |

Chacun de ces sons peut s'écouter dans **F4 → Guide des sons**, avec flèche gauche / flèche droite pour
l'entendre de chaque côté.
En combat, **Q** nomme les sons que vous venez d'entendre.

## Filtres de son (touches numériques) {#sound-filters-number-keys}

En combat, les touches numériques coupent ou rétablissent un groupe de sons pour vous aider à vous
concentrer. Le jeu n'utilise aucune de ces touches.

| Touche | Groupe |
|---|---|
| 1 | Ennemis : tambours, grognement, préparation d'attaque, arrivée d'un boss |
| 2 | Boules : bourdonnement roulant, clic, rattrapage, boule manquée, bruit de culasse |
| 3 | Récompenses : cristaux, or, ressources, objets spéciaux |
| 4 | Tirs et dangers |
| 5 | Bords de l'arène |
| 6 | Votre vie : dégâts, soin, battements de cœur, états |
| 7 | Clic de l'aide à la visée |
| 0 | Tout rétablir |

- Chaque chiffre active ou coupe son propre groupe, indépendamment des autres.
- **Ctrl+chiffre** ne garde que ce groupe. Exemple : seulement les ennemis et les récompenses = Ctrl+1,
  puis 3.
- Votre choix est conservé pour la session suivante. Quand un combat commence avec des sons coupés, le
  mod dit lesquels.
- Couper ne change que les sons : les touches H, J, P, N répondent toujours et l'aide au rattrapage
  esquive toujours.

## Aides {#assists}

Tout ce que fait une aide peut aussi se faire à l'oreille ; les aides sont là quand vous en voulez.

- **Aide à la visée** (F5 change le mode) : vise pour vous l'ennemi le plus proche (par défaut),
  l'ennemi le plus fort, l'ennemi devant vous, un angle fixe, ou désactivée (souris). Les flèches
  l'ajustent. Elle vise une partie de l'ennemi qu'aucun autre ne cache, ignore les ennemis à un angle où votre héros ne peut pas tirer et, dans le mode par défaut, vise d'abord le boss, sauf si un autre ennemi est sur le point de vous atteindre.
- **Aide au rattrapage** (maintenir Maj) : votre personnage va sous la prochaine boule qui descend ; si
  aucune boule n'arrive, il va au cristal ou à l'objet le plus proche ; si un tir est sur le point de vous
  toucher, il s'écarte d'abord. W / S vous font toujours avancer et reculer, A / D reprennent toujours la
  main. Dans F4, elle peut être désactivée, sur « maintenir Maj » (par défaut) ou automatique. Si vous êtes blessé, elle va d'abord chercher un objet de soin.

## Paramètres du mod (F4) {#mod-settings-f4}

Flèche haut / flèche bas pour se déplacer, flèche gauche / flèche droite ou Entrée pour modifier, Échap
ferme. Les paramètres sont enregistrés.

- Mode de l'aide à la visée
- Aide au rattrapage : désactivée, maintenir Maj, automatique
- Signaux sonores activés / désactivés, volume des signaux
- Radar des ennemis, sons des boules, bords de l'arène, tirs et dangers (activé / désactivé)
- Sons des objets à ramasser : désactivés, objets spéciaux seulement, tous
- Sons de vie, clic de l'aide à la visée (activé / désactivé)
- Lire la description des améliorations au focus
- Touches (Entrée ouvre la liste) : voir [Changer les touches](#changing-keys)
- Guide des sons (Entrée l'ouvre)
- Journal de débogage
- Enregistrer les combats (désactivé par défaut) : chaque combat est écrit dans `UserData\BallPitAccess\telemetry.tsv`, pour régler les sons

## Changer les touches {#changing-keys}

**F4 → Touches** liste chaque touche du mod avec la touche qu'elle utilise maintenant. Flèche haut /
flèche bas pour choisir, **Entrée** puis appuyez sur la nouvelle touche (Échap annule), **Suppr** remet la
touche par défaut, et la dernière ligne les remet toutes. Si la nouvelle touche appartient déjà à une
autre action du mod, les deux s'échangent et le mod dit où l'autre est partie. Les touches dont les menus
ou le jeu ont besoin (flèches, Entrée, Tab, Échap, Retour arrière, Espace, W A S D F E R X) ne peuvent pas
être prises. L'aide F1 se termine par les touches telles qu'elles sont réglées maintenant.

## Conseils pour une première expédition {#tips-for-a-first-run}

- **Rattrapez vos boules.** Une boule rattrapée peut être relancée aussitôt ; une boule manquée met 2 à
  3 secondes à revenir. Suivez le bourdonnement roulant jusqu'au clic, puis ne bougez plus.
- **Ramassez vos cristaux d'EXP.** Ils ne se ramassent pas tout seuls. Suivez le tintement de verre ;
  étouffé veut dire que c'est derrière vous.
- **Survivez d'abord.** En début d'expédition, rester en vie compte plus que les dégâts.
- **Fusionnez et faites évoluer** vos boules quand le gain de niveau le propose : les choix disent
  « fusion » ou « évolution ».
- **Ralentissez pour les boss** avec Num - (ou la touche moins du jeu). Le mod vous le rappelle quand un
  boss arrive à une vitesse élevée.
- Mourir est normal : l'or que vous rapportez sert à construire Nouvelle-Baboulone, et les bâtiments
  rendent chaque expédition suivante plus forte.

## Limites connues {#known-limits}

- Le lanceur automatique de la base demande encore la souris.
- Les classements et les votes Twitch ne sont lus que de façon basique. On ne peut pas passer les crédits.
- Les paramètres du mod sont sur F4, pas dans l'écran d'options du jeu.
- Les textes propres au mod dans les langues autres que l'anglais et l'espagnol ont été traduits sans
  relecture par une personne de langue maternelle pour l'instant, le français compris. Les corrections
  sont les bienvenues. Les noms venant du jeu (bâtiments, boules, ennemis, améliorations) viennent
  toujours de la traduction du jeu lui-même.
- Les volumes des sons, leurs rythmes et la largeur de rattrapage n'ont pas encore été réglés avec des
  joueurs.

## Signaler un problème {#reporting-a-problem}

Fichiers utiles, dans le dossier du jeu :

- `UserData\BallPitAccess\speech.log` : tout ce que le mod a dit.
- `MelonLoader\Latest.log` : le journal du mod (erreurs, écrans, boules rattrapées et manquées).
- En combat, **Ctrl+Maj+D** écrit `UserData\BallPitAccess\threats.txt` (ennemis, boules, tirs, objets
  avec leurs positions) et la liste de l'écran ouvert dans `Latest.log` : utile quand un son indique la
  mauvaise direction ou qu'un menu se lit dans le mauvais ordre.
- `UserData\BallPitAccess\telemetry.tsv`, si vous avez activé F4 → Enregistrer les combats : chaque son,
  ennemi, boule et tir de vos combats. La meilleure aide quand les sons sont difficiles à suivre.

Dites ce que vous avez fait, ce que vous avez entendu et ce à quoi vous vous attendiez.

## Crédits {#credits}

- BALL x PIT de Kenny Sun, édité par Devolver Digital. Ce mod n'est pas officiel.
- MelonLoader (LavaGang) charge le mod. Prism envoie la parole à votre lecteur d'écran.
