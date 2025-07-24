# Сервис банковские счета
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
Ответ (201 Created):
```json
{
  "id": "GUID",
  "ownerId": "GUID",
  "type": "Deposit",      
  "currency": "EUR",
  "balance": 0.00,
  "interestRate": 3.0,
  "openingDate": "2025-03-12",
  "closingDate": null
}
```
Ответ (400 Bad Request): невалидные данные.
### 2. Изменить счет
`PATCH /accounts/{id}`

Запрос:
```json
{
 "interestRate": null // Опционально, decimal > 0; для Deposit/Credit
}
```
Ответ (200 Ok):
```json
{
  "id": "GUID",
  "ownerId": "GUID",
  "type": "Deposit",
  "currency": "EUR",
  "balance": 0.00,
  "interestRate": 3.0,
  "openingDate": "2025-03-12",
  "closingDate": null
}
```
### 3. Удалить счет (Закрыть счет)
*Информация о счете не удаляется, добавляется значение для поля closingDate, и счет считается закрытым.*

`DELETE /accounts/{id}`  

Ответ (200 Ok):
```json
{
  "id": "GUID",
  "ownerId": "GUID",
  "type": "Deposit",
  "currency": "EUR",
  "balance": 123.00,
  "interestRate": 3.0,
  "openingDate": "2025-03-12",
  "closingDate": "2026-03-12"
}
```
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
Ответ (400 Bad Request): невалидные данные.

Ответ (404 Not Found): счёт не найден.
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
Ответ (201 Created):
```json
{
	"transactionId": "GUID",
	"accountId": "GUID",
	"counterpartyAccountId": null,
	"currency": "EUR",
	"value": 100,
	"type": "Credit", // "Deposit" | "Credit"
	"description": "",
	"date": "2025-03-12T11:30:19"
}
```
Ответ (400 Bad Request): невалидные данные.

Ответ (404 Not Found): транзакция не найдена.

Ответ (422 Unprocessable Entity): недостаточно средств.
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
Ответ (201 Created):
```json
{
	"transactionId": "GUID",
	"accountId": "GUID",
	"counterpartyAccountId": "GUID",
	"currency": "EUR",
	"value": 100,
	"type": "Credit",
	"description": "",
	"date": "2025-03-12T11:30:19"
}
```
Ответ (400 Bad Request): невалидные данные.

Ответ (404 Not Found): транзакция не найдена.

Ответ (422 Unprocessable Entity): недостаточно средств.
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
  "openingBalance": 100.00,
  "closingBalance": 350.00,
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
Ответ (400 Bad Request): невалидные параметры запроса, не указаны обязательные параметры start или end.

Ответ (404 Not Found): счет не найден.

Ответ (422 Unprocessable Entity): дата начала периода позже даты окончания.
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
Ответ (400 Bad Request): невалидные данные.

Ответ (404 Not Found): счёт не найден.
