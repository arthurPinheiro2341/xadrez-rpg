# Começar na Unity

Este repositório inicial é documentação. Não basta abri-lo na Unity: faltam `ProjectSettings`, `Packages`, cenas e assets de um projeto real.

## Antes de criar

No print do Hub enviado em 03/10/2026, estão selecionados o editor **6000.3.7f1 LTS** e o template **Universal 2D**. Essa é a configuração proposta para a criação; ainda não há projeto Unity confirmado no GitHub. Qualquer versão exigida pelo professor tem prioridade.

Confirmar também vitória, turnos e formato local conforme [Regras](Regras.md). Não precisamos fechar todo o balanceamento para montar o tabuleiro.

## Primeiro passo

1. Na raiz do clone `xadrez-rpg`, executar `git pull --ff-only` para receber a configuração atualizada.
2. No Unity Hub, manter **Universal 2D**, preencher **Project name** com `Game` e **Location** com a pasta clonada `xadrez-rpg`. Deixar **Use Unity Version Control** desmarcado, pois o versionamento será pelo GitHub. O projeto será criado em `xadrez-rpg/Game`.

O README, `docs`, `.gitignore` e `.gitattributes` ficam na raiz do repositório. O projeto Unity fica em `Game`, com `Game/Assets`, `Game/Packages` e `Game/ProjectSettings`. As pastas de assets descritas na arquitetura são relativas a `Game`.

Depois de criar:

3. Criar uma cena de partida e começar por 64 casas, coordenadas e seleção.
4. Usar peças provisórias identificáveis para todos os seis tipos.
5. Salvar cenas e assets pelo editor antes do primeiro commit Unity.

Ainda não há scripts para copiar. A próxima implementação deve entregar tabuleiro e seleção, sem adicionar economia ou habilidades na mesma tarefa.

## Arquivos que entram no Git

Manter `Game/Assets`, seus arquivos `.meta`, `Game/Packages` e `Game/ProjectSettings`. O `.gitignore` exclui caches, builds e arquivos locais gerados.

Não apagar ou regenerar `.meta` manualmente; eles preservam referências. Renomear ou mover assets pelo editor sempre que possível. Não subir tokens, senhas ou configurações pessoais.

Não adicionar pacote ou ferramenta só porque aparece como possibilidade nesta documentação. Escolher as dependências quando necessárias e registrar a versão realmente usada.
