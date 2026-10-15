# Constituição do projeto

## Stack
- .NET 8, C#, Minimal API
- Persistência em SQLite com EF Core
- Testes com xUnit

## Arquitetura
- Endpoint não acessa banco diretamente, sempre via serviço de aplicação
- Regra de negócio não vive no endpoint nem no handler HTTP
- A API tem exatamente dois endpoints: `POST /passwords` (gera, armazena e retorna o GUID) e `GET /passwords/{id:guid}` (retorna a senha associada ao GUID)
- Toda senha gerada tem no mínimo 16 caracteres, contém ao menos uma letra maiúscula, uma minúscula, um dígito e um caractere especial, e não contém espaços em branco
- Nenhuma dependência nova entra sem justificativa escrita no plano

## Qualidade
- Toda regra de negócio precisa ter teste unitário
- Todo endpoint precisa ter ao menos um teste de integração de caminho feliz
- GUID inexistente retorna 404 com ProblemDetails e tem teste de integração
- Erro de validação sempre retorna 400 com ProblemDetails

## Convenções
- O código deve ser em inglês, seguindo as convenções de nomenclatura do C#
- Os endpoints da API devem seguir o padrão REST
- Não encriptar senhas ou outros recursos de segurança
- Commits no formato Conventional Commits (`feat:`, `fix:`, `test:`, `docs:`)

## Governança
- Leia e respeite o `constitution.md` antes de qualquer alteração
- Esse arquivo de constituição não deve ser alterado pelo agente de IA
- Nunca commitar direto na `main`: crie uma branch por tarefa
- Ao commitar e fazer push, abra um pull request com descrição do que mudou e como foi testado
- A senha gerada é apenas para fins didáticos e não deve ser utilizada em ambiente de produção
