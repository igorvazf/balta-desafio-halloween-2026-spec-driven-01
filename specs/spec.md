# Gerador de Senhas Fortes

## Problema
Criar senhas fortes é trabalhoso e não há uma forma simples de armazená-las e recuperá-las após geradas.

## Objetivo
Disponibilizar uma API simples que gere senhas fortes seguindo um padrão fixo, armazene cada senha gerada e permita consultá-la depois por meio de um GUID.

## Usuários
Desenvolvedores que podem consumir a API localmente para testes e aplicações que podem adicionar essa funcionalidade.

## Histórias
- Como consumidor da API, quero gerar senhas fortes sem precisar criá-las manualmente
- Como consumidor da API, quero receber um GUID ao gerar a senha para poder consultá-la
- Como consumidor da API, quero consultar uma senha a partir do GUID para recuperá-la quando precisar
- Como consumidor da API, quero receber um erro claro quando o GUID for inválido ou não existir

## Requisitos funcionais
- RF01: `POST /passwords` gera uma senha, armazena no banco de dados e retorna `201 Created` com o GUID e a senha gerada
- RF02: `GET /passwords/{id}` retorna `200 OK` com a senha associada ao GUID
- RF03: `GET /passwords/{id}` com GUID inexistente retorna `404` com ProblemDetails
- RF04: `GET /passwords/{id}` com valor que não seja um GUID válido retorna `404` com ProblemDetails
- RF05: Toda senha gerada é persistida em SQLite via EF Core
- RF06: O GUID é gerado pelo servidor; o cliente não pode definir o identificador
- RF07: Toda resposta de erro (404 e 500) tem corpo no formato ProblemDetails, inclusive quando a rota não casa (ex.: `/passwords/abc`)

## Regras de negócio
- RN01: A senha tem no mínimo 16 caracteres
- RN02: A senha tem no máximo 128 caracteres
- RN03: A senha contém ao menos uma letra maiúscula, uma minúscula, um dígito e um caractere especial
- RN04: A senha não contém espaços em branco
- RN05: Cada chamada a `POST /passwords` gera uma nova senha e um novo GUID, sem reaproveitar registros
- RN06: A senha é armazenada em texto puro
- RN07: A API tem exatamente dois endpoints; qualquer endpoint novo exige alterar a constituição

## Casos de borda
- GUID bem formado, mas inexistente: 404
- GUID malformado na rota (ex.: `/passwords/abc`): 404
- GUID vazio (`00000000-0000-0000-0000-000000000000`): 404
- Muitas gerações seguidas: cada senha continua válida e com GUID único
- Senha gerada sem alguma das classes obrigatórias não pode ser aceita; o gerador deve garantir as quatro classes em toda geração
- Falha ao gravar no banco: não retornar GUID nem senha, e responder com erro `500` em ProblemDetails

## Fora de escopo
- Autenticação e autorização
- Criptografia ou hash das senhas armazenadas
- Parâmetros de geração (tamanho, conjunto de caracteres)
- Atualização, exclusão ou listagem de senhas
- Expiração ou consumo único das senhas
- Uso em produção

## Critérios de aceite
- `POST /passwords` retorna 201 com um GUID válido e uma senha
- A senha retornada tem 16 ou mais caracteres, inclui maiúscula, minúscula, dígito e caractere especial, e não tem espaços
- A senha retornada não tem mais de 128 caracteres
- `GET /passwords/{id}` com um GUID retornado pelo POST devolve a mesma senha
- `GET /passwords/{id}` com GUID inexistente retorna 404 com ProblemDetails
- `GET /passwords/{id}` com valor malformado retorna 404 com ProblemDetails
- A aplicação compila sem erros ou warnings
- Todos os testes passam
- A aplicação roda sem erros
