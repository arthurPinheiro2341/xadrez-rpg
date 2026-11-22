# Roadmap

Sem datas inventadas. Ajustar ao calendário da disciplina quando as próximas entregas forem conhecidas.

## Estado atual

- [x] Consolidar proposta e decisões da conversa.
- [x] Preparar documentação inicial e configuração Git.
- [x] Criar e preencher o repositório remoto no GitHub.
- [ ] Confirmar versão Unity, regras pendentes e próximos prazos.
- [x] Preparar gerador das pastas de scripts, sprites, cenas, prefabs e dados.
- [x] Criar projeto Unity real.
- [x] Executar o gerador e confirmar as pastas e seus .meta no editor.

## Marco 1 — tabuleiro e seleção

- [x] Tabuleiro 8×8 com conversão entre coordenadas e posição.
- [x] Clique em casa e destaque visual da seleção.
- [ ] Representação dos seis tipos de peças e dois jogadores.
- [ ] Clique em peça e destaque de seleção.
- [ ] Ocupação de casas consistente.

Concluído quando cada peça está na casa correta e é selecionável sem alterar a partida.

## Marco 2 — movimentação e turnos

- [ ] Movimento e alcance de ataque por tipo.
- [ ] Casas válidas destacadas.
- [ ] Uma ação principal por turno, se a proposta for confirmada.
- [ ] Salto compartilhado e comandos bloqueados durante resolução.

Concluído quando movimentos legais funcionam e casas bloqueadas, fora do tabuleiro e peças do outro jogador são rejeitadas.

## Marco 3 — combate e partida

- [ ] Atributos e dano.
- [ ] Alvo vivo: atacante retorna sem mudar ocupação.
- [ ] Alvo morto: remoção e ocupação pelo atacante.
- [ ] Vitória por morte do Rei, se confirmada.
- [ ] Interface de turno, atributos e reinício.
- [ ] Decisão para Peão na última fileira implementada.

Concluído quando dois jogadores conseguem terminar e reiniciar uma partida.

## Marco 4 — economia e Cavalo de Troia

- [ ] Renda por turno e saldo individual.
- [ ] Compra validada; transformação sem recuperar vida indevidamente.
- [ ] Invocação em até duas casas válidas após a morte do Troia.
- [ ] Indicação de classe e custo na interface.

Concluído quando a evolução funciona com zero, uma ou duas casas disponíveis e peças invocadas pertencem ao lado correto.

## Marco 5 — Peão Ninja

- [ ] Fechar duração, ativação e colisão com peças ocultas.
- [ ] Definir privacidade no formato local.
- [ ] Separar visão de dono e adversário.
- [ ] Revelar no ataque sem duplicar ações.

Concluído quando ambos os jogadores entendem a habilidade e a ocupação permanece correta. Se o prazo impedir esse marco, registrar a redução de escopo antes da entrega.

## Marco 6 — apresentação e entrega

- [ ] Importar sprites aprovadas e distinguir os lados.
- [ ] Ajustar escala, pivô e legibilidade.
- [ ] Implementar feedback de impacto e dano.
- [ ] Realizar uma partida completa em build.
- [ ] Registrar bugs e testar os valores de vida/dano/economia.
- [ ] Preparar entrega conforme o enunciado da etapa correspondente.

## Checklist de verificação manual

| Caso | Resultado esperado |
|---|---|
| Clique repetido durante salto | Uma ação e um dano |
| Peça aliada no destino | Movimento/ataque rejeitado |
| Torre/Bispo/Rainha atrás de bloqueio | Sem destino atravessando ocupação |
| Cavalo com bloqueios no caminho | Movimento em L permitido se destino válido |
| Ataque sem abate | Vida reduzida; atacante na origem |
| Ataque com abate | Alvo removido; atacante no destino |
| Compra sem dinheiro | Sem alterações de classe ou saldo |
| Início de turno | Renda creditada exatamente uma vez |
| Troia cercado | Invoca somente onde houver espaço |
| Rei derrotado | Partida termina e comandos bloqueiam |
| Reinício | Tabuleiro, vida, saldo e turnos restaurados |

Esses casos são critérios futuros. Não foram executados porque ainda não há implementação.
