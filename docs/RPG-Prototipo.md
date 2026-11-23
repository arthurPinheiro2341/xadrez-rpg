# RPG jogável — configuração e teste

A cena principal continua sendo `Game/Assets/_Game/Scenes/Board.unity`. Abra e pressione Play. O padrão é RPG contra bot (jogador com claras). O painel permite reiniciar, escolher RPG local ou retornar ao xadrez clássico local. Trocar o modo começa uma partida nova. O modo clássico conserva suas regras anteriores de xeque, roque, en passant e promoção.

## Uso

Selecione uma peça do jogador da vez e clique no destino destacado. A interface apresenta vida e dano. Atacar consome uma ação; se o defensor sobreviver, o atacante volta à origem. Se morrer, o atacante ocupa seu lugar, exceto no tiro da catapulta. A morte de um Rei encerra a partida imediatamente. Não há xeque no RPG.

Para evoluir, selecione a peça, clique em Evoluir e confirme a classe. É permitida uma compra antes da ação principal por turno; o Rei não evolui. A compra preserva a vida atual e os atributos básicos. Bot e humano chamam os mesmos métodos `Actions`, `Execute` e `Evolve` do modelo.

Ative Usar habilidade para o ataque frontal do Cavaleiro, a cura ou o tiro da catapulta. Clique no alvo destacado. As habilidades de Troia e Valkíria são passivas. Os ataques comuns diagonais do Peão Cavaleiro também aplicam o corte frontal.

## Valores editáveis

Selecione `Game/Assets/_Game/Resources/RpgBalance.asset` no Project. As listas seguem a ordem Peão, Torre, Cavalo, Bispo, Rainha, Rei. O controlador da cena referencia este asset; `Xadrez RPG/Configure RPG` restaura a associação e reimporta os sprites sem recriar o tabuleiro.

| Peça | Vida | Dano | Custo da classe |
|---|---:|---:|---:|
| Peão | 3 | 1 | 3 |
| Torre | 7 | 4 | 3 |
| Cavalo | 6 | 3 | 4 |
| Bispo | 5 | 3 | 3 |
| Rainha | 8 | 5 | 5 |
| Rei | 15 | 2 | sem classe |

Renda: 2 moedas no início do próprio turno, incluindo o primeiro. Saldo inicial antes da renda: zero. Consulta de movimentos, compra e conclusão de salto não creditam renda. Uma ação extra não inicia outro turno.

Escolhas de protótipo para valores não fechados:

- Cavaleiro: dano básico em cada inimigo das três casas frontais, sem fogo amigo; não possui recarga além de consumir a ação.
- Curandeiro: cura direcionada de 3 pontos, até 2 casas pela distância máxima entre eixos; ignora obstáculos, admite autocura, nunca ultrapassa a vida máxima. A área de cura por rodadas fica para uma expansão futura.
- Catapulta: tiro em qualquer direção até 2 casas pela mesma distância; ignora obstáculos, causa 1 de dano e permanece na origem. O ataque comum da Torre continua causando 4 de dano. Alcance e dano do tiro são configuráveis em `catapultRange` e `catapultDamage`.
- Troia: ao morrer, cria exatamente um Peão Cavaleiro na primeira casa vizinha vazia. Busca da fileira inferior para superior, esquerda para direita; exclui a casa da morte. Se nenhuma estiver livre, não invoca. Não invoca após a morte do Rei.
- Valkíria: atravessa obstáculos nas oito direções da Rainha, mas não aterrissa sobre aliado. Causa 3 de dano (configurável em `valkyrieDamage`); a Rainha comum conserva 5 de dano. Uma captura letal concede uma única ação adicional, restrita à mesma Valkíria; essa ação não encadeia novos bônus.
- RPG: sem roque/en passant/xeque. Mantém o avanço duplo inicial e promove gratuitamente o Peão a Rainha comum na última fileira, preservando o dano sofrido. São decisões deste protótipo, separadas do clássico.

Salto: duração 0,35 s, altura 45 unidades de Canvas; editáveis no asset e no componente RpgController durante Play. O deslocamento visual acompanha a base central da casa, sem modificar a ocupação lógica. Comandos e reinício ficam bloqueados durante salto e resposta do bot.

O painel oferece dificuldade Fácil, Médio e Difícil, com Médio como padrão. A escolha vale para as próximas jogadas e é mantida ao reiniciar durante a sessão. Fácil escolhe ações aleatórias e pode adiar compras. Médio prioriza dano, capturas letais, Rei, cura e avanço, e compra evoluções considerando custo e vida restante. Difícil simula até seis ações candidatas e até seis respostas, incluindo uma possível compra do adversário e ações extras da Valkíria. A busca é limitada: não é um motor competitivo. Nenhum nível recebe renda, vida, alcance ou dano adicional.

Cada ação do bot tem uma prévia de 0,7 s e uma pausa de resultado de 0,8 s, configuráveis em `actionPreviewDuration` e `actionResultDuration`. Origem e destino ficam destacados até a próxima seleção. O painel mantém a última ação do bot e um histórico de oito eventos, incluindo compras, movimentação, cura, danos, eliminações e invocações. A lista possui rolagem para caber em telas menores.

Validação adicional: 44 testes EditMode aprovados, incluindo dano da Valkíria, jogadas válidas em todas as dificuldades, preservação do estado durante simulação, ataque vencedor e prevenção de perda de Torre na resposta. Em Play Mode foram conferidos o seletor de dificuldade, a resposta do bot Difícil, os destaques e o histórico.

## Arte

Os 10 PNGs originais das classes estão em `Game/Assets/_Game/Sprites/Pieces/Resources/Classes`; os 10 Aseprite em `Game/Assets/_Game/Sprites/Pieces/ClassSources`. Os nomes originais foram preservados. As prévias não foram copiadas. O jogo carrega somente os PNGs; o Rei usa seu sprite padrão. Importação: Sprite Single, alpha do arquivo, transparência habilitada, Bilinear, sem mipmaps, sem compressão e sem redimensionamento NPOT. As artes de classe são dimensionadas proporcionalmente à casa.

## Verificação

34 testes EditMode passaram no Test Runner: 25 do clássico/tabuleiro e 9 novos do RPG. Cobrem renda única, defensor sobrevivente, morte do Rei, compra única e bloqueio do Rei, dano frontal, cura, catapulta estacionária, invocação do Troia e bônus restrito da Valkíria. Em Play Mode foram conferidos seleção, vida/custo no painel, bloqueio por saldo, jogada com resposta do bot e renda após a troca de turnos. A build executável externa e o playtest de balanceamento ainda precisam ser feitos.
