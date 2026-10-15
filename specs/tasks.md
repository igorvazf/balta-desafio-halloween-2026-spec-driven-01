# Tarefas

| ID | Tarefa | Origem | Depende de | Concluída quando |
|---|---|---|---|---|
| T-01 | Criar projeto e estrutura de pastas | Constituição | | Projeto compila |
| T-02 | Criar `PasswordEntry`, `AppDbContext`, connection string, primeira migration e `Database.Migrate()` na inicialização; adicionar `*.db` ao `.gitignore` | Plan: Modelo de dados; Spec: RF05 | T-01 | Migration versionada no repositório e a aplicação cria o banco ao iniciar |
| T-03 | Implementar `IPasswordGenerator` e `PasswordGenerator` com testes unitários | Spec: RN01 a RN04; Plan: Decisões | T-01 | Testes rodando em muitas iterações (ex.: 10 mil) confirmam tamanho entre 16 e 128, as quatro classes de caracteres e ausência de espaços |
| T-04 | Implementar `IPasswordService` e `PasswordService` (gerar e salvar com GUID e data UTC, buscar por GUID) com testes unitários | Spec: RF05, RF06, RN05; Plan: Arquitetura | T-02, T-03 | Testes unitários passam: cada chamada gera GUID novo, a senha só é devolvida após gravar, GUID inexistente retorna nulo |
| T-05 | Configurar `AddProblemDetails()`, `UseStatusCodePages()` e `UseExceptionHandler()` | Spec: RF03, RF07; Plan: Decisões | T-01 | Rota inexistente retorna 404 com corpo ProblemDetails |
| T-06 | Implementar `POST /passwords` com `MapGroup`, `TypedResults`, `PasswordResponse` e header `Location`, com teste de integração de caminho feliz | Spec: RF01; Plan: Contratos | T-04, T-05 | Teste de integração confirma 201, GUID válido e senha dentro das regras |
| T-07 | Implementar `GET /passwords/{id:guid}` com testes de integração | Spec: RF02, RF03, RF04; Constituição: Qualidade | T-06 | Testes confirmam: mesma senha do POST (200), GUID inexistente (404), GUID vazio (404) e `/passwords/abc` (404), todos os 404 com ProblemDetails |
| T-08 | Garantir resposta 500 em ProblemDetails quando a gravação falhar, com teste de integração | Spec: casos de borda; Plan: Riscos | T-06 | Teste simula falha de gravação e confirma 500 em ProblemDetails, sem GUID nem senha no corpo |
| T-09 | Configurar Swagger (`AddSwaggerGen`, `UseSwagger`, `UseSwaggerUI` só em desenvolvimento) e declarar `.Produces`, `.ProducesProblem`, `.WithName` e `.WithSummary` nos endpoints | Plan: Decisões (Swagger) | T-06, T-07 | `/swagger` abre em desenvolvimento e documenta sucesso e erros dos dois endpoints |
| T-10 | Verificação final dos critérios de aceite | Spec: Critérios de aceite | T-01 a T-09 | Compila sem erros ou warnings, todos os testes passam e a aplicação roda sem erros |