# Estratégia de testes

Esta é a regra de testes do Notifications Service.

## Matriz das suítes

| Projeto | O que comprova | Pode depender de |
|---|---|---|
| `Notifications.UnitTests` | Invariantes do Domain | Domain |
| `Notifications.IntegrationTests` | Host real, pipeline HTTP, contratos e segurança | API e camadas externas |
| `Notifications.Database.IntegrationTests` | Modelo relacional e conventions | Infrastructure e Migrations |
| `Notifications.ArchitectureTests` | Direção das dependências | Assemblies inspecionados |

## UnitTests

```text
UnitTests/
  Domain/
    NotificationTests.cs
```

Regras obrigatórias:

- Referenciar somente o projeto `Domain`.
- Testar invariantes da entidade e transições de estado.
- Usar dados determinísticos.

Os testes cobrem:

- Criação válida de notificação.
- Validações de UserId, Subject, Body e Channel.
- MarkAsSent e MarkAsFailed.
- Retry e limite de tentativas.

## IntegrationTests

```text
IntegrationTests/
  Host/
    HealthEndpointTests.cs
  Contracts/
    NotificationEndpointTests.cs
    OpenApiContractTests.cs
  Support/
    NotificationsApiFactory.cs
```

- `Host`: health checks e comportamento global do processo.
- `Contracts`: status code, rotas, autorização e contrato OpenAPI.
- `Support`: `WebApplicationFactory` com `TestAuthHandler`.

## Database.IntegrationTests

Valida conventions do EF Core e metadata do modelo relacional sem abrir
conexão com PostgreSQL real.

## ArchitectureTests

Falham quando uma camada interna referencia uma externa, controllers aparecem
fora de Presentation, ou migrations fora do projeto Migrations.

## Execução local

```powershell
dotnet build FiapCloudGames.Notifications.slnx -c Release
dotnet test FiapCloudGames.Notifications.slnx -c Release --no-build
dotnet format FiapCloudGames.Notifications.slnx --verify-no-changes --no-restore
```
