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

| Evento | Produtor | Descrição |
|---|---|---|
| `PaymentApprovedV1` | Payments | Pagamento aprovado, notifica confirmação |
| `PaymentRejectedV1` | Payments | Pagamento rejeitado, notifica para tentar novamente |
| `OrderCompletedV1` | Orders | Pedido concluído, jogo disponível na biblioteca |
| `OrderFailedV1` | Orders | Pedido falhou, notifica o problema |

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
- PostgreSQL 17 (ou usar Docker Compose quando disponível);
- Docker e Docker Compose (opcional, para execução via contêiner).

### Building Blocks

O pacote `FiapCloudGames.BuildingBlocks` versão `0.2.2` está disponível no
GitHub Packages como pacote privado. Este repositório inclui uma cópia local
em `packages/` como fallback, configurada no `NuGet.Config`. O restore funciona
sem autenticação no GitHub Packages enquanto o fallback estiver presente.

Para restaurar diretamente do GitHub Packages (sem fallback), defina:

```powershell
$env:GITHUB_PACKAGES_USERNAME = "MarinsPedro"
$env:GITHUB_PACKAGES_TOKEN = "SEU_PAT_CLASSIC"
```

O token precisa de `read:packages`. Remova-o da sessão quando não for mais
necessário:

```powershell
Remove-Item Env:\GITHUB_PACKAGES_TOKEN
```

## Execução local

1. Inicie um PostgreSQL e crie o banco:

```sql
CREATE USER fcg_notifications_app WITH PASSWORD 'SuaSenha';
CREATE DATABASE fcg_notifications OWNER fcg_notifications_app;
```

2. Configure a connection string:

```powershell
$env:ConnectionStrings__Database = "Host=localhost;Port=5432;Database=fcg_notifications;Username=fcg_notifications_app;Password=SuaSenha"
```

3. Configure a autenticação JWT (apontando para o Identity Service):

```powershell
$env:Authentication__Authority = "http://localhost:5081"
$env:Authentication__Audience = "notifications-api"
```

4. Restaure, aplique as migrations e execute:

```powershell
dotnet tool restore
dotnet restore FiapCloudGames.Notifications.slnx
dotnet run --project src/FiapCloudGames.Notifications.Migrations
dotnet run --project src/FiapCloudGames.Notifications.Api
```

Em `Development`, o Swagger fica disponível em `http://localhost:5087/swagger`.

## Configuração

| Variável | Descrição | Exemplo |
|---|---|---|
| `ConnectionStrings__Database` | Connection string do PostgreSQL | `Host=localhost;Port=5432;Database=fcg_notifications;Username=fcg_notifications_app;Password=xxx` |
| `Authentication__Authority` | URL do Identity Service (emissor dos tokens) | `http://localhost:5081` |
| `Authentication__Audience` | Audience esperada nos tokens JWT | `notifications-api` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação | `Development` |

## Criando migrations

```powershell
dotnet ef migrations add NomeDaMigration `
  --project src/FiapCloudGames.Notifications.Migrations `
  --startup-project src/FiapCloudGames.Notifications.Migrations `
  --output-dir Migrations
```

O processo da API nunca executa migrations automaticamente. Em contêineres, use
a imagem com target `migrations` antes de iniciar a API.

## Endpoints principais

| Método | Rota | Autenticação | Uso |
|---|---|---|---|
| `GET` | `/internal/v1/notifications/{notificationId}` | service + `notifications.read` | consulta notificação |
| `POST` | `/internal/v1/notifications/{notificationId}/retry` | service + `notifications.retry` | reprocessa notificação |
| `GET` | `/health/live` | anônimo | liveness |
| `GET` | `/health/ready` | anônimo | readiness |

Todos os endpoints de negócio são autenticados por padrão. Somente health checks
aceitam chamadas anônimas.

## Qualidade

```powershell
dotnet build FiapCloudGames.Notifications.slnx -c Release
dotnet test FiapCloudGames.Notifications.slnx -c Release --no-build
dotnet format FiapCloudGames.Notifications.slnx --verify-no-changes --no-restore
```
