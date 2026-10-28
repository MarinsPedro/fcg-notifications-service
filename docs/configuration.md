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
RabbitMq__Username=guest
RabbitMq__Password=guest
```

O Worker consome 5 filas. Cada produtor publica de um jeito, e o Notifications
se liga ao que ele publica (formatos e exemplos em [endpoints.md](endpoints.md)):

| Fila | Exchange | Routing key | Formato |
|---|---|---|---|
| `notifications.user-created` | `identity.user-created.v1` | - | envelope do MassTransit |
| `notifications.payment-approved` | `fcg.payments.events` (topic) | `payments.paid` | JSON puro |
| `notifications.payment-rejected` | `fcg.payments.events` (topic) | `payments.failed` | JSON puro |
| `notifications.order-completed` | `orders.order-completed.v1` | - | envelope do MassTransit |
| `notifications.order-failed` | `orders.order-failed.v1` | - | envelope do MassTransit |

Os nomes ficam em `ExchangeNames.cs`. O Orders ainda não publica os dois últimos
eventos; os nomes seguem o padrão `<produtor>.<evento>.v1` que ele usa e são uma
proposta a confirmar.

## Variáveis de ambiente

| Variável | Descrição | Exemplo |
|---|---|---|
| `ConnectionStrings__Database` | Connection string do PostgreSQL | `Host=localhost;Port=5432;Database=fcg_notifications;...` |
| `Authentication__Authority` | URL do Identity Service | `http://localhost:5081` |
| `Authentication__Audience` | Audience esperada nos tokens JWT | `notifications-api` |
| `RabbitMq__Host` | Host do RabbitMQ | `localhost` |
| `RabbitMq__Username` | Usuário do RabbitMQ | `guest` |
| `RabbitMq__Password` | Senha do RabbitMQ | `guest` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação | `Development` |

## Kubernetes

Em `k8s/base/` a configuração é separada em ConfigMap e Secrets:

| Recurso | Conteúdo |
|---|---|
| `notifications-config` (ConfigMap) | `ASPNETCORE_ENVIRONMENT`, `Authentication__Authority` (`http://identity-api:8080`), `Authentication__Audience` e `RabbitMq__Host` (`rabbitmq`) |
| `notifications-secret` (Secret) | `ConnectionStrings__Database` e `RabbitMq__Username`/`RabbitMq__Password` |
| `notifications-db-secret` (Secret) | `POSTGRES_DB`, `POSTGRES_USER` e `POSTGRES_PASSWORD`, usados só pelo contêiner do PostgreSQL |

Os dados do `notifications-db-secret` precisam ser iguais aos da connection
string do `notifications-secret` (banco `fcg_notifications`, usuário
`fcg_notifications_app` e a mesma senha). Se divergirem, a API, o Worker e as
migrations não autenticam no banco.

O banco roda como Deployment (`notifications-db`, PostgreSQL 17) com volume
persistente de 1Gi (`notifications-db-data`), que exige uma StorageClass padrão
no cluster. As tabelas são criadas pelo Job `notifications-migrations` (imagem
`fcg-notifications-migrations`), que tenta até 10 vezes enquanto o banco sobe.

A API e o Worker são Deployments separados (`notifications-api` e
`notifications-worker`); só a API tem Service.

O RabbitMQ e o Identity não fazem parte destes manifests: o ConfigMap espera
Services chamados `rabbitmq` e `identity-api` no namespace `fcg`. Sem o
RabbitMQ, o Worker não consegue consumir as filas.

## Health checks

- `/health/live` verifica se o processo HTTP está respondendo.
- `/health/ready` verifica se o PostgreSQL pode ser acessado.
- O Worker não expõe health check HTTP.

## Produção

Antes de publicar:

- substitua todos os placeholders e senhas locais;
- injete connection strings e segredos por secret manager;
- configure credenciais individuais no RabbitMQ por serviço;
- habilite TLS no broker;
- não registre tokens JWT ou connection strings em logs;
- conceda ao workflow acesso de leitura ao pacote privado Building Blocks.
