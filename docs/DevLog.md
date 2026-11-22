# DevLog

## 01/10/2026 — documentação inicial

**Realizado:** consolidação da ideia, distinção entre regras aceitas e propostas, plano de implementação, arquitetura sugerida, diretrizes de sprites, texto da etapa 2 e configuração inicial de Git.

**Estado:** pacote de documentação preparado localmente. Criação e envio ao GitHub ainda pendentes. Nenhum sistema Unity implementado ou testado.

**Evidência:** arquivos Markdown e configurações deste pacote. O enunciado anexado foi lido e preservado em `Referencias/enunciado-etapa2.png`.

**Próximo passo:** concluir criação remota e confirmar regras essenciais; depois criar tabuleiro e seleção na Unity.

## 01/10/2026 — criação do GitHub

**Realizado:** criação do repositório privado [arthurPinheiro2341/xadrez-rpg](https://github.com/arthurPinheiro2341/xadrez-rpg) na conta de Arthur e envio dos 16 arquivos de documentação e configuração. README, Roadmap e registro de decisões atualizados para refletir essa etapa.

**Estado:** documentação versionada no GitHub. Ainda não há implementação Unity, sprites jogáveis neste repositório ou build.

**Verificação:** links locais, configuração de arquivos ignorados e integridade do pacote conferidos antes do envio. Arquivos publicados na branch `main`.

**Próximo passo:** confirmar versão Unity e regras essenciais; iniciar o marco de tabuleiro e seleção.

## 03/10/2026 — preparação da criação na Unity

**Evidência:** Arthur informou que clonou o repositório e enviou a tela de criação do Hub, com editor 6000.3.7f1 LTS e template Universal 2D selecionados.

**Realizado:** configuração Git ajustada para ignorar caches e builds também dentro de subpastas; documentação alinhada ao projeto Unity planejado em `Game`.

**Estado:** criação ainda em preparação no computador de Arthur. Nenhuma cena, script ou build recebida ou testada nesta etapa.

**Próximo passo:** atualizar o clone, criar `Game` pela configuração indicada e confirmar sua abertura no editor.

## 03/10/2026 — gerador da organização de pastas

**Realizado:** preparação de `Tools/CreateProjectStructure.cmd`, que cria 19 pastas finais dentro de `Game/Assets/_Game` para scripts, sprites dos seis tipos de peças, tabuleiro, interface, cenas, prefabs e dados. A pasta do Cavalo segue `Horse`, como no esquema fornecido por Arthur.

**Comportamento:** exige um projeto Unity já criado; preserva arquivos existentes; usa `.gitkeep` para rastrear pastas vazias; não cria arquivos `.meta` manualmente.

**Verificação:** lista de pastas e correspondência com a arquitetura conferidas; gerador revisado estaticamente. Não foi executado em Windows ou Unity neste ambiente.

**Estado:** gerador pronto no repositório; sua execução no computador de Arthur e a importação no editor ainda estão pendentes. Nenhuma implementação de gameplay foi acrescentada.

## 22/11/2026 — projeto Unity e tabuleiro 8×8

**Visual:** integrado `tabuleiro_medieval.aseprite` como fundo do tabuleiro. As 64 casas permanecem como áreas de clique transparentes e a seleção usa um destaque semitransparente sobre a arte.

**Peças padrão:** integrados os 12 PNGs medievais de Peão, Torre, Cavalo, Bispo, Rainha e Rei nas cores clara e escura, preservando os 12 arquivos `.aseprite` correspondentes. A cena mostra a formação inicial clássica com 32 instâncias puramente visuais, sem adicionar movimentos, combate ou classes evoluídas.

**Realizado:** criação do projeto `Game` em Unity 6000.6.10f1 com Universal 2D; execução do gerador de pastas; criação da cena `Assets/_Game/Scenes/Board.unity`; implementação de coordenadas algébricas, geração das 64 casas e seleção visual por clique.

**Arquitetura:** `BoardCoordinate` converte entre arquivo/fileira, notação algébrica e índice visual; `BoardView` gera e consulta as casas; `BoardTileView` encaminha o clique e aplica o destaque. A ferramenta `Xadrez RPG/Create Board Scene` recria a cena inicial e a coloca primeiro no Build Settings.

**Verificação:** projeto compilado sem erros; as 32 peças foram conferidas visualmente em Play Mode, mantendo proporção, transparência, centralização e base apoiada na casa. Os 16 testes EditMode passaram no Unity Test Runner, cobrindo notação, entradas inválidas, orientação do tabuleiro, geração de 64 casas, seleção e configuração visual das peças.

**Estado:** tabuleiro, seleção de casas e formação visual padrão concluídos. Ainda não há ocupação lógica, movimento ou combate.

**Próximo passo:** associar as peças visuais ao estado lógico da partida e implementar seleção de peça sem alterar as regras planejadas.

## 22/11/2026 — xadrez clássico jogável

**Realizado:** implementada uma partida local para duas pessoas com ocupação lógica, seleção de peça, destinos legais, alternância de turnos, captura, xeque, xeque-mate, afogamento, roque, en passant e promoção para Rainha, Torre, Bispo ou Cavalo. A interface informa o jogador da vez, diferencia movimentos e capturas por cor e permite reiniciar a partida.

**Arquitetura:** `ChessGame` concentra as regras sem depender da interface; `ChessGameController` conecta cliques, peças visuais e textos; `ChessMove` descreve jogadas comuns e especiais. Essa separação preserva o núcleo para a futura camada de vida, dinheiro e classes.

**Verificação:** 25 testes EditMode passaram no Unity Test Runner. Uma sequência manual em Play Mode confirmou seleção, avanço duplo de Peão, alternância de turno, destaque de captura, remoção da peça capturada e reinício da formação.

**Fora desta etapa:** bot, animação de salto, vida, dano, dinheiro, classes e evoluções continuam não implementados.

## Modelo para futuras entradas

Data; tarefa; arquivos ou commit; comportamento entregue; verificação feita; problemas restantes; próximo passo.

Não marcar gameplay concluído só porque um script existe. Registrar se foi compilado, testado em cena e verificado em build.
