# Arquitetura planejada

Ainda não implementada. O objetivo é dividir responsabilidades sem criar uma infraestrutura maior que o jogo.

## Sistemas

| Responsabilidade | Nome sugerido | Faz |
|---|---|---|
| Tabuleiro | `BoardState` / `BoardView` | Ocupação, coordenadas e desenho das casas |
| Peças | `PieceState` / `PieceView` | Tipo, dono, classe, vida atual e representação |
| Regras | `MoveRules` | Destinos de movimento e ataque válidos |
| Turnos | `TurnController` | Jogador atual e início/fim de turno |
| Combate | `CombatResolver` | Dano, remoção e resultado do ataque |
| Economia | `EconomyController` | Saldo, renda e pagamento |
| Evoluções | `EvolutionController` | Validação e transformação de classe |
| Apresentação | `PieceAnimator` | Salto, retorno, impacto e fade |
| Interface | `GameUI` | Seleção, atributos, compras e vitória |

Nomes são uma sugestão. Começar com os componentes exigidos pelo próximo marco, sem criar classes vazias para todos os sistemas.

## Dados de peça

Cada instância guarda identificação, tipo original, classe atual, jogador dono, coordenada e vida atual. Definições compartilhadas guardam vida máxima, dano, sprite e custo de evolução. Usar ScriptableObjects para essas definições é uma opção; não é requisito para o primeiro teste.

O estado lógico usa coordenadas inteiras do tabuleiro. A posição visual pode subir durante o salto sem mudar o alcance ou a ocupação da peça.

## Sequência de uma ação

Entrada → validação → resolução lógica com comandos bloqueados → apresentação → habilidade de morte, se houver → vitória ou próximo turno.

O salto não detecta colisão física para decidir dano. O resultado vem das regras de tabuleiro, e o impacto aplica o resultado uma única vez. Pode ser usada uma coroutine com um ponto de impacto explícito.

## Pastas Unity planejadas

O gerador `Tools/CreateProjectStructure.cmd` cria todas as pastas abaixo após existir um projeto Unity real. Ele não cria código, imagens ou cenas e pode ser executado novamente sem sobrescrever arquivos. O projeto ficará na subpasta `Game` do repositório; os caminhos abaixo são relativos a ela. Por exemplo, `Assets/_Game/Scenes/` corresponde a `Game/Assets/_Game/Scenes/` no Git.

| Pasta | Uso |
|---|---|
| `Assets/_Game/Scenes/` | Cena de partida e, se necessário, menu |
| `Assets/_Game/Scripts/Board/` | Tabuleiro e coordenadas |
| `Assets/_Game/Scripts/Pieces/` | Estado, tipos e visual de peças |
| `Assets/_Game/Scripts/Combat/` | Dano e morte |
| `Assets/_Game/Scripts/Turns/` | Turnos e resolução de ações |
| `Assets/_Game/Scripts/Economy/` | Moedas e compras |
| `Assets/_Game/Scripts/Evolutions/` | Troia e Ninja |
| `Assets/_Game/Scripts/UI/` | Seleção e informações |
| `Assets/_Game/Scripts/Presentation/` | Saltos e feedback |
| `Assets/_Game/Sprites/Pieces/Pawn/` | `pawn_normal.png`, `pawn_ninja.png` |
| `Assets/_Game/Sprites/Pieces/Horse/` | `horse_normal.png`, `horse_trojan.png` |
| `Assets/_Game/Sprites/Pieces/King/` | `king_normal.png` |
| `Assets/_Game/Sprites/Pieces/Rook/` | `rook_normal.png` |
| `Assets/_Game/Sprites/Pieces/Bishop/` | `bishop_normal.png` |
| `Assets/_Game/Sprites/Pieces/Queen/` | `queen_normal.png` |
| `Assets/_Game/Sprites/Board/` | Arte do tabuleiro, se usada |
| `Assets/_Game/Sprites/UI/` | Vida, dano, moeda e sombra |
| `Assets/_Game/Prefabs/` | Casa, peça e feedback de dano |
| `Assets/_Game/Data/` | Definições compartilhadas |

Manter a pasta `Horse` e os nomes `horse_*` do esquema enviado por Arthur. `Knight` pode ser usado como nome do tipo lógico do Cavalo de xadrez; não exige renomear as imagens.

## Criar as pastas

Na raiz do clone, depois de criar o projeto `Game`, executar no PowerShell:

```powershell
git pull --ff-only
.\Tools\CreateProjectStructure.cmd
```

O gerador exige `Game/Assets`, `Game/Packages` e `Game/ProjectSettings`. Ele cria um `.gitkeep` em cada pasta final para que o Git preserve pastas ainda vazias. Esses marcadores não são sprites nem scripts. Ao importar as pastas, deixar a Unity gerar os arquivos `.meta` e incluí-los no próximo commit.

## Cuidados de implementação

- Somente o dono da peça pode comandá-la em seu turno.
- Toda compra valida saldo e elegibilidade antes de alterar dados.
- Morte remove referência do tabuleiro, seleção e representação.
- Invocação respeita limites e casas ocupadas.
- A classe Ninja separa visibilidade de existência e ocupação.
- Vitória bloqueia ações e renda futura.
- Uma classe nova pode reutilizar movimento e salto; não duplicar o sistema inteiro.
