# Xadrez RPG

Projeto de curso de Design e Desenvolvimento de Jogos — Arthur Pinheiro.

Repositório privado: [arthurPinheiro2341/xadrez-rpg](https://github.com/arthurPinheiro2341/xadrez-rpg).

Um jogo de estratégia por turnos baseado no xadrez, em que as peças têm vida, dano e classes. Durante a partida, o jogador recebe moedas e pode transformar suas peças em variantes com habilidades diferentes.

**Estado em 22/11/2026:** projeto Unity criado com Universal 2D e primeiro marco iniciado. A cena `Board` já apresenta um tabuleiro 8×8 com coordenadas e seleção visual de casas. Ainda não há peças, movimentação, combate ou build jogável; os valores numéricos continuam sendo propostas de teste.

## A ideia

As peças começam em suas versões normais. Um Peão pode virar Peão Ninja e ficar invisível para o adversário até atacar; um Cavalo pode virar Cavalo de Troia e gerar dois peões aliados ao morrer. A evolução deve mudar a forma de jogar, com vantagens e limitações, em vez de apenas aumentar atributos.

O visual preserva a silhueta de peças de xadrez. Cada sprite é um arquivo separado com fundo transparente. A movimentação e o combate usam pequenos saltos do objeto na Unity, sem animações complexas por personagem.

## Decisões já aceitas

- Nome provisório: **Xadrez RPG**.
- Peças com vida, dano, habilidades e variantes compradas com moedas.
- Recursos recebidos por turno.
- Movimentação e ataques representados por saltos.
- Se o alvo sobreviver, o atacante volta à casa de origem.
- Se o alvo morrer, o atacante ocupa sua casa.
- Uma peça por imagem, com identidade visual de xadrez.
- Escopo pequeno, adequado ao tempo da disciplina.

## Primeira versão proposta

Tabuleiro 8×8, seis tipos de peças, dois jogadores locais, combate por turnos, dinheiro e duas evoluções: Peão Ninja e Cavalo de Troia. A condição de vitória proposta é derrotar o Rei; xeque e xeque-mate tradicionais ficam fora dessa proposta.

O formato local e a regra de vitória ainda precisam ser confirmados. A invisibilidade do Ninja também precisa de uma solução para ocultar informações entre jogadores na mesma tela. Veja [Regras](docs/Regras.md).

## Documentação

| Arquivo | Conteúdo |
|---|---|
| [GDD](docs/GDD.md) | Proposta, experiência e limites de escopo |
| [Regras](docs/Regras.md) | Movimento, combate, turnos, economia e dúvidas abertas |
| [Arquitetura](docs/Arquitetura.md) | Responsabilidades dos sistemas e pastas Unity planejadas |
| [Arte](docs/Arte.md) | Sprites, padronização e feedback dos saltos |
| [Roadmap](docs/Roadmap.md) | Ordem de implementação e critérios de conclusão |
| [Decisões](docs/Decisoes.md) | O que foi aceito e o que é proposta |
| [DevLog](docs/DevLog.md) | Registro do trabalho realmente realizado |
| [Começar na Unity](docs/ComecarNaUnity.md) | Primeiro passo de implementação |
| [Configurar GitHub](docs/GitHub.md) | Criação do repositório e primeiro envio |
| [Entrega da etapa 2](docs/EntregaEtapa2.md) | Texto para revisar e postar no fórum |

## Referências

Xadrez tradicional; Hearthstone, principalmente Campos de Batalha, como inspiração para recursos e decisões de compra por turno; Fire Emblem, como inspiração para unidades com classes e atributos. São referências de design, sem compromisso de reproduzir suas regras.

## Como acompanhar o trabalho

Use [Roadmap](docs/Roadmap.md) para escolher a próxima tarefa e [DevLog](docs/DevLog.md) para registrar a conclusão. Sugestão de fluxo: uma tarefa pequena por branch, commit, revisão e merge em `main`.

O projeto ainda não inclui sprites definitivos nem os sistemas de peças, movimentação, combate e evolução. As imagens geradas anteriormente foram discutidas na conversa, mas seus arquivos não estão presentes neste pacote.
