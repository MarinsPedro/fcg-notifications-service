# Configuração do Notifications

As configurações seguem a hierarquia padrão do ASP.NET Core. Em produção, use
variáveis de ambiente e um gerenciador de segredos.

## Banco de dados

```text
ConnectionStrings__Database=Host=localhost;Port=5432;Database=fcg_notifications;Username=fcg_notifications_app;Password=...
```

O Notifications é o único proprietário do banco `fcg_notifications`. Nenhum
outro serviço deve acessar diretamente suas tabelas.

## Autenticação JWT

```text
Authentication__Authority=http://localhost:5081
Authentication__Audience=notifications-api
```

- `Authority` aponta para o Identity Service.
- `Audience` identifica este serviço.

## RabbitMQ

```text
RabbitMq__Host=localhost
RabbitMq__Port=5672
RabbitMq__VirtualHost=/
RabbitMq__Username=guest
RabbitMq__Password=guest
```

O Worker consome 5 filas MassTransit, configuradas em `fcg-orchestration/config/*notifications-worker.env` e na classe `RabbitMqTopologyConfigurator.cs`:

| Fila | Exchange | Formato |
|---|---|---|
| `notifications.user-created` | `identity.user-created.v1` | MassTransit |
| `notifications.payment-approved` | `payments.payment-approved.v1` | MassTransit |
| `notifications.payment-rejected` | `payments.payment-rejected.v1` | MassTransit |
| `notifications.order-completed` | `orders.order-completed.v1` | MassTransit |
| `notifications.order-failed` | `orders.order-failed.v1` | MassTransit |

No pacote inspecionado, Orders publica `OrderCompletedEventV1`, mas a publicacao de `OrderFailedEventV1` nao foi encontrada. O consumer do Notifications esta preparado para recebe-lo, quando publicado.

## Variáveis de ambiente

| Variável | Descrição | Exemplo |
|---|---|---|
| `ConnectionStrings__Database` | Connection string do PostgreSQL | `Host=localhost;Port=5432;Database=fcg_notifications;...` |
| `Authentication__Authority` | URL do Identity Service | `http://localhost:5081` |
| `Authentication__Audience` | Audience esperada nos tokens JWT | `notifications-api` |
| `RabbitMq__Host` | Host do RabbitMQ | `localhost` |
| `RabbitMq__Port` | Porta AMQP do RabbitMQ (opcional, padrão `5672`) | `5672` |
| `RabbitMq__VirtualHost` | Virtual host do RabbitMQ (opcional, padrão `/`) | `/` |
| `RabbitMq__Username` | Usuário do RabbitMQ | `guest` |
| `RabbitMq__Password` | Senha do RabbitMQ | `guest` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação | `Development` |

## Kubernetes consolidado

Use `../fcg-orchestration/FCG-K8S.cmd`. Os tres manifests proprios do servico ficam em `k8s/`: `api.yaml`, `worker.yaml`, `migrations-job.yaml`. O Notifications nao sobe um PostgreSQL isolado: `fcg_notifications` e criado no Postgres compartilhado pelo script central.

- `fcg-notifications-api-config`, `fcg-notifications-worker-config` e `fcg-notifications-migrations-config` sao gerados de `fcg-orchestration/config/k8s-notifications-*.env`.
- `fcg-domain-dev-secrets` contem a chave `notifications-connection-string` e `fcg-dev-secrets` fornece as credenciais RabbitMQ.
- A porta externa para Swagger e `127.0.0.1:5187`, via port-forward criado pelo script.
- No Compose, a porta e `127.0.0.1:5087` e a senha fica em `runtime/compose.env`.
- A senha demonstrativa `Fiap@123456` e gerada somente em instalacao DEV nova; clusters existentes preservam os valores anteriores.

O sistema de eventos atual utiliza MassTransit para declarar cinco filas e exchanges, conforme `RabbitMqTopologyConfigurator.cs` e `fcg-orchestration/config/*notifications-worker.env`.

## Logs e rastreio

Cada consumer do Worker abre um escopo de log com `MessageId`, `CorrelationId` e
`ConversationId` da mensagem, quando o produtor os informa. Use o
`CorrelationId` para seguir uma operação entre os serviços e o `MessageId` para
achar a mensagem exata. O console do Worker imprime esses campos porque o
`appsettings.json` define `Logging:Console:FormatterOptions:IncludeScopes=true`.

## Health checks

- `/health/live` verifica se o processo HTTP está respondendo.
- `/health/ready` verifica se o PostgreSQL pode ser acessado.
- O Worker também expõe `/health/live` e `/health/ready` na porta 8080 do
  contêiner. O `ready` verifica o PostgreSQL e a conexão com o RabbitMQ; o
  `live` só confirma que o processo responde. Os probes do Kubernetes em
  `worker.yaml` usam esses endpoints. Localmente, defina
  `ASPNETCORE_URLS` (por exemplo `http://localhost:5090`) para escolher a porta.

## Produção

Antes de publicar:

- substitua todos os placeholders e senhas locais;
- injete connection strings e segredos por secret manager;
- configure credenciais individuais no RabbitMQ por serviço;
- habilite TLS no broker;
- não registre tokens JWT ou connection strings em logs;
- conceda ao workflow acesso de leitura ao pacote privado Building Blocks.
