# Fiap Cloud Games — Notifications Service

Serviço responsável por receber eventos de outros microsserviços e enviar notificações aos usuários da plataforma Fiap Cloud Games.

Este repositório é independente do monólito. Ele possui banco, migrations, pipeline e ciclo de entrega próprios.

## Responsabilidades

- consumir eventos de pagamento e conclusão de pedidos via RabbitMQ;
- registrar notificações com tentativas e resultados;
- enviar notificações por e-mail (provedor fake/log inicialmente);
- impedir notificações duplicadas por `eventId` (inbox pattern);
- permitir consulta e retry administrativos;
- falha de notificação nunca reverte a compra.

## Eventos consumidos

| Evento | Produtor | Fila | Descrição |
|---|---|---|---|
| `PaymentApprovedV1` | Payments | `notifications.payment-approved` | Pagamento aprovado |
| `PaymentRejectedV1` | Payments | `notifications.payment-rejected` | Pagamento rejeitado |
| `OrderCompletedV1` | Orders | `notifications.order-completed` | Pedido concluído |
| `OrderFailedV1` | Orders | `notifications.order-failed` | Pedido falhou |

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
docs/
  architecture.md
  configuration.md
  endpoints.md
  testing.md
```

As regras e exemplos da estrutura estão em [docs/architecture.md](docs/architecture.md).
O uso, os contratos e a ordem recomendada dos endpoints estão em
[docs/endpoints.md](docs/endpoints.md).
A estratégia e as fronteiras das suítes de teste estão em
[docs/testing.md](docs/testing.md).
As variáveis de ambiente e opções de produção estão em
[docs/configuration.md](docs/configuration.md).

## Pré-requisitos

- .NET SDK 10;
- PostgreSQL 17 (ou usar Docker Compose);
- RabbitMQ 3.13+ (ou usar Docker Compose);
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

Para testar alterações locais do Building Blocks sem publicar um NuGet,
mantenha os repositórios como irmãos e use:

```powershell
dotnet build FiapCloudGames.Notifications.slnx -p:UseLocalBuildingBlocks=true
```

O padrão é `UseLocalBuildingBlocks=false`, portanto CI e produção continuam
usando a versão publicada definida em `Directory.Packages.props`.

## Execução local com Docker Compose (recomendado)

A forma mais simples de rodar o serviço completo:

```powershell
docker compose up --build
```

Isso sobe automaticamente:

| Serviço | Container | Porta |
|---|---|---|
| PostgreSQL 17 | `fcg-notifications-db` | `5435` |
| RabbitMQ 3.13 | `fcg-notifications-rabbit` | `5672` (AMQP) / `15672` (Management) |
| Migrations | `fcg-notifications-migrations` | — (executa e encerra) |
| API | `fcg-notifications-api` | `5087` |

A senha padrão do banco é `DevPass123!`. O RabbitMQ usa `guest`/`guest`.

Após subir, o Swagger fica disponível em `http://localhost:5087/swagger` e o
RabbitMQ Management em `http://localhost:15672`.

Para derrubar tudo e limpar os volumes:

```powershell
docker compose down -v
```

## Execução local sem Docker

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

4. Configure o RabbitMQ:

```powershell
$env:RabbitMq__Host = "localhost"
$env:RabbitMq__Username = "guest"
$env:RabbitMq__Password = "guest"
```

5. Restaure, aplique as migrations e execute:

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
| `RabbitMq__Host` | Host do RabbitMQ | `localhost` |
| `RabbitMq__Username` | Usuário do RabbitMQ | `guest` |
| `RabbitMq__Password` | Senha do RabbitMQ | `guest` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação | `Development` |

## Criando migrations

```powershell
dotnet ef migrations add NomeDaMigration `
  --project src/FiapCloudGames.Notifications.Migrations `
  --startup-project src/FiapCloudGames.Notifications.Migrations `
  --output-dir Migrations
```

O projeto suporta as duas ferramentas do EF Core:

- `dotnet ef`, restaurado pelo tool manifest em `.config/dotnet-tools.json`;
- Package Manager Console do Visual Studio, por meio de
  `Microsoft.EntityFrameworkCore.Tools`.

No Package Manager Console:

```powershell
Add-Migration NomeDaMigration `
  -Project FiapCloudGames.Notifications.Migrations `
  -StartupProject FiapCloudGames.Notifications.Migrations `
  -OutputDir Migrations
```

O processo da API nunca executa migrations automaticamente. Em contêineres, use
a imagem com target `migrations` antes de iniciar a API:

```powershell
docker build `
  --target migrations `
  --secret "id=nuget_config,src=$env:APPDATA\NuGet\NuGet.Config" `
  -t fcg-notifications-migrations:local .
```

Para criar a imagem da API sem gravar credenciais do GitHub Packages em camadas
Docker, forneça um `NuGet.Config` autenticado como secret do BuildKit:

```powershell
docker build `
  --secret "id=nuget_config,src=$env:APPDATA\NuGet\NuGet.Config" `
  -t fcg-notifications:local .
```

## Endpoints principais

| Método | Rota | Autenticação | Uso |
|---|---|---|---|
| `GET` | `/internal/v1/notifications/{notificationId}` | service + `notifications.read` | consulta notificação |
| `POST` | `/internal/v1/notifications/{notificationId}/retry` | service + `notifications.retry` | reprocessa notificação |
| `GET` | `/health/live` | anônimo | liveness |
| `GET` | `/health/ready` | anônimo | readiness |

Todos os endpoints de negócio são autenticados por padrão. Somente health checks
aceitam chamadas anônimas.

Os contratos detalhados com exemplos de request/response estão em
[docs/endpoints.md](docs/endpoints.md).

## Qualidade

```powershell
dotnet build FiapCloudGames.Notifications.slnx -c Release
dotnet test FiapCloudGames.Notifications.slnx -c Release --no-build
dotnet format FiapCloudGames.Notifications.slnx --verify-no-changes --no-restore
```

## Segurança operacional

- armazene segredos e connection strings em variáveis de ambiente ou secret manager, nunca em Git;
- configure credenciais individuais no RabbitMQ por serviço;
- não registre tokens JWT, segredos ou connection strings em logs;
- revogue imediatamente qualquer token pessoal do GitHub que tenha sido exposto;
- substitua as credenciais locais conhecidas fora de `Development`.
