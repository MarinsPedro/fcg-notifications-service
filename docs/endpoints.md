# Endpoints e contratos do Notifications

Este documento detalha cada endpoint do Notifications Service.

## Visão rápida

| Endpoint | Quem chama | Credencial exigida | Finalidade |
|---|---|---|---|
| `GET /internal/v1/notifications/{notificationId}` | administração interna | service + `notifications.read` | consultar notificação |
| `POST /internal/v1/notifications/{notificationId}/retry` | administração interna | service + `notifications.retry` | reprocessar notificação |
| `GET /health/live` | infraestrutura | nenhuma | liveness |
| `GET /health/ready` | infraestrutura | nenhuma | readiness |

Todos os endpoints de negócio são autenticados por padrão. Somente health checks
aceitam chamadas anônimas.

## Detalhamento dos endpoints

### `GET /internal/v1/notifications/{notificationId}`

Consulta uma notificação pelo ID. Exige token `service` com scope
`notifications.read`.

```http
GET /internal/v1/notifications/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer <token-service>
```

**Response (200 OK):**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "eventType": "PaymentApprovedV1",
  "eventId": "2ace1057-8815-4675-babe-a672928b328c",
  "recipientUserId": "47de4c37-34ab-4084-a293-e46229c34aad",
  "channel": "Email",
  "subject": "Pagamento aprovado",
  "status": "Sent",
  "attempts": 1,
  "lastAttemptAtUtc": "2026-10-14T12:00:00+00:00",
  "createdAtUtc": "2026-10-14T12:00:00+00:00"
}
```

| Campo | Tipo | Descrição |
|---|---|---|
| `id` | UUID | Identificador da notificação |
| `eventType` | string | Tipo do evento que originou a notificação |
| `eventId` | UUID | ID do evento externo (usado para deduplicação) |
| `recipientUserId` | UUID | Usuário destinatário |
| `channel` | string | Canal de envio (`Email`, `Push`, `SMS`) |
| `subject` | string | Assunto da notificação |
| `status` | string | `Pending`, `Sent` ou `Failed` |
| `attempts` | int | Número de tentativas de envio |
| `lastAttemptAtUtc` | datetime? | Data da última tentativa |
| `createdAtUtc` | datetime | Data de criação |

Retorna `404 Not Found` quando o ID não existe.

### `POST /internal/v1/notifications/{notificationId}/retry`

Reprocessa uma notificação falhada. Exige token `service` com scope
`notifications.retry`.

```http
POST /internal/v1/notifications/3fa85f64-5717-4562-b3fc-2c963f66afa6/retry
Authorization: Bearer <token-service>
```

**Response (200 OK):** mesmo formato da consulta.

| Status | Cenário |
|---|---|
| `200` | Retry executado com sucesso |
| `404` | Notificação não encontrada |
| `422` | Limite de tentativas atingido (máximo 5) |

## Eventos consumidos (Worker)

O Worker consome 5 filas do RabbitMQ via MassTransit. Cada evento gera uma
notificação no banco e tenta o envio imediato. Cada produtor publica de um
jeito, e o Notifications se adapta ao contrato publicado por ele.

| Evento no Notifications | Produtor | Exchange | Routing key | Fila | Formato |
|---|---|---|---|---|---|
| `UserCreatedEvent` | Identity | `identity.user-created.v1` | - | `notifications.user-created` | envelope do MassTransit |
| `PaymentApprovedV1` | Payments (`PaymentPaidEventV1`) | `fcg.payments.events` (topic) | `payments.paid` | `notifications.payment-approved` | JSON puro |
| `PaymentRejectedV1` | Payments (`PaymentFailedEventV1`) | `fcg.payments.events` (topic) | `payments.failed` | `notifications.payment-rejected` | JSON puro |
| `OrderCompletedV1` | Orders (a confirmar) | `orders.order-completed.v1` | - | `notifications.order-completed` | envelope do MassTransit |
| `OrderFailedV1` | Orders (a confirmar) | `orders.order-failed.v1` | - | `notifications.order-failed` | envelope do MassTransit |

Os nomes das exchanges e as routing keys ficam em `ExchangeNames.cs`.

- **Identity:** o consumer usa uma cópia do contrato
  (`Contracts/Identity/UserCreatedEvent.cs`) com o mesmo namespace e o mesmo
  nome do tipo do Identity, porque o MassTransit casa a mensagem pelo tipo.
  Campos: `eventId`, `userId`, `name`, `email` e `occurredAtUtc`.
- **Payments:** publica JSON puro, sem envelope. As filas desligam a topologia
  automática e se ligam à exchange `fcg.payments.events` à mão, uma routing key
  por fila. Campos lidos: `paymentId`, `orderId`, `userId` e `amount` (aprovado);
  `paymentId`, `orderId`, `userId` e `failureReason` (recusado). A deduplicação
  usa o `MessageId` da mensagem, ou o `paymentId` quando ele não vier.
- **Orders:** ainda não publica estes dois eventos. Os nomes seguem o padrão
  `<produtor>.<evento>.v1` que o Orders usa e são uma proposta a confirmar.

### Exemplos de mensagem

Pagamento aprovado (`payments.paid`, JSON puro):

```json
{
  "paymentId": "uuid",
  "orderId": "uuid",
  "userId": "uuid",
  "amount": 84.90,
  "providerTransactionId": "tx-123",
  "occurredAtUtc": "2026-10-27T12:00:00+00:00"
}
```

Pagamento recusado (`payments.failed`, JSON puro):

```json
{
  "paymentId": "uuid",
  "orderId": "uuid",
  "userId": "uuid",
  "amount": 10.00,
  "failureReason": "Cartão recusado",
  "occurredAtUtc": "2026-10-27T12:00:00+00:00"
}
```

Usuário criado (`identity.user-created.v1`, conteúdo da mensagem no envelope):

```json
{
  "eventId": "uuid",
  "userId": "uuid",
  "name": "Maria Silva",
  "email": "maria@example.com",
  "occurredAtUtc": "2026-10-27T12:00:00+00:00"
}
```