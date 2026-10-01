# Regras de jogo

As regras abaixo distinguem decisões aceitas de propostas para o protótipo. Nenhuma foi validada em playtest ainda.

## Já definido

- Cada peça tem vida e dano; um ataque pode não derrotar o alvo.
- Peças podem mudar de classe durante a partida mediante compra.
- Cada jogador recebe recursos por turno.
- A peça salta para mover e atacar.
- Quando o alvo sobrevive, o atacante volta à origem.
- Quando o alvo morre, o atacante ocupa a casa dele.
- Duas peças não podem ocupar permanentemente a mesma casa.
- Peão Ninja e Cavalo de Troia são os exemplos centrais de evolução.

## Proposta para testar primeiro

| Tema | Regra proposta |
|---|---|
| Jogadores | Dois jogadores locais alternando turnos |
| Tabuleiro | 8×8; formação inicial baseada no xadrez |
| Vitória | Reduzir a vida do Rei inimigo a zero |
| Ação principal | Uma movimentação ou um ataque por turno |
| Evolução | No máximo uma compra antes da ação principal |
| Dinheiro inicial | 0 moedas; primeiro recebimento no início do primeiro turno de cada jogador |
| Renda | +2 moedas por turno próprio, acumuladas sem limite no primeiro teste |
| Contra-ataque | Não existe automático no primeiro protótipo |
| Ataque | Dano fixo, sem sorte, armadura ou crítico |
| Evolução de atributos | Preservar o dano já sofrido; vida atual = máximo(1, nova vida máxima − dano sofrido) |
| Repetir evolução | Uma evolução por peça; sem trocar novamente de classe na primeira versão |

Essa proposta favorece implementação simples. Precisa ser confirmada e testada; não é uma exigência do professor.

## Movimento e ataque propostos

O destino vazio permite movimento. Um destino ocupado por inimigo permite ataque se estiver no alcance de ataque da peça. Uma peça aliada bloqueia o destino.

| Peça | Movimento básico | Ataque básico |
|---|---|---|
| Peão | Uma casa à frente, vazia | Uma casa diagonal à frente |
| Torre | Linha ou coluna | Linha ou coluna |
| Bispo | Diagonal | Diagonal |
| Rainha | Linha, coluna ou diagonal | Linha, coluna ou diagonal |
| Cavalo | Deslocamento em L; salta bloqueios | Deslocamento em L |
| Rei | Uma casa em qualquer direção | Uma casa em qualquer direção |

Torre, Bispo e Rainha não atravessam casas ocupadas; o primeiro inimigo no trajeto pode ser alvo. O salto é apenas apresentação visual e não autoriza atravessar bloqueios lógicos.

Cada lado tem sua própria direção de avanço dos peões. Roque, en passant, avanço duplo inicial, promoção, xeque e xeque-mate não entram nessa proposta mínima. A consequência de um Peão chegar à última fileira precisa ser escolhida antes do teste completo; recomendação inicial: promoção gratuita a uma peça comum escolhida na interface, sem conservar classe Ninja. Isso adiciona trabalho, portanto só fechar após testar a base.

## Atributos de partida sugeridos

Valores ilustrativos da conversa, ainda sem balanceamento:

| Peça | Vida máxima | Dano |
|---|---:|---:|
| Peão | 3 | 1 |
| Cavalo | 6 | 3 |
| Bispo | 5 | 3 |
| Torre | 7 | 4 |
| Rainha | 8 | 5 |
| Rei | 15 | 2 |

Peão Ninja: exemplo de 2 de vida, 2 de dano e custo 3. Cavalo de Troia: exemplo de 5 de vida, 2 de dano e custo 5. Esses exemplos são sugestões para testar, sem aprovação definitiva.

## Resolução do combate

1. Validar jogador, origem, destino, alcance e estado da partida.
2. Bloquear novos comandos durante a resolução.
3. Animar salto até o alvo e apresentar o impacto.
4. Aplicar o dano uma única vez.
5. Se o alvo sobreviver, voltar o atacante à origem; ocupação lógica permanece igual.
6. Se o alvo morrer, removê-lo e transferir o atacante para o destino.
7. Resolver a habilidade de morte, considerando a ocupação após a transferência.
8. Verificar vitória e, se a partida continuar, avançar o turno e liberar comandos.

Nenhum clique durante um salto deve causar dano extra ou trocar o jogador duas vezes. A casa do alvo pode conter sobreposição visual temporária, mas nunca duas peças no estado lógico.

## Cavalo de Troia

Conceito: ao morrer, cria até dois peões aliados próximos.

Regra proposta para evitar ambiguidade: usar casas vazias entre as oito vizinhas da casa da morte, em uma ordem fixa documentada no código. Excluir a casa ocupada pelo atacante. Se houver só uma disponível, criar um Peão; se não houver nenhuma, não criar nenhum. Não remover outras peças para abrir espaço.

Peões invocados pertencem ao jogador do Cavalo de Troia e só agem em um turno futuro desse jogador. Não têm promoção de classe automática. Se o Rei for derrotado, a partida termina e não se processam novas ações.

## Peão Ninja

Conceito aceito: ocultar a peça do adversário e revelá-la quando atacar. Duração, ativação, custo e regras de colisão estão abertos.

Problema a resolver: esconder a sprite em uma tela compartilhada também a esconde do dono ou revela informações quando se alterna jogador. Um simples fade não implementa privacidade.

Proposta: turno com tela de passagem entre jogadores; visão do dono mostra o Ninja com indicação de furtividade; visão do adversário o oculta. Essa solução depende de os jogadores respeitarem a passagem de turno e tem custo de interface. Outra possibilidade é adiar a invisibilidade real e apresentar a classe como demonstração separada.

Antes de implementar, escolher: duração; se mover quebra furtividade; como selecionar a peça oculta; como resolver tentativa de entrar em sua casa; se bloquear trajetos revela sua posição; quando o adversário recebe indicação de uma peça escondida. Até lá, não prometer que a habilidade está pronta.

## Decisões necessárias antes de programar a partida completa

- Confirmar vitória pela morte do Rei e ausência de regras de xeque.
- Confirmar ação por turno, compra e economia.
- Confirmar formato local e solução do Ninja.
- Definir promoção na última fileira.
- Definir duração desejada de partida para ajustar vida e dano.
