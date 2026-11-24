# BALL x PIT Access

Um mod que torna **BALL x PIT** (Kenny Sun / Devolver Digital) jogável sem enxergar. Ele lê todos os menus
pelo seu leitor de tela, permite usar o jogo inteiro pelo teclado e transforma o campo de batalha em som:
inimigos, suas esferas, disparos inimigos, cristais de EXP e itens especiais têm cada um o seu som, que
vem do lado em que estão em relação a você.

Versão 0.8.0, para a versão 1.308 do jogo, incluindo o DLC Bolalônia Elevada (Risen Ballbylon). O mod fala
**todos os idiomas que o jogo oferece** e segue o idioma configurado no jogo.

<!-- LANGS -->
[English](README.html) · [Español](README.es.html) · [Français](README.fr.html) · [Deutsch](README.de.html) · [Italiano](README.it.html) · **Português (Brasil)** · [Polski](README.pl.html) · [Türkçe](README.tr.html) · [Русский](README.ru.html) · [Українська](README.uk.html) · [日本語](README.ja.html) · [한국어](README.ko.html) · [简体中文](README.zh-CN.html) · [繁體中文](README.zh-TW.html) · [ไทย](README.th.html)
<!-- /LANGS -->

> **Situação: versão de teste inicial.** Os menus, a base e as primeiras partidas já foram jogados com o
> mod. Os sons de batalha e as partes mais novas (som contínuo da esfera, assistência para pegar, filtros
> de som) ainda não foram ajustados com jogadores. Por favor, conte o que funciona e o que não funciona
> (veja [Relatar um problema](#reporting-a-problem)).

## Conteúdo

- [Do que você precisa](#what-you-need)
- [Instalação](#installing)
- [Remover o mod](#removing-the-mod)
- [Primeiros passos](#first-steps)
- [Teclas em todo lugar](#keys-everywhere)
- [Menus](#menus)
- [A base (Nova Bolalônia)](#the-base-new-ballbylon)
- [Batalha](#battle)
- [Sons de batalha](#battle-sounds)
- [Filtros de som (teclas de número)](#sound-filters-number-keys)
- [Assistências](#assists)
- [Configurações do mod (F4)](#mod-settings-f4)
- [Mudar as teclas](#changing-keys)
- [Controle](#controller)
- [Dicas para a primeira partida](#tips-for-a-first-run)
- [Limitações conhecidas](#known-limits)
- [Relatar um problema](#reporting-a-problem)
- [Créditos](#credits)

## Do que você precisa {#what-you-need}

- BALL x PIT no Windows (Steam), versão 1.308.
- Um leitor de tela: o NVDA é o que foi testado. Outros suportados pela biblioteca de fala Prism (JAWS,
  Narrador, vozes SAPI) também devem funcionar.
- **MelonLoader 0.7.3** (x64), o carregador de mods. Gratuito, na página oficial de versões dele no GitHub.
- Fones de ouvido ou caixas de som estéreo: esquerda e direita importam muito na batalha.

## Instalação {#installing}

**Com o instalador (o jeito mais fácil):** baixe `BallPitAccessibilityInstaller.exe` da
[versão mais recente](https://github.com/gomods1/BallPitAccess/releases/latest) e execute-o. Ele encontra o jogo pela Steam, instala o MelonLoader 0.7.3, a
biblioteca de voz Prism e o mod, e depois os atualiza ou remove. Em seguida, abra o jogo: a primeira
abertura leva alguns minutos e fica em silêncio enquanto o MelonLoader prepara os arquivos do jogo;
depois o seu leitor de tela começa a falar.

**Manualmente:** baixe `BallPitAccess-vX.Y.Z.zip` da mesma página e extraia tudo na pasta do jogo (por
padrão `C:\Program Files (x86)\Steam\steamapps\common\BALLxPIT`, a pasta que contém `Balls.exe`), de modo que `version.dll` fique ao lado de `Balls.exe`. O
zip já inclui o MelonLoader: não é preciso instalá-lo à parte. Abra o jogo; a primeira abertura é lenta e
silenciosa, como acima.

## Remover o mod {#removing-the-mod}

O botão Desinstalar do instalador remove o mod e o MelonLoader e mantém as suas configurações em
`UserData`. Manualmente:

Apague `Mods\BallPitAccess.dll` e `prism.dll`. Para remover também o MelonLoader, apague `version.dll` e as
pastas `MelonLoader`, `Mods`, `Plugins` e `UserData` da pasta do jogo. O mod não altera nenhum arquivo do
jogo.

## Primeiros passos {#first-steps}

- Pressione **F1** a qualquer momento para abrir a ajuda: todas as partes dela, começando pela parte do
  lugar onde você está. As setas para cima e para baixo leem linha por linha, Tab pula para a próxima
  parte, Esc fecha.
- Pressione **F4** para as configurações do mod e, dentro delas, abra o **guia de sons**: ele toca cada
  som de batalha e diz o que significa. Ouvi-lo uma vez antes da sua primeira partida ajuda muito.
- Com teclado, o próprio jogo é controlado pelo mouse; com o mod você nunca precisa do mouse, exceto
  para o lançador ocioso da base (veja [Limitações conhecidas](#known-limits)).

## Teclas em todo lugar {#keys-everywhere}

| Tecla | Ação |
|---|---|
| F1 | Ajuda (todas as partes; Tab / Shift+Tab entre as partes) |
| F2 | Ler a tela inteira |
| F3 | Repetir a última coisa dita |
| F4 | Configurações do mod e guia de sons |
| F5 | Mudar o modo da assistência de mira |
| F6 / F7 | Girar a mira 10 graus para a esquerda / direita |
| F8 | Dizer para onde você está mirando |
| Ctrl+seta para cima | Ler os detalhes do item em foco (Ctrl+seta para baixo volta) |
| Ctrl+seta para a esquerda / Ctrl+seta para a direita | Alternar entre detalhes e eventos recentes |

Enquanto uma lista do mod (ajuda, configurações, guia de sons) está aberta, o jogo não recebe as suas
teclas.

## Menus {#menus}

Cada tela é **uma única lista em uma ordem lógica**: primeiro as opções, depois os botões e por último
Fechar. A ordem nunca depende de onde as coisas estão desenhadas na tela.

| Tecla | Ação |
|---|---|
| Seta para cima / seta para baixo | Item anterior / próximo |
| Enter | Escolher |
| Tab / Shift+Tab | Próximo / anterior grupo da tela |
| Home / End | Primeiro / último item do grupo |
| Seta para a esquerda / seta para a direita | Mudar uma opção de lista ou de controle deslizante; em um nível, mudar a dificuldade dele |
| Ctrl+Tab / Ctrl+Shift+Tab | Próxima / anterior aba nas telas que têm abas |
| Backspace ou Esc | Voltar |
| Page Up / Page Down | Conjunto na enciclopédia; nível de Novo Jogo+ na lista de níveis |
| Shift+Enter | Em um nível: abrir o placar dele |

- Enter em um personagem leva você ao botão Selecionar dele.
- Quando o tutorial guiado aponta para algo, o foco pula para lá: pressione Enter.
- As opções de melhoria são lidas com os números ("Causa de 10 a 15 de dano").

## A base (Nova Bolalônia) {#the-base-new-ballbylon}

| Tecla | Ação |
|---|---|
| Seta para cima / seta para baixo, Enter | Botões da barra lateral: Jogar, Colher, Construir, Expandir, Reorganizar, Opções |
| Tab / Shift+Tab | Percorrer o elevador e as suas edificações |
| Enter em uma edificação | Começar uma colheita mirando nela, se estiver pronta; senão, abrir a melhoria dela |
| Shift+Enter | Abrir a melhoria de uma edificação |
| Enter no elevador | Abrir a melhoria do elevador (para o poço, ou para a Bolalônia Elevada) |
| Delete | Desmontar uma edificação |
| H | Recursos e dia |
| J | Colheita e trabalhadores |
| K | Explorar a base casa por casa |
| I | Posicionando ou movendo um prédio: o que este lugar atrapalha |

- **Construção**: as setas movem a edificação um bloco e dizem se ela cabe, Enter constrói, R gira, Home
  pula para um espaço livre, Backspace cancela.
- **Colheita**: ela começa mirando em uma edificação. Enter lança os trabalhadores, Tab mira em outra
  edificação, as setas para a esquerda e para a direita giram a mira, segure Espaço para acelerar, Esc
  sai.
- **Construção**: edificações em construção ou em melhoria também estão na lista do Tab da colheita. Cada trabalhador que bate em uma delas a constrói; o mod avisa quando ela fica pronta. Enter em uma delas na base começa a colheita mirando nela.
- **Os trabalhadores vão chegar?** Quando a colheita mira em uma construção (Tab, ou Enter nela na base), o mod segue o caminho dos trabalhadores do jeito que o jogo os move e escolhe um ângulo que chega: "os trabalhadores acertam direto", "os trabalhadores chegam rebatendo em uma parede", "os trabalhadores passam por cima" (campos de trigo), ou "sem caminho livre, Floresta está no meio". Uma construção sem caminho livre só é alcançada por sorte: mova-a, ou mova a que está no caminho. Esquerda / Direita dizem no que a linha acerta.
- **Depois de uma colheita** o mod diz o que cada construção em obras recebeu ("Casa Assombrada: 6 golpes, de 40 para 55 por cento"). O resumo do jogo só conta recursos: diz "Um Esforço Infrutífero" sempre que nada foi colhido, mesmo que os trabalhadores tenham construído.
- **Expansão**: Tab escolhe um terreno, Enter compra.
- **Reorganizar**: Tab escolhe uma edificação, Enter pega, as setas movem, Enter solta.
- **Explorar** (K): as setas andam uma casa e dizem o que há nela: livre (e quantas casas livres seguem para a direita e para baixo), uma construção com o tamanho, ou terreno não comprado. Tab / Shift+Tab pulam para a próxima / anterior casa livre. K, Backspace ou Esc saem. As colunas e linhas são as mesmas usadas ao posicionar uma construção.
- O menu de construção diz quanto espaço cada construção ocupa ("3 de largura, 2 de altura"), e o posicionamento começa com o nome e o tamanho.
- **Posicionar ou mover um prédio**: a posição diz o que a bloqueia ("bloqueado por Floresta"). I diz se os trabalhadores alcançam o lugar e quais prédios ele isolaria ou deixaria alcançáveis só ricocheteando numa parede. Home encontra o melhor lugar livre, que os trabalhadores acertam em linha reta e que não isola ninguém, e gira o prédio para caber. Depois de R a nova forma é dita: alguns prédios, como a casa assombrada, não são retângulos ("canto aberto em cima à esquerda"). Ao reorganizar também dá para girar os prédios.
- **Explorar**: Page Down / Page Up pulam para o próximo prédio / o anterior e dizem onde ele está, o tamanho e a forma.

## Batalha {#battle}

### As teclas do próprio jogo

| Tecla | Ação |
|---|---|
| W A S D | Mover-se |
| Espaço | Disparar (segurar) |
| F | Disparo automático ligado / desligado |
| E | Abrir uma subida de nível quando houver uma pronta |
| = / - | Velocidade do jogo mais rápida / mais lenta (não existem em alguns layouts de teclado: use Num + / Num - do mod, abaixo) |
| Esc | Pausar |

### Teclas do mod

| Tecla | Ação |
|---|---|
| Seta para a esquerda / seta para a direita | Girar a mira 5 graus manualmente |
| Seta para cima | Devolver a mira à assistência |
| Seta para baixo | Dizer para onde você está mirando |
| Shift (segurar) | Assistência para pegar (veja [Assistências](#assists)) |
| 1 a 7, 0 | Filtros de som (veja [Filtros de som](#sound-filters-number-keys)) |
| H | Vida e efeitos de status |
| U | Status com os segundos restantes, e se você pode se mover, e se não, por quê (congelado, empurrado por um golpe, inimigos avançando). Quando um congelamento acaba o mod diz "Você pode se mover de novo" |
| J | Inimigos e chefe |
| K | Sua posição |
| G | Velocidade do jogo |
| L | Nível, tempo, onda, engrenagens ganhas |
| B | Suas esferas e passivas |
| P | Itens no campo |
| N | Perigos (disparos, lasers, bombas, chão) |
| Q | Que som foi esse? Os sons tocando agora e os últimos, com o que cada um significa e o seu lado |
| M | Mapa da batalha (pausa) |
| Num + / Num - | Velocidade do jogo mais rápida / mais lenta, em qualquer layout de teclado |

Todas as teclas do mod podem ser mudadas: veja [Mudar as teclas](#changing-keys).

Falado durante a batalha: o nível no começo, os chefes e a vida deles em 75, 50 e 25 por cento, dano e
vida, efeitos de status, itens especiais quando caem, "subidas de nível prontas, pressione E", o fim da
partida.

**Chefes**: J dá a vida real do chefe, a mesma da barra dele no jogo. Alguns chefes só levam dano em um ponto fraco: o corpo do Rei Esqueleto não pode ser ferido, só a coroa na cabeça dele, e o corpo a protege pela frente, então as bolas chegam nela depois de rebater nas paredes. J diz onde está o ponto fraco, e a assistência de mira vai atrás dele.

**Mapa da batalha (pausa)**: M abre o mapa da batalha e pausa o jogo. As setas percorrem o campo casa por casa e dizem o que há nela: inimigos com a vida deles, suas bolas, tiros, lasers, bombas, perigos no chão e itens. Control e seta para cima diz o que um inimigo faz, o estado dele e os efeitos. Tab pula de inimigo em inimigo, o mais próximo primeiro; Page Down para as outras coisas; Home volta para você; End conta tudo. Enter trava a mira no inimigo; quando ele morre, a mira volta para o inimigo mais próximo. Delete destrava a mira. M ou Esc fecha o mapa e a batalha continua.

## Sons de batalha {#battle-sounds}

Três regras valem para todos os sons, sempre em relação ao seu herói:

- **Lado**: um som vem de onde está a coisa de que ele fala: da esquerda se ela está à sua esquerda, da
  direita se está à sua direita, do centro quando está bem à frente.
- **Altura**: mais agudo significa mais perto.
- **Abafado** significa que está atrás de você.

As coisas que se movem o tempo todo (a sua esfera, os disparos inimigos) são **sons contínuos** que as
seguem; as coisas que mudam aos poucos (inimigos avançando, itens no chão) são sons curtos.

| Som | Significado |
|---|---|
| Tambor de mão | Um inimigo. Mais agudo e mais rápido = mais perto. Quando os inimigos se movem, os mais próximos tocam em sequência; enquanto nada muda, só o mais próximo. |
| Tambor grande e grave | Um chefe. Tambores, gongo e trompa quando um chega. |
| Rosnado | Um inimigo está prestes a alcançar você. |
| Gemido que sobe | Um inimigo prepara um ataque, do lado em que ele está. |
| **Zumbido rolante** (contínuo) | A esfera que você pode pegar. Ele soa de **onde a esfera está** e **sobe conforme a esfera se aproxima de você**, então você ouve para onde ela vai; pulsa mais rápido conforme a esfera chega perto da sua linha e fica mais suave enquanto a esfera ainda pode quicar em um inimigo. |
| Clique | Você está parado onde a esfera vai cair: fique aí. |
| Baque de luva | Esfera pega. |
| Baque surdo que afunda | Uma esfera passou por você. |
| Engatilhar de arma | Suas esferas voltaram: você pode disparar (em silêncio com o disparo automático). |
| Tilintar de vidro | O cristal de XP, ouro ou recurso até o qual andar: o mais próximo, ou um monte deles um pouco mais longe. Eles não são coletados sozinhos. |
| Brilho de kalimba | Um item especial: reator de fusão, diagrama, ovo, engrenagem, vida, ímã, bomba, saco de gemas. |
| **Assobio de ar** (contínuo) | Um disparo inimigo voando perto de você, mais agudo conforme se aproxima. Mais alto quando vai acertar você. |
| Sopro e estalo | Um disparo vai acertar você; ele soa do disparo: saia do caminho dele. |
| Zumbido elétrico / dois choques | Um laser está mirando / você está na linha dele; os dois soam do laser: saia da linha. |
| Assobio que cai / chiado de pavio | Uma bomba está caindo perto / ela vai cair em você; os dois soam de onde ela cai: afaste-se de lá. |
| Chapinhar | Você pisou em um perigo no chão (o nome dele é falado). |
| Soco / gole | Dano / cura. |
| Batimento cardíaco | Vida baixa; mais rápido quando está muito baixa. |
| Oscilação tonta | Um efeito de status atingiu você. |
| Duas notas de sino subindo | Uma subida de nível está pronta (pressione E). |
| Batida de madeira | Borda lateral da arena (também o fim de uma lista de menu). |
| Batida de metal | Limite da frente ou de trás da sua área de movimento. |
| Clique de interruptor | A assistência de mira trocou para outro alvo, do lado do novo. Com o disparo automático, não depois de um abate. |

Todos esses sons podem ser ouvidos em **F4 → Guia de sons**, com as setas para a esquerda e para a direita
para ouvi-los de cada lado.
Na batalha, **Q** diz quais sons você acabou de ouvir.

## Filtros de som (teclas de número) {#sound-filters-number-keys}

Na batalha, as teclas de número silenciam ou reativam um grupo de sons para você se concentrar. O jogo não
usa nenhuma dessas teclas.

| Tecla | Grupo |
|---|---|
| 1 | Inimigos: tambores, rosnado, preparação de ataque, chegada de chefe |
| 2 | Esferas: zumbido rolante, clique, esfera pega, esfera perdida, engatilhar de arma |
| 3 | Recompensas: cristais, ouro, recursos, itens especiais |
| 4 | Disparos e perigos |
| 5 | Bordas da arena |
| 6 | Sua vida: dano, cura, batimento cardíaco, status |
| 7 | Clique da assistência de mira |
| 0 | Tudo ativado de novo |

- Cada número liga e desliga o seu próprio grupo, independentemente dos outros.
- **Ctrl+número** deixa só esse grupo. Exemplo: só inimigos e recompensas = Ctrl+1, depois 3.
- A sua escolha fica salva para a próxima sessão. Quando uma batalha começa com algo silenciado, o mod
  diz o que está silenciado.
- Silenciar muda só os sons: as teclas H, J, P e N continuam respondendo e a assistência para pegar
  continua desviando.

## Assistências {#assists}

Tudo o que uma assistência faz também pode ser feito de ouvido; as assistências estão aí para quando
você quiser.

- **Assistência de mira** (F5 muda o modo): mira por você no inimigo mais próximo (padrão), no inimigo
  mais forte, no inimigo à sua frente, em um ângulo fixo, ou fica desativada (mouse). As setas fazem o
  ajuste fino. Ela mira numa parte do inimigo que nenhum outro esconde, ignora inimigos num ângulo em que seu herói não pode atirar e, no modo padrão, vai primeiro no chefe, a menos que outro inimigo esteja prestes a alcançar você.
- **Assistência para pegar** (segurar Shift): o herói anda até ficar embaixo da próxima esfera que está
  descendo; sem nenhuma esfera vindo, ele anda até o cristal ou item mais próximo; se um disparo está
  prestes a acertar você, antes ele sai do caminho. W / S ainda movem você para a frente e para trás, A / D
  sempre assumem o controle. Em F4 ela pode ficar desativada, em "segurar Shift" (padrão) ou automática. Se você estiver ferido, ela busca primeiro um item de vida.

## Configurações do mod (F4) {#mod-settings-f4}

Seta para cima / seta para baixo movem, seta para a esquerda / seta para a direita ou Enter mudam, Esc
fecha. As configurações são salvas.

- Modo da assistência de mira
- Assistência para pegar: desativada, segurar Shift, automática
- Sons de ajuda ativados / desativados, volume dos sons
- Radar de inimigos, sons das esferas, bordas da arena, disparos e perigos (ativado / desativado)
- Sons de itens: desativados, só itens especiais, todos
- Sons de vida, clique da assistência de mira (ativado / desativado)
- Ler as descrições das melhorias ao focar
- Controle
- Teclas (Enter abre): veja [Mudar as teclas](#changing-keys)
- Guia de sons (Enter abre)
- Registro de depuração
- Gravar batalhas (desligado por padrão): cada batalha é gravada em `UserData\BallPitAccess\telemetry.tsv`, para ajustar os sons

## Mudar as teclas {#changing-keys}

**F4 → Teclas** lista cada tecla do mod com a tecla que ela usa agora. Seta para cima / seta para baixo
escolhem, **Enter** e depois pressione a nova tecla (Esc cancela), **Delete** volta para a tecla padrão, e
a última linha restaura todas. Se a nova tecla já pertence a outra ação do mod, as duas trocam de lugar e
o mod diz para onde a outra foi. As teclas de que os menus ou o jogo precisam (setas, Enter, Tab, Esc,
Backspace, Espaço, W A S D F E R X) não podem ser usadas. A ajuda do F1 termina com as teclas como estão
agora.

## Controle {#controller}

- Botões do Xbox; um controle de PlayStation ou de Switch funciona com o Steam Input ativado para o jogo
- Fora de uma batalha o direcional ou o analógico esquerdo movem como as setas, A é Enter e B volta
- X e Y pulam para o primeiro e o último item; ao posicionar uma construção X procura um lugar onde ela cabe e Y a gira
- RB e LB são Tab e Shift Tab, RT e LT trocam as abas da tela
- Start é a pausa do jogo
- Em uma batalha o jogo usa os analógicos, os gatilhos e os botões
- Direcional para cima diz os inimigos, para baixo sua vida, para a esquerda seus estados, para a direita seu progresso
- Segure o analógico esquerdo pressionado para pegar, como Shift
- Se deixar o analógico direito parado, a assistência de mira mira por você; mova-o para mirar você mesmo
- Segure View para mais: A ajuda, B configurações do mod, X lê a tela, Y repete, Start muda o modo de mira
- View com LB e RB lê linhas mais antigas e mais novas da lista de revisão, View com um clique de analógico alterna entre detalhes e eventos
- Em uma batalha View com o direcional: para cima perigos, para baixo equipamento, para a esquerda itens, para a direita o último som; View com LT e RT deixa o jogo mais lento e mais rápido
- Na base View com o direcional: para cima colheita, para baixo recursos, para a esquerda modo explorar
- Nos menus View com LT e RT é Page Up e Page Down

Na batalha, clicar o analógico direito abre o mapa da batalha; no menu de pausa, Exibir e direcional para a direita. No mapa o direcional anda, RB e LB pulam entre inimigos, A trava a mira e B fecha.

## Dicas para a primeira partida {#tips-for-a-first-run}

- **Pegue suas esferas.** Uma esfera pega pode ser disparada de novo na hora; uma perdida leva de 2 a 3
  segundos para voltar. Siga o zumbido rolante até ouvir o clique e então fique parado.
- **Colete seus cristais de EXP.** Eles não são coletados sozinhos. Siga o tilintar de vidro; abafado
  significa que está atrás de você.
- **Primeiro, sobreviva.** No começo de uma partida, ficar vivo importa mais do que o dano.
- **Funda e evolua** suas esferas quando a subida de nível oferecer isso: as opções dizem "fusão" ou
  "evolução".
- **Desacelere para os chefes** com Num - (ou a tecla de menos do jogo). O mod avisa quando um chefe chega
  com a velocidade alta.
- Morrer é normal: o ouro que você traz de volta constrói a Nova Bolalônia, e as edificações deixam cada
  partida seguinte mais forte.

## Limitações conhecidas {#known-limits}

- O lançador ocioso da base ainda precisa do mouse.
- Os placares e a votação da Twitch são lidos só de forma básica. Os créditos não podem ser pulados.
- As configurações do mod ficam no F4, não dentro da tela de opções do próprio jogo.
- Os textos do próprio mod em idiomas que não sejam inglês e espanhol foram traduzidos sem revisão de um
  falante nativo até agora, e isso vale também para o português. Correções são bem-vindas. Os nomes que
  vêm do jogo (edificações, esferas, inimigos, melhorias) sempre vêm da tradução do próprio jogo.
- Os volumes, as frequências dos sons e a largura da área para pegar as esferas ainda não foram ajustados
  com jogadores.

## Relatar um problema {#reporting-a-problem}

Arquivos úteis, na pasta do jogo:

- `UserData\BallPitAccess\speech.log`: tudo o que o mod falou.
- `MelonLoader\Latest.log`: o registro do mod (erros, telas, esferas pegas e perdidas).
- Na batalha, **Ctrl+Shift+D** grava `UserData\BallPitAccess\threats.txt` (inimigos, esferas, disparos,
  itens com as posições) e a lista da tela aberta no `Latest.log`: útil quando um som aponta para o lado
  errado ou quando um menu é lido na ordem errada.
- `UserData\BallPitAccess\telemetry.tsv`, se você ligou F4 → Gravar batalhas: cada som, inimigo, esfera e
  disparo das suas batalhas. A melhor ajuda quando os sons são difíceis de acompanhar.

Diga o que você fez, o que você ouviu e o que esperava.

## Créditos {#credits}

- BALL x PIT de Kenny Sun, publicado pela Devolver Digital. Este mod não é oficial.
- O MelonLoader (LavaGang) carrega o mod. O Prism envia a fala para o seu leitor de tela.
