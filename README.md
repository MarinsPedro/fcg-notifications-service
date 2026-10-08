# Fiap Cloud Games — Notifications Service

Serviço responsável por receber eventos de outros microsserviços e enviar notificações aos usuários da plataforma Fiap Cloud Games.

Este repositório é independente do monólito. Ele possui banco, migrations, pipeline e ciclo de entrega próprios.

## Responsabilidades

- consumir eventos de pagamento e conclusão de pedidos;
- registrar notificações com tentativas e resultados;
- enviar notificações por e-mail (provedor fake/log inicialmente);
- impedir notificações duplicadas por `eventId`;
- permitir consulta e retry administrativos;
- falha de notificação nunca reverte a compra.

## Eventos consumidos

| Evento | Produtor |
|---|---|
| `PaymentApprovedV1` | Payments |
| `PaymentRejectedV1` | Payments |
| `OrderCompletedV1` | Orders |
| `OrderFailedV1` | Orders |

## Estrutura

```text
src/
  FiapCloudGames.Notifications.Api/             composition root e pipeline
  FiapCloudGames.Notifications.Application/     casos de uso e portas técnicas
  FiapCloudGames.Notifications.Domain/          agregados e regras do domínio
  FiapCloudGames.Notifications.Infrastructure/  EF Core, repositories, sender e autenticação
  FiapCloudGames.Notifications.Presentation/    Controllers, Requests, Responses e mappings HTTP
  FiapCloudGames.Notifications.Migrations/      migrations e atualizador do banco
tests/
  FiapCloudGames.Notifications.ArchitectureTests/
  FiapCloudGames.Notifications.Database.IntegrationTests/
  FiapCloudGames.Notifications.IntegrationTests/
  FiapCloudGames.Notifications.UnitTests/
```

## Pré-requisitos

- .NET SDK 10;
- PostgreSQL;
- acesso ao pacote privado `FiapCloudGames.BuildingBlocks` versão `0.2.2` no GitHub Packages.

## Execução local

1. Copie `.env.example` para o mecanismo local de segredos de sua preferência.
2. Inicie um PostgreSQL e crie o banco `fcg_notifications` com usuário `fcg_notifications_app`.
3. Restaure, aplique as migrations e execute:

```powershell
$env:GITHUB_PACKAGES_USERNAME = "MarinsPedro"
$env:GITHUB_PACKAGES_TOKEN = "SEU_PAT_CLASSIC"

dotnet tool restore
dotnet restore FiapCloudGames.Notifications.slnx
dotnet run --project src/FiapCloudGames.Notifications.Migrations
dotnet run --project src/FiapCloudGames.Notifications.Api
```

Em `Development`, a interface Swagger fica disponível em `http://localhost:5087/swagger`.

## Endpoints principais

| Método | Rota | Autenticação | Uso |
|---|---|---|---|
| `GET` | `/internal/v1/notifications/{id}` | service + `notifications.read` | consulta notificação |
| `POST` | `/internal/v1/notifications/{id}/retry` | service + `notifications.retry` | reprocessa notificação |
| `GET` | `/health/live` | anônimo | liveness |
| `GET` | `/health/ready` | anônimo | readiness |

## Qualidade

```powershell
dotnet build FiapCloudGames.Notifications.slnx -c Release
dotnet test FiapCloudGames.Notifications.slnx -c Release --no-build
dotnet format FiapCloudGames.Notifications.slnx --verify-no-changes --no-restore
```
