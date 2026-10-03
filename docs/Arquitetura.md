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

Criar quando houver projeto Unity real. Não estão preenchidas neste pacote. O projeto ficará na subpasta `Game` do repositório; os caminhos abaixo são relativos a ela. Por exemplo, `Assets/_Game/Scenes/` corresponde a `Game/Assets/_Game/Scenes/` no Git.

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
| `Assets/_Game/Sprites/Pieces/Knight/` | `knight_normal.png`, `knight_trojan.png` |
| `Assets/_Game/Sprites/Pieces/King/` | `king_normal.png` |
| `Assets/_Game/Sprites/Pieces/Rook/` | `rook_normal.png` |
| `Assets/_Game/Sprites/Pieces/Bishop/` | `bishop_normal.png` |
| `Assets/_Game/Sprites/Pieces/Queen/` | `queen_normal.png` |
| `Assets/_Game/Sprites/Board/` | Arte do tabuleiro, se usada |
| `Assets/_Game/Sprites/UI/` | Vida, dano, moeda e sombra |
| `Assets/_Game/Prefabs/` | Casa, peça e feedback de dano |
| `Assets/_Game/Data/` | Definições compartilhadas |

Usar `Knight` para Cavalo como padrão de nomes. Os nomes `horse_*` usados na conversa são equivalentes conceituais; só renomear arquivos reais quando importados, preservando suas referências.

## Cuidados de implementação

- Somente o dono da peça pode comandá-la em seu turno.
- Toda compra valida saldo e elegibilidade antes de alterar dados.
- Morte remove referência do tabuleiro, seleção e representação.
- Invocação respeita limites e casas ocupadas.
- A classe Ninja separa visibilidade de existência e ocupação.
- Vitória bloqueia ações e renda futura.
- Uma classe nova pode reutilizar movimento e salto; não duplicar o sistema inteiro.
