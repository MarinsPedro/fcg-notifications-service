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

O Worker registra cinco consumers MassTransit com filas individuais. Os nomes das exchanges foram conferidos no codigo atual e ficam nos `.env` do orquestrador.

| Evento | Produtor | Exchange | Fila |
|---|---|---|---|
| `UserCreatedEvent` | Identity | `identity.user-created.v1` | `notifications.user-created` |
| `PaymentApprovedEventV1` | Payments | `payments.payment-approved.v1` | `notifications.payment-approved` |
| `PaymentRejectedEventV1` | Payments | `payments.payment-rejected.v1` | `notifications.payment-rejected` |
| `OrderCompletedEventV1` | Orders | `orders.order-completed.v1` | `notifications.order-completed` |
| `OrderFailedEventV1` | Orders (consumer preparado) | `orders.order-failed.v1` | `notifications.order-failed` |

Os contratos atuais possuem `EventId` para idempotencia, `OrderId`/`UserId` para rastreamento e metadados adicionais. Os eventos sao tratados como mensagens do MassTransit, **nao como JSON puro com `payments.paid`/`payments.failed`**.

A revisao do Orders recebido confirmou publicacao do evento `OrderCompletedEventV1`, mas nao identificou publicacao de `OrderFailedEventV1`. O simples fato de existir uma fila/consumer nao prova que o produtor publique esse tipo de evento; valide separadamente esse fluxo.

O `LogNotificationSender` e um mecanismo simulado de entrega (logs), nao um gateway de email real.
