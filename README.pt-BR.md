# BALL x PIT Access

Um mod que torna **BALL x PIT** (Kenny Sun / Devolver Digital) jogável sem enxergar. Ele lê todos os menus
pelo seu leitor de tela, permite usar o jogo inteiro pelo teclado e transforma o campo de batalha em som:
inimigos, suas esferas, disparos inimigos, cristais de EXP e itens especiais têm cada um o seu som, que
vem do lado em que estão em relação a você.

Versão 0.5.0, para a versão 1.308 do jogo, incluindo o DLC Bolalônia Elevada (Risen Ballbylon). O mod fala
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

- **Construção**: as setas movem a edificação um bloco e dizem se ela cabe, Enter constrói, R gira, Home
  pula para um espaço livre, Backspace cancela.
- **Colheita**: ela começa mirando em uma edificação. Enter lança os trabalhadores, Tab mira em outra
  edificação, as setas para a esquerda e para a direita giram a mira, segure Espaço para acelerar, Esc
  sai.
- **Expansão**: Tab escolhe um terreno, Enter compra.
- **Reorganizar**: Tab escolhe uma edificação, Enter pega, as setas movem, Enter solta.

## Batalha {#battle}

### As teclas do próprio jogo

| Tecla | Ação |
|---|---|
| W A S D | Mover-se |
| Espaço | Disparar (segurar) |
| F | Disparo automático ligado / desligado |
| E | Abrir uma subida de nível quando houver uma pronta |
| = / - | Velocidade do jogo mais rápida / mais lenta |
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
| J | Inimigos e chefe |
| K | Sua posição |
| G | Velocidade do jogo |
| L | Nível, tempo, onda, engrenagens ganhas |
| B | Suas esferas e passivas |
| P | Itens no campo |
| N | Perigos (disparos, lasers, bombas, chão) |

Falado durante a batalha: o nível no começo, os chefes e a vida deles em 75, 50 e 25 por cento, dano e
vida, efeitos de status, itens especiais quando caem, "subidas de nível prontas, pressione E", o fim da
partida.

## Sons de batalha {#battle-sounds}

Quatro regras valem para todos os sons:

- **Lado**: um som vem do lado da coisa de que ele fala.
- **Altura**: mais agudo significa mais perto.
- **Abafado** significa que está atrás de você.
- Um som que pede para você **se mover** vem do lado para onde ir.

As coisas que se movem o tempo todo (a sua esfera, os disparos inimigos) são **sons contínuos** que as
seguem; as coisas que mudam aos poucos (inimigos avançando, itens no chão) são sons curtos.

| Som | Significado |
|---|---|
| Tambor de mão | Um inimigo. Mais agudo e mais rápido = mais perto. Os três mais próximos tocam um de cada vez. |
| Tambor grande e grave | Um chefe. Tambores, gongo e trompa quando um chega. |
| Rosnado | Um inimigo está prestes a alcançar você. |
| Gemido que sobe | Um inimigo prepara um ataque, do lado em que ele está. |
| **Zumbido rolante** (contínuo) | A esfera que você pode pegar. Ande na direção do lado dela; ele **sobe conforme você se aproxima de onde a esfera vai cair**, pulsa mais rápido conforme a esfera chega perto de você e fica mais suave enquanto a esfera ainda pode quicar em um inimigo. |
| Clique quando o zumbido sobe um degrau | Você está embaixo da esfera: fique aí. |
| Baque de luva | Esfera pega. |
| Baque surdo que afunda | Uma esfera passou por você. |
| Engatilhar de arma | Suas esferas voltaram: você pode disparar (em silêncio com o disparo automático). |
| Tilintar de vidro | O cristal de EXP, ouro ou recurso mais próximo até o qual você pode andar. Eles não são coletados sozinhos. |
| Brilho de kalimba | Um item especial: reator de fusão, diagrama, ovo, engrenagem, vida, ímã, bomba, saco de gemas. |
| **Assobio de ar** (contínuo) | Um disparo inimigo voando perto de você, mais agudo conforme se aproxima. Mais alto quando vai acertar você. |
| Sopro e estalo | Um disparo vai acertar você: dê um passo para o lado em que ele soa. |
| Zumbido elétrico / dois choques | Um laser está mirando / você está na linha dele: dê um passo para o lado em que ele soa. |
| Assobio que cai / chiado de pavio | Uma bomba está caindo perto / ela vai cair em você: dê um passo para o lado em que ele soa. |
| Chapinhar | Você pisou em um perigo no chão (o nome dele é falado). |
| Soco / gole | Dano / cura. |
| Batimento cardíaco | Vida baixa; mais rápido quando está muito baixa. |
| Oscilação tonta | Um efeito de status atingiu você. |
| Duas notas de sino subindo | Uma subida de nível está pronta (pressione E). |
| Batida de madeira | Borda lateral da arena (também o fim de uma lista de menu). |
| Batida de metal | Limite da frente ou de trás da sua área de movimento. |
| Clique de interruptor | A assistência de mira mudou de alvo, do lado do novo alvo. |

Todos esses sons podem ser ouvidos em **F4 → Guia de sons**, com as setas para a esquerda e para a direita
para ouvi-los de cada lado.

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
  ajuste fino.
- **Assistência para pegar** (segurar Shift): o herói anda até ficar embaixo da próxima esfera que está
  descendo; sem nenhuma esfera vindo, ele anda até o cristal ou item mais próximo; se um disparo está
  prestes a acertar você, antes ele sai do caminho. W / S ainda movem você para a frente e para trás, A / D
  sempre assumem o controle. Em F4 ela pode ficar desativada, em "segurar Shift" (padrão) ou automática.

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
- Guia de sons (Enter abre)
- Registro de depuração

## Dicas para a primeira partida {#tips-for-a-first-run}

- **Pegue suas esferas.** Uma esfera pega pode ser disparada de novo na hora; uma perdida leva de 2 a 3
  segundos para voltar. Siga o zumbido rolante até ouvir o clique e então fique parado.
- **Colete seus cristais de EXP.** Eles não são coletados sozinhos. Siga o tilintar de vidro; abafado
  significa que está atrás de você.
- **Primeiro, sobreviva.** No começo de uma partida, ficar vivo importa mais do que o dano.
- **Funda e evolua** suas esferas quando a subida de nível oferecer isso: as opções dizem "fusão" ou
  "evolução".
- **Desacelere para os chefes** com a tecla de menos. O mod avisa quando um chefe chega com a velocidade
  alta.
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

Diga o que você fez, o que você ouviu e o que esperava.

## Créditos {#credits}

- BALL x PIT de Kenny Sun, publicado pela Devolver Digital. Este mod não é oficial.
- O MelonLoader (LavaGang) carrega o mod. O Prism envia a fala para o seu leitor de tela.
