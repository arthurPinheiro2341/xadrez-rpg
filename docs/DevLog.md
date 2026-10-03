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

## Modelo para futuras entradas

Data; tarefa; arquivos ou commit; comportamento entregue; verificação feita; problemas restantes; próximo passo.

Não marcar gameplay concluído só porque um script existe. Registrar se foi compilado, testado em cena e verificado em build.
