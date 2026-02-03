# Сервис банковских счетов

## Кратко
REST-сервис для управления банковскими счетами и транзакциями.

**Что умеет:**
- Управление счетами: создание, изменение, закрытие.
- Работа с транзакциями: пополнение, списание, переводы.
- Получение выписок и списков счетов с фильтрацией.
- Интеграция через RabbitMQ (Outbox / Inbox).
- Авторизация через Keycloak.

**Технологии:** .NET, PostgreSQL, EF Core, MediatR (CQRS), RabbitMQ, Keycloak.

---

## Содержание
1. [Общая информация](#1-общая-информация)
2. [Быстрый старт](#2-быстрый-старт)
   - [Запуск через Docker](#21-запуск-через-docker-compose-рекомендуется)
   - [Локальный запуск](#22-локальный-запуск-без-docker)
3. [Авторизация и Swagger](#3-авторизация-и-swagger)
4. [Архитектура проекта](#4-архитектура-проекта)
5. [REST API](#5-rest-api)
   - [Счета](#51-счета)
   - [Транзакции](#52-транзакции)
   - [Выписки](#53-выписка-по-счету)
6. [RabbitMQ и сообщения](#6-rabbitmq-и-сообщения)
7. [Диагностика и проверки](#7-диагностика-и-проверки)
8. [Интеграционные тесты](#8-интеграционные-тесты)

---

## 1. Общая информация
Сервис предоставляет REST API для управления банковскими счетами и транзакциями.

Поддерживаемые типы счетов:
- Deposit (депозитный)
- Checking / Current (расчетный)
- Credit (кредитный)

---

## 2. Быстрый старт

### 2.1 Запуск через Docker Compose (рекомендуется)

```bash
docker-compose up -d --build
```

Swagger:
```
http://localhost:80/swagger
```

---

### 2.2 Локальный запуск (без Docker)

**Требования:**
- PostgreSQL: `localhost:5432`
- Keycloak: `http://localhost:8080`
- RabbitMQ: `localhost:5672`

```bash
dotnet run --project bank-accounts
```

Swagger:
```
http://localhost:5209/swagger
```

---

## 3. Авторизация и Swagger

1. Открыть Swagger.
2. Нажать **Authorize**.
3. `client_id` и `client_secret` подставляются автоматически.
4. Keycloak:
   - Login: `user`
   - Password: `password`
5. JWT автоматически добавляется к запросам.

---

## 4. Архитектура проекта

- **Features** — бизнес-логика (`Accounts`, `Transactions`)
- **Infrastructure** — EF Core, репозитории
- **Services** — валидации и вспомогательные сервисы

Используется:
- MediatR (CQRS)
- FluentValidation
- EF Core + PostgreSQL
- RabbitMQ
- Keycloak

---

## 5. REST API

### 5.1 Счета

#### Создать счет
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

**Ответ (201):**
```json
{
  "accountId": "GUID"
}
```

---

#### Изменить счет
`PATCH /accounts/{id}`

```json
{
  "interestRate": 4.5
}
```

---

#### Удалить или закрыть счет

- Полное удаление:
`DELETE /accounts/{id}`

- Закрытие (soft):
`PATCH /accounts/{id}/close`

---

#### Получить список счетов
`GET /accounts?ownerId=123&page=1&pageSize=20`

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

### 5.2 Транзакции

#### Зарегистрировать транзакцию
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

**Ответ (201):**
```json
{
  "transactionIds": ["GUID"]
}
```

---

#### Перевод между счетами

```json
{
  "accountId": "GUID",
  "counterpartyAccountId": "GUID",
  "currency": "EUR",
  "value": 100,
  "type": "Credit",
  "date": "2025-03-12T11:30:19"
}
```

**Ответ:** два `transactionId`.

---

### 5.3 Выписка по счету

`GET /accounts/{accountId}/statement?start=2025-03-01&end=2025-03-31`

```json
{
  "accountId": "GUID",
  "transactions": [
    {
      "id": "GUID",
      "type": "Credit",
      "value": 200.0,
      "date": "2025-03-05T14:30:00"
    }
  ],
  "totalCredits": 200.0,
  "totalDebits": 50.0
}
```

---

## 6. RabbitMQ и сообщения

### RabbitMQ UI
- `http://localhost:15672`
- `guest / guest`

### Outbox

События:
- AccountOpened
- InterestAccrued
- MoneyCredited
- MoneyDebited
- TransferCompleted

### Inbox

- `inbox_consumed`
- `inbox_dead_letters`

---

## 7. Диагностика и проверки

### Explain Analyze

Endpoint в Swagger для анализа запроса выписки.

⚠️ Индекс `(AccountId, Date)` применяется только при отсутствии отдельного индекса по дате.

---

## 8. Интеграционные тесты

⚠️ Тесты могут падать при одновременно запущенных Docker-контейнерах.

Рекомендуется использовать один режим запуска (Docker **или** локально).

