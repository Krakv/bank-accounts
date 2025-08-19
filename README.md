# Сервис банковские счета

## Краткое описание

Сервис предоставляет REST API для управления банковскими счетами и транзакциями. Основные возможности:
- Создание, изменение и закрытие счетов (депозитных, кредитных, расчетных).
- Регистрация транзакций (пополнение, списание, переводы между счетами).
- Получение выписок по счетам.
- Получение отфильтрованного списка счетов.

## Проверка Explain Analyze

В swagger доступен endpoint, который возвращает результат анализа запроса получения выписки по счету. (Обычно он возвращает результат поиска с использованием индекса GiST по дате. Составной индекс (AccountId, Date) удается применить, только если удалить индекс по дате).

## RabbitMQ

RabbitMQ UI доступен по адресу localhost: `http://localhost:15672`. Login: guest. Password: guest.

## Проверка сохраненных сообщений для RabbitMQ

Event контроллер позволит получить список записей в Outbox, InboxConsumed, InboxDeadLetters таблицах. Для тестирования блокирования клиента, есть 2 endpoint для блокировки и разблокировки.

## Важная информация по интеграционным тестам

Интеграционные тесты могут упасть из-за работающих приложений в докере, которые автоматически запущенны Visual Studio для bank-accounts (rabbitmq, postgres...).

## Пошаговая инструкция запуска

### Запуск с docker compose

В проекте "docker-compose" в Visual Studio использовать профиль запуска Docker Compose.

Или в корне проекта запустить командой
```bash
docker-compose up -d --build
```

Страница swagger будет доступна по адресу: `http://localhost:80/swagger`

Чтобы авторизоваться, необходимо нажать кнопку Authorize на странице swagger. client_secret и cliend_id должны подставиться сами, иначе они указаны на странице swagger. Затем откроется страница keycloak `http://localhost:8080`, в которой потребуется ввести логин (`user`) и пароль (`password`). После ввода данных, происходит перенаправление на страницу swagger и авторизация успешно завершается, JWT должен подставляться к запросам через swagger.

### Локальный запуск (без Docker)

**Требования:**
- PostgreSQL запущена на `localhost:5432`
- Keycloak доступен на `http://localhost:8080`
- RabbitMq доступен на `http://localhost:5672`

*Требуется конфигурация, используемая в docker-compose.*

Профиль запуска в Visual Studio - **http**.

Или запустить командой 
```bash
dotnet run --project bank-accounts
```

Страница swagger будет доступна по адресу: `http://localhost:5209/swagger`

Чтобы авторизоваться, необходимо нажать кнопку Authorize на странице swagger. client_secret и cliend_id должны подставиться сами, иначе они указаны на странице swagger. Затем откроется страница keycloak `http://localhost:8080`, в которой потребуется ввести логин (`user`) и пароль (`password`). После ввода данных, происходит перенаправление на страницу swagger и авторизация успешно завершается, JWT должен подставляться к запросам через swagger.

## Архитектура проекта

- **Features** — бизнес-логика:
  - `Accounts`, `Transactions` — сущности с их командами и запросами;
- **Infrastructure** — EF Core, репозитории;
- **Services** — вспомогательные сервисы, валидации;

Используются:
- `MediatR` для CQRS
- `FluentValidation`
- `EF Core` + `PostgreSQL`
- `RabbitMQ`
- `Keycloak`

## REST API

Полное описание доступно в Swagger.

## Формат сообщений для rabbitmq

### Outbox

#### Outbox сообщения хранятся в таблице Outbox со следующей структурой:

