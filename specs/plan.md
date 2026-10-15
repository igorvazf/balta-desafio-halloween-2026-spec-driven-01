# Plano técnico

## Contexto
API didática em .NET 8 (Minimal API) com dois endpoints: `POST /passwords`, que gera uma senha forte, armazena em SQLite via EF Core e retorna o GUID e a senha, e `GET /passwords/{id:guid}`, que devolve a senha associada ao GUID. Este plano segue `constitution.md` e `spec.md`.

## Arquitetura
Estrutura de pastas
```
src/
├── PasswordApi/
│ ├── Program.cs
│ ├── Endpoints/
│ │ └── PasswordEndpoints.cs
│ ├── Services/
│ │ ├── IPasswordService.cs
│ │ ├── PasswordService.cs
│ │ ├── IPasswordGenerator.cs
│ │ └── PasswordGenerator.cs
│ ├── Data/
│ │ ├── AppDbContext.cs
│ │ └── PasswordEntry.cs
│ ├── Contracts/
│ │ └── PasswordResponse.cs
│ └── Migrations/
└── tests/PasswordApi.Tests/
  ├── PasswordGeneratorTests.cs
  └── PasswordEndpointsTests.cs
```
Fluxo: endpoint → `IPasswordService` → `IPasswordGenerator` e `AppDbContext`. O endpoint nunca acessa o banco diretamente, e as entidades do EF não saem da camada de serviço (o serviço devolve `PasswordResponse`).

## Decisões
- **Tamanho da senha**: entre 16 e 128 caracteres, definido aleatoriamente.
- **Algoritmo do gerador**: sorteia um tamanho para a senha, sorteia um caractere de cada classe (maiúscula, minúscula, dígito, especial), completa o restante a partir do conjunto combinado e embaralha o resultado (Fisher-Yates). O sorteio usa `RandomNumberGenerator.GetInt32`, que já faz parte do .NET e não exige dependência nova.
- **Caracteres especiais permitidos**: `!@#$%^&*()-_=+[]{}<>?`. Nenhum espaço ou caractere de controle entra em nenhum dos conjuntos.
- **GUID**: gerado no servidor com `Guid.NewGuid()` no serviço e usado como chave primária.
- **Rota**: `{id:guid}` no GET. Valor malformado não casa a rota e resulta em 404.
- **ProblemDetails**: `AddProblemDetails()` + `UseStatusCodePages()` para que o 404 de rota não casada também tenha corpo ProblemDetails, e `UseExceptionHandler()` para que falhas de gravação retornem 500 em ProblemDetails sem expor GUID nem senha.
- **Migrations**: versionadas no repositório e aplicadas na inicialização com `Database.Migrate()`.
- **Dependências** (justificativa exigida pela constituição):
  - `Microsoft.EntityFrameworkCore.Sqlite`: exigido pela stack (SQLite com EF Core)
  - `Microsoft.EntityFrameworkCore.Design` (somente desenvolvimento): necessário para criar migrations
  - `Microsoft.AspNetCore.Mvc.Testing`: permite testes de integração com `WebApplicationFactory`
  - `xUnit` e templates de teste padrão: exigidos pela stack
  - `Swashbuckle.AspNetCore`: gera a documentação OpenAPI e a interface Swagger UI, que é o template padrão do .NET 8 para Minimal APIs
  - Nenhuma outra dependência (sem FluentValidation ou bibliotecas de geração de senha)
- **Swagger**: `AddEndpointsApiExplorer()` e `AddSwaggerGen()` no registro de serviços; `UseSwagger()` e `UseSwaggerUI()` habilitados apenas em ambiente de desenvolvimento. Os endpoints declaram suas respostas com `.Produces<PasswordResponse>(...)` e `.ProducesProblem(...)` (404 e 500), além de `.WithName()` e `.WithSummary()`, para que o Swagger mostre sucesso e erros em ProblemDetails.

## Modelo de dados
Tabela `Passwords`, entidade `PasswordEntry`:

| Coluna | Tipo | Restrições |
|---|---|---|
| `Id` | `Guid` (TEXT no SQLite) | PK, gerado pelo servidor |
| `Value` | `string` | obrigatório, máximo 128 |
| `Date` | `DateTime` | obrigatório, data em UTC |

- Connection string em `appsettings.json` (`Data Source=passwords.db`)
- Os testes de integração usam SQLite em memória (conexão mantida aberta durante o teste), nunca o banco de desenvolvimento
- O arquivo `*.db` entra no `.gitignore`

## Contratos
A documentação interativa fica em `/swagger` (somente em desenvolvimento), com o JSON OpenAPI em `/swagger/v1/swagger.json`.

Resposta de sucesso (`PasswordResponse`, um `record`):

```json
{ "id": "3f2b8c1e-5d4a-4e7b-9a10-6c2d8e0f1a23", "password": "k#9Vx!2mQpL@7zRt" }
```

| Endpoint | Sucesso | Erros |
|---|---|---|
| `POST /passwords` (sem corpo) | `201 Created`, header `Location: /passwords/{id}`, corpo `PasswordResponse` | `500` ProblemDetails se a gravação falhar |
| `GET /passwords/{id:guid}` | `200 OK`, corpo `PasswordResponse` | `404` ProblemDetails se o GUID não existir, for vazio (`0000...`) ou malformado; `500` ProblemDetails em falha inesperada |

## Riscos
- **Texto puro**: qualquer pessoa com acesso ao arquivo `.db` lê todas as senhas. Aceito por ser projeto didático; não usar em produção.
- **GUID como único controle de acesso**: sem autenticação, quem tiver o GUID lê a senha (fora de escopo).
- **404 sem corpo**: se `UseStatusCodePages()` não for configurado, rotas não casadas voltam sem ProblemDetails. Cobrir com teste de integração para `/passwords/abc`.
- **Vazamento em logs**: a senha não deve ser logada pela aplicação. Manter `EnableSensitiveDataLogging` desligado, pois o EF pode registrar valores de parâmetros.
- **Gerador sem uma das classes**: coberto por testes unitários que rodam o gerador em muitas iterações (por exemplo, 10 mil) validando todas as regras.
- **Falha de gravação**: o serviço só devolve GUID e senha depois de `SaveChangesAsync` concluir com sucesso.
- **Swagger exposto**: a interface não deve ficar habilitada fora de desenvolvimento. Aceito por ser projeto didático, mas o `if (app.Environment.IsDevelopment())` deve ser mantido.