# Começar na Unity

Este repositório inicial é documentação. Não basta abri-lo na Unity: faltam `ProjectSettings`, `Packages`, cenas e assets de um projeto real.

## Antes de criar

Confirmar a versão Unity disponível no computador e qualquer versão exigida pelo professor. Não foi escolhida uma versão neste pacote.

Confirmar também vitória, turnos e formato local conforme [Regras](Regras.md). Não precisamos fechar todo o balanceamento para montar o tabuleiro.

## Primeiro passo

1. Criar um projeto 2D pela instalação disponível no Unity Hub.
2. Manter este `README.md`, a pasta `docs`, `.gitignore` e `.gitattributes` na raiz do projeto, junto de `Assets`, `Packages` e `ProjectSettings`.
3. Criar uma cena de partida e começar por 64 casas, coordenadas e seleção.
4. Usar peças provisórias identificáveis para todos os seis tipos.
5. Salvar cenas e assets pelo editor antes do primeiro commit Unity.

Ainda não há scripts para copiar. A próxima implementação deve entregar tabuleiro e seleção, sem adicionar economia ou habilidades na mesma tarefa.

## Arquivos que entram no Git

Manter `Assets`, seus arquivos `.meta`, `Packages` e `ProjectSettings`. O `.gitignore` exclui caches, builds e arquivos locais gerados.

Não apagar ou regenerar `.meta` manualmente; eles preservam referências. Renomear ou mover assets pelo editor sempre que possível. Não subir tokens, senhas ou configurações pessoais.

Não adicionar pacote ou ferramenta só porque aparece como possibilidade nesta documentação. Escolher as dependências quando necessárias e registrar a versão realmente usada.
