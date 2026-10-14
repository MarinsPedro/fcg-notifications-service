# Arquitetura do Notifications Service

Este documento descreve as regras de estrutura, camadas e convenções do
Notifications Service.

## Camadas

```text
Presentation  →  Application  →  Domain
     ↓                ↓
Infrastructure ───────┘
```

| Camada | Projeto | Responsabilidade |
|---|---|---|
| Api | `FiapCloudGames.Notifications.Api` | Composition root, pipeline HTTP e configuração |
| Application | `FiapCloudGames.Notifications.Application` | Casos de uso, portas técnicas e DTOs internos |
| Domain | `FiapCloudGames.Notifications.Domain` | Entidades, value objects e regras de negócio |
| Infrastructure | `FiapCloudGames.Notifications.Infrastructure` | EF Core, repositórios, sender, health checks |
| Presentation | `FiapCloudGames.Notifications.Presentation` | Controllers, requests, responses e policies |
| Migrations | `FiapCloudGames.Notifications.Migrations` | Migrations EF Core e runner |
| Worker | `FiapCloudGames.Notifications.Worker` | Consumers RabbitMQ, event DTOs, templates HTML |

## Workloads

Diferente dos serviços puramente HTTP, o Notifications possui **dois workloads**:

- **API** (`notifications-api`): endpoints HTTP para consulta e retry administrativo.
- **Worker** (`notifications-worker`): BackgroundService que consome 5 filas do
  RabbitMQ via MassTransit (boas-vindas, pagamento e pedido).

Ambos ficam no mesmo repositório e compartilham Domain, Application e
Infrastructure. O Worker e a API ficam no mesmo repositório conforme a regra:
"API e Worker do mesmo domínio ficam no mesmo repositório".

## Regras de dependência

- **Domain** não referencia nenhum outro projeto nem pacotes de infraestrutura.
- **Application** referencia apenas Domain.
- **Infrastructure** referencia Application e Domain.
- **Presentation** referencia Application e Domain.
- **Api** referencia todas as camadas para composição.
- **Migrations** referencia Infrastructure para acessar o `DbContext`.
- **Worker** referencia Infrastructure para acessar repositórios e services.

Essas regras são verificadas automaticamente pelo projeto `ArchitectureTests`.

## Convenções de código

### Entidades e agregados

- Construtores privados.
- Factory method estático `Create()` com validações.
- Propriedades somente-leitura com setters `private` ou `init`.

### Services (Application)

- Uma classe por caso de uso.
- Recebem dependências por injeção no construtor (primary constructor).
- Retornam um `Result` record tipado.
- Lançam `AppException` do BuildingBlocks para erros.

### Controllers (Presentation)

- Um controller por feature ou agregado.
- Rota base com versionamento: `internal/v1/<recurso>`.
- Services injetados via `[FromServices]`.
- `ProducesResponseType` declarado para cada status code possível.

### Abstrações de envio

- `INotificationSender` define a porta técnica de envio.
- `LogNotificationSender` é a implementação placeholder (simula via log).
- Implementações reais (e-mail, push) substituem o placeholder por DI.

### Templates de notificação (Worker)

- 4 templates HTML em `Worker/Templates/`, um por tipo de evento.
- Armazenados como embedded resources no assembly do Worker.
- `NotificationTemplateRenderer` carrega o template e substitui placeholders
  (`{{OrderId}}`, `{{Amount}}`, `{{GameName}}`, `{{Reason}}`).
- Layout responsivo e compatível com clientes de e-mail (table-based).

## Configuração da solução

- `Directory.Build.props` centraliza `TargetFramework`, `Nullable`,
  `TreatWarningsAsErrors` e a flag `UseLocalBuildingBlocks`.
- `Directory.Packages.props` centraliza versões de pacotes.
- `global.json` trava a versão do SDK.
