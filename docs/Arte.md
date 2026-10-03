# Arte e apresentação

## Direção definida

Sprites 2D de peças de xadrez com aparência de fantasia e silhueta reconhecível. Cada arquivo contém uma única peça, com fundo transparente e enquadramento consistente. Não combinar Cavalo e tabuleiro no mesmo arquivo.

Manter o ângulo, a escala da base, a iluminação e a espessura do contorno entre as peças. A direção exata deve usar uma sprite aprovada como referência quando seu arquivo estiver disponível.

As sprites anteriores foram aprovadas na conversa, mas seus arquivos não foram fornecidos neste pacote. O anexo atual é o enunciado da atividade, não uma sprite. Não há assets jogáveis anexados ao repositório inicial.

## Inventário planejado

| Arquivo | Prioridade | Situação neste pacote |
|---|---|---|
| `pawn_normal.png` | Base | Não disponível |
| `horse_normal.png` | Base | Não disponível |
| `bishop_normal.png` | Base | Não disponível |
| `rook_normal.png` | Base | Não disponível |
| `queen_normal.png` | Base | Não disponível |
| `king_normal.png` | Base | Não disponível |
| `pawn_ninja.png` | Evolução | Não disponível |
| `horse_trojan.png` | Evolução | Não disponível |
| `board_basic.png` | Opcional | Não disponível |
| `coin_icon.png`, `health_icon.png`, `damage_icon.png` | Interface | Não disponíveis |
| `piece_shadow.png` | Opcional | Não disponível |

Para testar, usar formas simples e letras representando todos os seis tipos de peças. Deixar Torre, Bispo e Rainha sem representação prejudicaria o teste de uma partida com formação completa.

O tabuleiro pode ser montado com 64 casas coloridas na Unity; isso facilita destacar destinos e dispensa uma imagem grande no início.

## Preparação para importar

Proposta: PNG em canvas quadrado de 512×512, transparência real e base perto do centro inferior, sem cortar a peça. Usar os mesmos parâmetros de escala e pivô para todas as sprites. Confirmar tamanho e estilo a partir dos arquivos reais antes de fechar a importação.

Separar os dois lados por contorno, marcador ou paleta que continue legível em casas claras e escuras. Alterar apenas a cor do objeto pode não funcionar bem em uma imagem muito colorida.

## Movimento e combate aceitos

- Movimento: pequeno arco até o centro da casa de destino.
- Ataque: salto até o inimigo e impacto.
- Alvo vivo: retorno à origem.
- Alvo derrotado: remoção e atacante no destino.
- Todas as peças reutilizam a animação de salto.

Feedback adicional proposto: tremida curta no alvo, texto de dano e sombra oval. A sombra acompanha a projeção da peça no tabuleiro e fica menor quando a peça sobe; ela não sobe junto com a sprite.

Não há necessidade de sprites de caminhada ou vários quadros de ataque. O salto desloca o objeto; o desenho permanece estático.

## Habilidades

Troia: remover Cavalo e apresentar peões invocados com um salto curto, respeitando o resultado lógico.

Ninja: fade na representação e sinal visual para o dono. O fade não resolve sozinho a informação oculta; a interface depende das regras em [Regras](Regras.md).
