# Сервис банковских счетов

## Кратко
REST‑сервис для управления банковскими счетами и транзакциями.

**Основные возможности:**
- Создание, изменение, закрытие и удаление счетов (депозитные, расчетные, кредитные).
- Регистрация транзакций: пополнение, списание, переводы между счетами.
- Получение выписок по счетам.
- Получение списков счетов с фильтрацией и пагинацией.
- Асинхронная интеграция через RabbitMQ (Outbox / Inbox).
- Авторизация через Keycloak (OAuth2 / JWT).

**Технологический стек:**
.NET · EF Core · PostgreSQL · MediatR (CQRS) · RabbitMQ · Keycloak

---

## Содержание
1. [Общая информация](#1-общая-информация)
2. [Быстрый старт](#2-быстрый-старт)
   - [Запуск через Docker Compose](#21-запуск-через-docker-compose)
   - [Локальный запуск](#22-локальный-запуск-без-docker)
3. [Авторизация и Swagger](#3-авторизация-и-swagger)
4. [Архитектура проекта](#4-архитектура-проекта)
5. [REST API](#5-rest-api)
   - [Счета](#51-счета)
   - [Транзакции](#52-транзакции)
   - [Выписка по счету](#53-выписка-по-счету)
6. [RabbitMQ и сообщения](#6-rabbitmq-и-сообщения)
   - [Outbox](#61-outbox)
   - [Inbox](#62-inbox)
7. [Диагностика и проверки](#7-диагностика-и-проверки)
8. [Интеграционные тесты](#8-интеграционные-тесты)

---

## 1. Общая информация

Сервис предоставляет REST API для управления банковскими счетами и связанными с ними транзакциями.

**Типы счетов:**
- **Deposit** — депозитный счет (может иметь процентную ставку)
- **Checking** — расчетный счет
- **Credit** — кредитный счет (может иметь процентную ставку)

---

## 2. Быстрый старт

## 2.1 Запуск через Docker Compose

Рекомендуемый способ запуска.

В Visual Studio:
- проект `docker-compose`
- профиль запуска **Docker Compose**

Или из корня проекта:
```bash
docker-compose up -d --build
```

Swagger UI будет доступен:
```
http://localhost:80/swagger
```

---

## 2.2 Локальный запуск (без Docker)

**Требования:**
- PostgreSQL: `localhost:5432`
- Keycloak: `http://localhost:8080`
- RabbitMQ: `localhost:5672`

Используется конфигурация, эквивалентная `docker-compose`.

Запуск:
```bash
dotnet run --project bank-accounts
```

Swagger UI:
```
http://localhost:5209/swagger
```

---

## 3. Авторизация и Swagger

1. Открыть Swagger.
2. Нажать кнопку **Authorize**.
3. `client_id` и `client_secret` подставляются автоматически (либо указаны в Swagger).
4. Происходит редирект в Keycloak: `http://localhost:8080`
   - Login: `user`
   - Password: `password`
5. После успешной авторизации JWT автоматически добавляется ко всем запросам.

---

## 4. Архитектура проекта

**Структура:**
- **Features** — бизнес‑логика и CQRS:
  - `Accounts`
  - `Transactions`
- **Infrastructure** — EF Core, репозитории, БД
- **Services** — валидации и вспомогательные сервисы

**Используемые библиотеки и подходы:**
- MediatR (CQRS)
- FluentValidation
- EF Core + PostgreSQL
- RabbitMQ
- Keycloak

---

## 5. REST API

Полное описание и контракты доступны в Swagger.

---

## 5.1 Счета

### 1. Создать счет
`POST /accounts`

**Запрос:**
```json
{
  "ownerId": "GUID",
  "type": "Deposit",
  "currency": "EUR",
  "interestRate": 3.0
}
```

**Ответ (201 Created):**
```json
{
  "accountId": "GUID"
}
```

---

### 2. Изменить счет
`PATCH /accounts/{id}`

```json
{
  "interestRate": 4.5
}
```

**Ответ (200 OK)**

---

### 3. Удалить или закрыть счет

Полное удаление:
`DELETE /accounts/{id}`

Закрытие (soft‑close, данные сохраняются):
`PATCH /accounts/{id}/close`

---

### 4. Получить список счетов

Примеры запросов:
```http
GET /accounts?ownerId=123&page=1&pageSize=20
GET /accounts?accountIds=1,5,10&page=1&pageSize=10
GET /accounts?ownerId=123&type=Deposit&currency=EUR&page=2&pageSize=50
```

**Ответ:**
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

---

## 5.2 Транзакции

### 5. Зарегистрировать транзакцию
`POST /transactions`

```json
{
  "accountId": "GUID",
  "counterpartyAccountId": null,
  "currency": "EUR",
  "value": 100,
  "type": "Credit",
  "description": "",
  "date": "2025-03-12T11:30:19"
}
```

**Ответ (201 Created):**
```json
{
  "transactionIds": ["GUID"]
}
```

---

### 6. Перевод между счетами

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

**Ответ (201 Created):**
```json
{
  "transactionIds": ["GUID", "GUID"]
}
```

---

## 5.3 Выписка по счету

### 7. Получить выписку

`GET /accounts/{accountId}/statement?start=2025-03-01&end=2025-03-31`

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
    }
  ],
  "totalCredits": 200.00,
  "totalDebits": 50.00
}
```

---

## 6. RabbitMQ и сообщения

### RabbitMQ UI
- URL: `http://localhost:15672`
- Login / Password: `guest / guest`

---

## 6.1 Outbox

### Таблица Outbox

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

### Базовый формат Outbox сообщения

```json
{
  "Id": "UUID",
  "Type": "AccountOpened",
  "Payload": { /* событие */ },
  "OccurredAt": "2023-10-25T14:30:45.123Z",
  "ProcessedAt": null,
  "Source": "account-service",
  "CorrelationId": "UUID",
  "CausationId": "UUID"
}
```

### Поддерживаемые события
- AccountOpened
- InterestAccrued
- MoneyCredited
- MoneyDebited
- TransferCompleted

---

## 6.2 Inbox

### inbox_consumed
```json
{
  "Id": "UUID",
  "ProcessedAt": "2023-10-25T14:30:45.123Z",
  "Handler": "ClientBlockingHandler"
}
```

### inbox_dead_letters
```json
{
  "Id": "UUID",
  "ReceivedAt": "2023-10-25T14:30:45.123Z",
  "Handler": "ClientBlockingHandler",
  "Payload": "{ /* original message */ }",
  "Error": "Ошибка валидации"
}
```

---

## 7. Диагностика и проверки

### Explain Analyze

В Swagger доступен endpoint анализа запроса получения выписки.

Примечание:
- По умолчанию используется GiST индекс по дате
- Составной индекс `(AccountId, Date)` применяется только при удалении отдельного индекса по дате

---

## 8. Интеграционные тесты

⚠️ Интеграционные тесты могут падать при одновременно запущенных Docker‑контейнерах (Postgres, RabbitMQ и др.).

Рекомендуется использовать один режим запуска: **Docker** или **локально**.

