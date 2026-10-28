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

| Evento | Produtor | Fila | Template |
|---|---|---|---|
| `UserCreatedEvent` | Identity | `notifications.user-created` | `UserCreated.html` |
| `PaymentApprovedV1` (`payments.paid`) | Payments | `notifications.payment-approved` | `PaymentApproved.html` |
| `PaymentRejectedV1` (`payments.failed`) | Payments | `notifications.payment-rejected` | `PaymentRejected.html` |
| `OrderCompletedV1` | Orders | `notifications.order-completed` | `OrderCompleted.html` |
| `OrderFailedV1` | Orders | `notifications.order-failed` | `OrderFailed.html` |

Exchanges, routing keys, formato das mensagens e exemplos estão em\n[docs/endpoints.md](docs/endpoints.md). Cada evento gera uma notificação com corpo HTML renderizado a partir de um
template com placeholders (`{{UserName}}`, `{{Email}}`, `{{OrderId}}`,
`{{Amount}}`, `{{GameName}}`, `{{Reason}}`). Os templates ficam como embedded
resources no projeto Worker.

## Estrutura

```text
src/
  FiapCloudGames.Notifications.Api/             composition root e pipeline HTTP
  FiapCloudGames.Notifications.Application/     casos de uso e portas técnicas
  FiapCloudGames.Notifications.Domain/          agregados e regras do domínio
  FiapCloudGames.Notifications.Infrastructure/  EF Core, repositories, sender e autenticação
  FiapCloudGames.Notifications.Presentation/    Controllers, Requests, Responses e mappings HTTP
  FiapCloudGames.Notifications.Migrations/      migrations e atualizador do banco
  FiapCloudGames.Notifications.Worker/          consumers RabbitMQ via MassTransit
    Consumers/                                  5 consumers (um por evento)
    Events/                                     DTOs dos eventos consumidos
    Templates/                                  templates HTML de notificação
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
k8s/
  base/
    namespace.yaml          namespace fcg
    configmap.yaml          configurações não-sensíveis (JWT, RabbitMQ host)
    secret.yaml             connection string e credenciais RabbitMQ
    db-secret.yaml          credenciais do PostgreSQL (iguais às do secret.yaml)
    db-pvc.yaml             volume persistente do banco (1Gi)
    db-deployment.yaml      Deployment do PostgreSQL 17 (notifications-db)
    db-service.yaml         Service ClusterIP do banco (porta 5432)
    migrations-job.yaml     Job que aplica as migrations e termina
    deployment-api.yaml     Deployment da API HTTP
    deployment-worker.yaml  Deployment do Worker (consumers RabbitMQ)
    service.yaml            Service ClusterIP (porta 80 → 8080)
    kustomization.yaml      agregador dos recursos
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

O pacote `FiapCloudGames.BuildingBlocks` versão `0.3.0` está disponível no
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
| Worker | `fcg-notifications-worker` | — (consome filas RabbitMQ) |

A senha padrão do banco é `DevPass123!`. O RabbitMQ usa `guest`/`guest`.

Após subir, o Swagger fica disponível em `http://localhost:5087/swagger` e o
RabbitMQ Management em `http://localhost:15672`.

Para derrubar tudo e limpar os volumes:

```powershell
docker compose down -v
```

## Deploy em Kubernetes

Os manifests ficam em `k8s/base/` e usam Kustomize (embutido no `kubectl`).

1. Construa as imagens e carregue no cluster local (exemplo com Kind):

```powershell
docker build --target final --secret "id=nuget_config,src=$env:APPDATA\NuGet\NuGet.Config" -t fcg-notifications:latest .
docker build --target worker --secret "id=nuget_config,src=$env:APPDATA\NuGet\NuGet.Config" -t fcg-notifications-worker:latest .
docker build --target migrations --secret "id=nuget_config,src=$env:APPDATA\NuGet\NuGet.Config" -t fcg-notifications-migrations:latest .
kind load docker-image fcg-notifications:latest fcg-notifications-worker:latest fcg-notifications-migrations:latest
```

2. Antes de aplicar, edite `k8s/base/secret.yaml` (connection string e
   credenciais do RabbitMQ) e `k8s/base/db-secret.yaml` (credenciais do
   PostgreSQL). A senha do banco precisa ser a mesma nos dois arquivos. As
   senhas que vêm nesses arquivos são valores de estudo, só para o cluster
   local; em qualquer outro ambiente, substitua-as e não as versione.

3. Aplique os manifests. Além da API e do Worker, eles criam o banco
   (`notifications-db`, com volume persistente) e o Job `notifications-migrations`,
   que cria as tabelas:

```powershell
kubectl apply -k k8s/base/
```

4. Verifique os recursos:

```powershell
kubectl get pods,jobs -n fcg -l "app in (notifications-api, notifications-worker, notifications-db, notifications-migrations)"
```

Devem aparecer `notifications-db`, `notifications-api` e `notifications-worker`
em `Running` e o Job `notifications-migrations` em `Completed`. Para ver o
resultado das migrations:

```powershell
kubectl logs -n fcg job/notifications-migrations
```

Dentro do cluster, outros serviços acessam a API via `http://notifications-api:80`.
O Worker consome as filas do RabbitMQ automaticamente via MassTransit.

O RabbitMQ e o Identity não fazem parte destes manifests: o ConfigMap espera
Services chamados `rabbitmq` e `identity-api` no namespace `fcg`. Sem o
RabbitMQ, o Worker não consome as filas. Veja
[docs/configuration.md](docs/configuration.md) e [docs/endpoints.md](docs/endpoints.md).

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
$env:RabbitMq__Port = "5672"         # opcional
$env:RabbitMq__VirtualHost = "/"     # opcional
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

6. Em outro terminal, inicie o Worker:

```powershell
dotnet run --project src/FiapCloudGames.Notifications.Worker
```

Em `Development`, o Swagger fica disponível em `http://localhost:5087/swagger`.

## Configuração

| Variável | Descrição | Exemplo |
|---|---|---|
| `ConnectionStrings__Database` | Connection string do PostgreSQL | `Host=localhost;Port=5432;Database=fcg_notifications;Username=fcg_notifications_app;Password=xxx` |
| `Authentication__Authority` | URL do Identity Service (emissor dos tokens) | `http://localhost:5081` |
| `Authentication__Audience` | Audience esperada nos tokens JWT | `notifications-api` |
| `RabbitMq__Host` | Host do RabbitMQ | `localhost` |
| `RabbitMq__Port` | Porta AMQP do RabbitMQ (opcional, padrão `5672`) | `5672` |
| `RabbitMq__VirtualHost` | Virtual host do RabbitMQ (opcional, padrão `/`) | `/` |
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
