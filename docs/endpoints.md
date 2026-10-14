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
notificação no banco e tenta o envio imediato.

| Evento | Produtor | Fila | Descrição |
|---|---|---|---|
| `UserCreatedV1` | Identity | `notifications.user-created` | Cadastro de usuário (boas-vindas) |
| `PaymentApprovedV1` | Payments | `notifications.payment-approved` | Pagamento aprovado |
| `PaymentRejectedV1` | Payments | `notifications.payment-rejected` | Pagamento rejeitado |
| `OrderCompletedV1` | Orders | `notifications.order-completed` | Pedido concluído |
| `OrderFailedV1` | Orders | `notifications.order-failed` | Pedido falhou |

### Envelope dos eventos

```json
{
  "messageId": "2ace1057-8815-4675-babe-a672928b328c",
  "eventType": "PaymentApprovedV1",
  "occurredAt": "2026-10-14T12:00:00Z",
  "correlationId": "5ae87974-a227-4b97-9f20-3593efdad7ca",
  "causationId": null,
  "producer": "payments",
  "payload": {
    "orderId": "uuid",
    "userId": "uuid",
    "amount": 84.90
  }
}
```

O `messageId` é usado como `ExternalEventId` para deduplicação. Eventos
duplicados são ignorados sem erro.

## Respostas de erro comuns

| Status | Significado típico |
|---|---|
| `401 Unauthorized` | Token ausente, expirado ou inválido |
| `403 Forbidden` | Token válido mas sem scope necessário |
| `404 Not Found` | Notificação não encontrada |
| `422 Unprocessable Entity` | Regra de negócio violada (ex: limite de retry) |
