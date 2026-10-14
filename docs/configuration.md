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

O Worker consome 5 filas no exchange `fcg.events`:

| Routing key | Fila |
|---|---|
| `identity.user-created.v1` | `notifications.user-created` |
| `payments.payment-approved.v1` | `notifications.payment-approved` |
| `payments.payment-rejected.v1` | `notifications.payment-rejected` |
| `orders.order-completed.v1` | `notifications.order-completed` |
| `orders.order-failed.v1` | `notifications.order-failed` |

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

## Health checks

- `/health/live` verifica se o processo HTTP está respondendo.
- `/health/ready` verifica se o PostgreSQL pode ser acessado.

## Produção

Antes de publicar:

- substitua todos os placeholders e senhas locais;
- injete connection strings e segredos por secret manager;
- configure credenciais individuais no RabbitMQ por serviço;
- habilite TLS no broker;
- não registre tokens JWT ou connection strings em logs;
- conceda ao workflow acesso de leitura ao pacote privado Building Blocks.