```sql
CREATE TABLE "Outbox" (
    "Id" UUID PRIMARY KEY,
    "Type" VARCHAR(255) NOT NULL,
    "Payload" JSONB NOT NULL,
    "OccurredAt" TIMESTAMP NOT NULL,
    "ProcessedAt" TIMESTAMP NULL,
    "Source" VARCHAR(100) NOT NULL,
    "CorrelationId" UUID NOT NULL,
    "CausationId" UUID NOT NULL
);
```
#### Базовый формат Outbox cообщения (таблица):
```json
{
  "Id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Type": "AccountOpened",
  "Payload": {
    "EventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "OccuredAt": "2023-10-25T14:30:45.123Z",
    "Meta": {
      "Version": "v1",
      "Source": "account-service",
      "CorrelationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "CausationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    },
    // Специфичные поля события
    "AccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "OwnerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "Currency": "USD",
    "Type": "Savings"
  },
  "OccurredAt": "2023-10-25T14:30:45.123Z",
  "ProcessedAt": null,
  "Source": "account-service",
  "CorrelationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "CausationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### Формат Outbox Payload (сообщение rabbitmq) для разных типов событий:
AccountOpened:

```json
{
  "EventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "OccuredAt": "2023-10-25T14:30:45.123Z",
  "Meta": {
    "Version": "v1",
    "Source": "account-service",
    "CorrelationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "CausationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  },
  "AccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "OwnerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Currency": "USD",
  "Type": "Savings"
}
```
InterestAccrued:

```json
{
  "EventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "OccuredAt": "2023-10-25T14:30:45.123Z",
  "Meta": {
    "Version": "v1",
    "Source": "account-service",
    "CorrelationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "CausationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  },
  "AccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "PeriodFrom": "2023-10-01",
  "PeriodTo": "2023-10-31",
  "Amount": 15.75
}
```
MoneyCredited:

```json
{
  "EventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "OccuredAt": "2023-10-25T14:30:45.123Z",
  "Meta": {
    "Version": "v1",
    "Source": "account-service",
    "CorrelationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "CausationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  },
  "AccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Amount": 100.00,
  "Currency": "USD",
  "OperationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```
MoneyDebited:

```json
{
  "EventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "OccuredAt": "2023-10-25T14:30:45.123Z",
  "Meta": {
    "Version": "v1",
    "Source": "account-service",
    "CorrelationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "CausationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  },
  "AccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Amount": 50.00,
  "Currency": "USD",
  "OperationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Reason": "Monthly fee"
}
```
TransferCompleted:

```json
{
  "EventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "OccuredAt": "2023-10-25T14:30:45.123Z",
  "Meta": {
    "Version": "v1",
    "Source": "account-service",
    "CorrelationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "CausationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  },
  "SourceAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "DestinationAccountId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "Amount": 200.00,
  "Currency": "USD",
  "TransferId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### Inbox

#### Базовый формат Inbox cообщения (таблица)

1. Обработанные сообщения (inbox_consumed)
```json
{
  "Id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "ProcessedAt": "2023-10-25T14:30:45.123Z",
  "Handler": "ClientBlockingHandler"
}
```
2. Некорректные сообщения (inbox_dead_letters)
```json
{
  "Id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "ReceivedAt": "2023-10-25T14:30:45.123Z",
  "Handler": "ClientBlockingHandler",
  "Payload": "{ /* оригинальное сообщение */ }",
  "Error": "Ошибка валидации: поле ClientId обязательно"
}
```
#### Формат Inbox Payload (сообщение rabbitmq)

ClientBlockingPayload:

```json
{
  "EventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "OccuredAt": "2023-10-25T14:30:45.123Z",
  "Meta": {
    "Version": "v1",
    "Source": "client.events",
    "CorrelationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "CausationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  },
  "ClientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

## Функции

### 1. Создать счет
`POST /accounts`

Запрос:
```json
{
  "ownerId": "GUID",     // GUID
  "type": "Deposit",     // "Deposit" | "Checking" | "Credit"
  "currency": "EUR",     // ISO 4217 ("USD", "EUR", "RUB")
  "interestRate": 3.0,   // Опционально, decimal > 0; для Deposit/Credit
}
```
Ответ (201 Created) - возвращает accountId
Ответ (400 Bad Request) - ошибки валидации.
### 2. Изменить счет
`PATCH /accounts/{id}`

Запрос:
```json
{
 "interestRate": null // Опционально, decimal > 0; для Deposit/Credit
}
```
Ответ (200 Ok)
### 3. Удалить счет (и закрыть счет)
*Полное удаление счёта из системы.*

`DELETE /accounts/{id}`  

Ответ (200 Ok) - счет удален.

Ответ (400 BadRequest) - ошибки валидации.

**Если требуется закрыть счет:**

*Информация о счете не удаляется, добавляется значение для поля closingDate, и счет считается закрытым.*

`PATCH /accounts/{id}/close`

Ответ (200 Ok) - счет закрыт.

Ответ (400 BadRequest) - ошибки валидации.

### 4. Получить список счетов

*По умолчанию возвращаются все счета, отсортированные по дате, требуется использовать фильтрацию*

```http
GET /accounts?ownerId=123&page=1&pageSize=20
```
```http
GET /accounts?accountIds=1,5,10&page=1&pageSize=10
```
```http
GET /accounts?ownerId=123&type=Deposit&currency=EUR&page=2&pageSize=50
```
Ответ (200 Ok):
```json
{
  "accounts": [
    {
      "id": "GUID",
      "ownerId": 123,
      "type": "Deposit",
      "currency": "EUR",
      "balance": 1000,
      "interestRate": null,
      "openingDate": "2025-03-12",
      "closingDate": null
    },
    {
      "id": "GUID",
      "ownerId": 123,
      "type": "Current",
      "currency": "USD",
      "balance": 500,
      "interestRate": null,
      "openingDate": "2025-03-10",
      "closingDate": null
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 35,
    "totalPages": 2
  }
}
```
Ответ (400 Bad Request) - ошибки валидации.

### 5. Зарегистрировать транзакцию по счету
`POST /transactions`

Запрос:
```json
{
	"accountId": "GUID",
	"counterpartyAccountId": null,
	"currency": "EUR",
	"value": 100,
	"type": "Credit", // "Deposit" | "Credit"
	"description": "",
	"date": "2025-03-12T11:30:19"
}
```
Ответ (201 Created)  - возвращает массив с transactionId

Ответ (400 Bad Request) - ошибки валидации.

### 6.  Выполнить перевод между счётами
`POST /transactions`

Запрос:
```json
{
	"accountId": "GUID",
	"counterpartyAccountId": "GUID",
	"currency": "EUR",
	"value": 100,
	"type": "Credit",
	"description": "",
	"date": "2025-03-12T11:30:19"
}
```
Ответ (201 Created)  - возвращает массив с двумя transactionId

Ответ (400 Bad Request) - ошибки валидации.

### 7. Получить выписку
`GET /accounts/{accountId}/statement?start=2025-03-01&end=2025-03-31`

Ответ (200 Ok):
```json
{
  "accountId": "GUID",
  "ownerId": "GUID",
  "currency": "EUR",
  "startDate": "2025-03-01",
  "endDate": "2025-03-31",
  "transactions": [
    {
      "id": "GUID",
      "type": "Credit",
      "value": 200.00,
      "description": "Пополнение счёта",
      "date": "2025-03-05T14:30:00",
      "counterpartyAccountId": null
    },
    {
      "id": "GUID",
      "type": "Debit",
      "value": 50.00,
      "description": "Перевод в сбережения",
      "date": "2025-03-10T09:15:00",
      "counterpartyAccountId": 1
    }
  ],
  "totalCredits": 200.00,
  "totalDebits": 50.00
}
```
Ответ (400 Bad Request) - ошибки валидации.

### 8. Проверка счетов у клиента
`GET /accounts?ownerId=123`

Ответ (200 Ok):
```json
{
  "accounts": [
    {
      "id": "GUID",
      "ownerId": 123,
      "type": "Deposit",
      "currency": "EUR",
      "balance": 1000,
      "interestRate": null,
      "openingDate": "2025-03-12",
      "closingDate": null
    },
    {
      "id": "GUID",
      "ownerId": 123,
      "type": "Deposit",
      "currency": "USD",
      "balance": 500,
      "interestRate": null,
      "openingDate": "2025-03-10",
      "closingDate": null
    }
  ]
}
```
Ответ (400 Bad Request) - ошибки валидации.
