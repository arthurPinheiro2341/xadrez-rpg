# Configuração do GitHub

## Repositório criado

| Campo | Valor proposto |
|---|---|
| Conta | `arthurPinheiro2341`, verificada pela conexão do GitHub |
| Nome | `xadrez-rpg` |
| Endereço | [arthurPinheiro2341/xadrez-rpg](https://github.com/arthurPinheiro2341/xadrez-rpg) |
| Descrição | Projeto de Jogos da UFOP: xadrez com vida, dano, classes e evoluções por turno. |
| Visibilidade inicial | Privado, até Arthur decidir publicar |
| Branch principal | `main` |
| Licença | Nenhuma licença adicionada nesta etapa |

**Estado:** criado como privado em 01/10/2026 e preenchido com a documentação inicial. Ainda não contém projeto Unity completo.

## Trabalhar no computador

Clonar o repositório pela conta com acesso, usando GitHub Desktop ou Git. Pelo terminal, depois de configurar a autenticação:

```bash
git clone https://github.com/arthurPinheiro2341/xadrez-rpg.git
```

O histórico remoto é a fonte dos commits. Usar o clone para continuar o trabalho, em vez de inicializar um Git separado a partir do ZIP antigo. Depois adicionar `Assets`, `Packages` e `ProjectSettings` do projeto real conforme [Começar na Unity](ComecarNaUnity.md).

## Fluxo de trabalho sugerido

Para cada tarefa: partir de `main` atualizada, criar uma branch pequena (`feat/board-selection`, por exemplo), implementar, verificar, fazer commit, revisar as alterações e integrar. Evitar uma branch única acumulando todos os sistemas.

Commits de exemplo: `docs: registrar regras iniciais`; `feat: criar seleção de peças`; `fix: bloquear ação durante salto`.

O template de tarefa registra objetivo e conclusão. O template de pull request registra comportamento, validação e pendências. Não é necessário criar dezenas de issues antes de implementar o primeiro marco.

## Registro de mudanças

Manter README, [Roadmap](Roadmap.md), [Decisões](Decisoes.md) e [DevLog](DevLog.md) de acordo com o que realmente foi implementado e verificado.
