# Documento de design — Xadrez RPG

Versão 0.1 da documentação — 01/10/2026. Autor do projeto: Arthur Pinheiro.

## Proposta

Transformar o xadrez em uma batalha por turnos com atributos, habilidades e escolhas de evolução. Duas partidas podem começar com as mesmas peças e terminar com composições diferentes conforme o investimento de cada jogador.

## Experiência desejada

O jogador observa o tabuleiro, escolhe onde investir suas moedas e decide entre avançar, atacar ou preservar uma unidade. Precisa reconhecer a peça original e entender como sua classe altera seu comportamento.

O combate com vida permite que uma peça resista a vários ataques. Isso muda bastante o ritmo do xadrez: precisamos testar se os jogadores continuam avançando ou apenas repetem ataques sem decisões interessantes.

## Pilares

1. **Identidade de xadrez:** peças reconhecíveis e movimentação baseada na peça original.
2. **Evoluções com função:** transformar uma peça abre uma alternativa de jogo.
3. **Informação legível:** vida, dano, moedas, casas válidas e classes fáceis de consultar.
4. **Produção simples:** sprites estáticos, saltos compartilhados e poucas habilidades.

## Ciclo de partida proposto

Iniciar turno e receber moedas → consultar peças → comprar uma evolução opcional → mover ou atacar com uma peça → resolver dano e habilidades → verificar fim da partida → trocar jogador.

Quantidade de compras, ações e moedas por turno ainda são propostas; [Regras](Regras.md) registra as opções iniciais.

## Conteúdo da primeira versão proposta

| Sistema | Conteúdo |
|---|---|
| Tabuleiro | 8×8, casas e ocupação |
| Peças | Peão, Torre, Cavalo, Bispo, Rainha e Rei |
| Combate | Vida, dano fixo, morte e ocupação de casas |
| Economia | Moedas recebidas no começo do próprio turno |
| Evoluções | Peão Ninja e Cavalo de Troia |
| Interface | Jogador atual, moedas, atributos, seleção, compras e vitória |
| Feedback | Salto, impacto, tremida e número de dano |
| Partida | Dois jogadores locais; formato de privacidade a definir |

## Limites recomendados para a entrega

Não planejar multiplayer online, IA adversária, campanha, baralho completo, coleção, dezenas de classes, animações de ataque individuais ou sistema amplo de equipamentos. A inspiração em cartas não exige implementar um sistema de cartas.

O primeiro marco é uma partida curta com peças comuns; o segundo é provar economia e evolução com o Cavalo de Troia. O Ninja entra quando houver regra clara de informação oculta.

## Exemplos de variantes

| Variante | Conceito | Estado |
|---|---|---|
| Peão Ninja | Invisível para o adversário; reaparece ao atacar | Previsto; regras pendentes |
| Cavalo de Troia | Movimento de Cavalo; gera dois peões ao morrer | Previsto; casas de nascimento pendentes |
| Cavalo Fantasma / morto-vivo | Direção de arte discutida anteriormente | Fora do escopo inicial; habilidade não definida |
| Peão Escudeiro | Mais resistência como exemplo de alternativa | Ideia futura |
| Torre de Cerco | Mais dano com limitação de movimento | Ideia futura |
| Bispo Necromante | Relação com peças derrotadas | Ideia futura |

O Cavalo comum já salta sobre peças no xadrez. Uma habilidade de “atravessar peças” sozinha não o diferencia; o Fantasma precisa de outro efeito caso seja desenvolvido.

## Critério de sucesso

Conseguir concluir uma partida, entender de quem é o turno, selecionar e executar apenas ações válidas, comprar uma evolução e perceber seu efeito. A apresentação deve continuar clara durante e depois dos saltos.

Datas das próximas entregas, versão Unity e duração desejada de partida não foram informadas. Definir esses dados antes de aumentar o escopo.
