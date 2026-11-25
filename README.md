# LibraryManagement

REST API для управления библиотекой: каталогом книг, авторами и категориями, читателями и выдачей книг. Данные хранятся в PostgreSQL. У книги может быть несколько авторов, а количество экземпляров учитывается отдельно как общее и доступное: при выдаче доступное количество уменьшается, при возврате — увеличивается.

## Возможности

- Создание, просмотр, обновление и удаление книг, авторов и категорий.
- Частичное обновление автора через JSON Patch.
- Читатели: создание, обновление, «мягкое» удаление (деактивация) и восстановление.
- Выдача и возврат книг с учётом доступных экземпляров, статус выдачи (`Active`, `Returned`, `Overdue`) и история выдач читателя.
- Постраничное получение всех списков, кроме списка категорий.
- Фильтрация и сортировка книг и выдач с возможностью сочетать условия.
- Проверка входных данных и ответы об ошибках в формате Problem Details.
- Защита от нарушения целостности данных: нельзя удалить автора или категорию с книгами, книгу с историей выдач, а также выдать книгу без свободных экземпляров или неактивному читателю.
- Интерактивная документация Scalar и спецификация OpenAPI в режиме Development.
- Заполнение базы демонстрационными данными в режиме Development.

## Технологии

| Компонент | Использование |
| --- | --- |
| .NET 11 / ASP.NET Core | Web API: контроллеры для книг, авторов и выдач, Minimal API для категорий и читателей |
| Entity Framework Core 11 / Npgsql | Работа с PostgreSQL и миграции схемы |
| ErrorOr | Возврат ошибок из сервисов без исключений и их преобразование в HTTP-статусы |
| FluentValidation | Валидация категорий и читателей через endpoint-фильтры |
| Data Annotations | Валидация DTO книг, авторов и выдач |
| Microsoft.AspNetCore.JsonPatch | Частичное обновление авторов (`PATCH`) |
| Mapperly | Генерация преобразований между сущностями и DTO |
| OpenAPI / Scalar | Спецификация API и интерфейс для выполнения запросов |

Версии зависимостей указаны в [файле проекта](./LibraryManagement.Api/LibraryManagement.Api.csproj).

## Требования

- [.NET SDK 11](https://dotnet.microsoft.com/download/dotnet/11.0).
- Запущенный сервер [PostgreSQL](https://www.postgresql.org/download/).
- Утилита `dotnet-ef` версии 11.x для применения миграций.
- Клиент `psql` или другой инструмент для создания базы данных.

## Локальный запуск

Команды ниже выполняются из корня репозитория в PowerShell.

### 1. Восстановить зависимости

```powershell
dotnet restore
```

Если `dotnet-ef` ещё не установлен:

```powershell
dotnet tool install --global dotnet-ef --prerelease
```

Проект использует пакеты EF Core `11.0.0-rc.3`, поэтому версия `dotnet-ef` должна быть той же ветки 11.x. Проверить установленную версию можно командой `dotnet ef --version`, обновить — `dotnet tool update --global dotnet-ef --prerelease`.

### 2. Настроить PostgreSQL

Создайте базу данных, например через `psql`:

```powershell
psql -h localhost -p 5432 -U postgres -c "CREATE DATABASE librarydb;"
```

В [настройках Development](./LibraryManagement.Api/appsettings.Development.json) задано подключение к `localhost:5432`, базе `librarydb`, с пользователем и паролем `postgres`.

Если параметры вашего сервера отличаются, задайте строку подключения в текущей сессии PowerShell. Она будет использоваться и миграциями, и приложением:

```powershell
$env:ConnectionStrings__LibraryDb = "Host=localhost;Port=5432;Database=librarydb;Username=postgres;Password=postgres"
```

Ключ конфигурации — `ConnectionStrings:LibraryDb`. Переменная окружения переопределяет значения из файлов настроек. Для запуска в другом окружении подключение также нужно настроить: в [основном файле настроек](./LibraryManagement.Api/appsettings.json) оно не задано.

### 3. Применить миграции

```powershell
dotnet ef database update --project LibraryManagement.Api
```

Миграции автоматически при запуске не применяются. В режиме Development приложение сразу обращается к базе для добавления демонстрационных данных, поэтому схему нужно подготовить заранее.

### 4. Запустить API

Для HTTPS доверьте локальный сертификат разработки и запустите соответствующий профиль:

```powershell
dotnet dev-certs https --trust
dotnet run --project LibraryManagement.Api --launch-profile https
```

Адреса из [профилей запуска](./LibraryManagement.Api/Properties/launchSettings.json):

| Ресурс | Адрес |
| --- | --- |
| API по HTTPS | `https://localhost:7035` |
| API по HTTP | `http://localhost:5276` |
| Scalar | [https://localhost:7035/scalar/v1](https://localhost:7035/scalar/v1) |
| OpenAPI JSON | [https://localhost:7035/openapi/v1.json](https://localhost:7035/openapi/v1.json) |

Для запуска только по HTTP:

```powershell
dotnet run --project LibraryManagement.Api --launch-profile http
```

В этом случае Scalar доступен по адресу `http://localhost:5276/scalar/v1`. Оба профиля включают Development. В приложении подключено перенаправление на HTTPS; при использовании профиля `https` обращайтесь к HTTPS-адресу.

### Демонстрационные данные

При запуске в Development [DataSeeder](./LibraryManagement.Api/DataAccess/DataSeeder.cs) добавляет 10 авторов, 5 категорий, 10 книг и 10 читателей. Данные добавляются только если в таблицах авторов, категорий, книг и читателей ещё нет ни одной записи. Выдачи не заполняются — их можно создать через API.

## API

Все списки отсортированы по `Id`, если порядок не задан параметрами сортировки.

### Книги

| Метод | Маршрут | Действие |
| --- | --- | --- |
| GET | `/api/books` | Список книг с фильтрацией, сортировкой и пагинацией |
| GET | `/api/books/{id}` | Книга с категорией и авторами |
| POST | `/api/books` | Создать книгу |
| PUT | `/api/books/{id}` | Обновить книгу |
| DELETE | `/api/books/{id}` | Удалить книгу без выдач |

### Авторы

| Метод | Маршрут | Действие |
| --- | --- | --- |
| GET | `/api/authors` | Список авторов с пагинацией |
| GET | `/api/authors/{id}` | Автор и его книги |
| POST | `/api/authors` | Создать автора |
| PUT | `/api/authors/{id}` | Полностью обновить автора |
| PATCH | `/api/authors/{id}` | Частично обновить автора (JSON Patch) |
| DELETE | `/api/authors/{id}` | Удалить автора без привязанных книг |

### Категории

| Метод | Маршрут | Действие |
| --- | --- | --- |
| GET | `/api/categories` | Все категории без пагинации |
| GET | `/api/categories/{id}` | Категория по идентификатору |
| GET | `/api/categories/{id}/books` | Книги категории с пагинацией |
| POST | `/api/categories` | Создать категорию |
| PUT | `/api/categories/{id}` | Обновить категорию |
| DELETE | `/api/categories/{id}` | Удалить категорию без привязанных книг |

### Читатели

| Метод | Маршрут | Действие |
| --- | --- | --- |
| GET | `/api/members` | Список читателей с пагинацией и фильтром по статусу |
| GET | `/api/members/{id}` | Читатель по идентификатору |
| GET | `/api/members/{id}/loans` | История выдач читателя, от новых к старым, без пагинации |
| POST | `/api/members` | Создать читателя |
| PUT | `/api/members/{id}` | Обновить имя и фамилию читателя |
| DELETE | `/api/members/{id}` | Деактивировать читателя без невозвращённых книг |
| PATCH | `/api/members/{id}/restore` | Снова активировать читателя |

### Выдачи

| Метод | Маршрут | Действие |
| --- | --- | --- |
| GET | `/api/loans` | Список выдач с фильтрацией, сортировкой и пагинацией |
| GET | `/api/loans/{id}` | Выдача по идентификатору |
| POST | `/api/loans` | Выдать книгу читателю |
| PATCH | `/api/loans/{id}/return` | Вернуть книгу |

### Коды ответов

| Код | Когда возвращается |
| --- | --- |
| `200 OK` | Успешное чтение, обновление и возврат книги |
| `201 Created` | Успешное создание; заголовок `Location` указывает на созданный ресурс |
| `204 No Content` | Успешное удаление, деактивация или восстановление читателя |
| `400 Bad Request` | Неверные данные, несуществующая категория, автор, книга или читатель в теле запроса |
| `404 Not Found` | Запись по `id` из маршрута не найдена |
| `409 Conflict` | Нарушение бизнес-правил (подробнее — в разделах ниже) |
| `500 Internal Server Error` | Необработанное исключение, ответ в формате Problem Details |

Ошибки возвращаются в формате Problem Details с пояснением в `detail`; ошибки валидации — со словарём `errors`. Параметр `{id}` ограничен типом `int` на уровне маршрута: например, `/api/books/abc` не соответствует маршруту и возвращает `404 Not Found`.

Каждый ответ содержит заголовок `X-Response-Time-Ms` со временем обработки запроса в миллисекундах.

### Пагинация

Все списки, кроме категорий и истории выдач читателя, принимают параметры из [PaginationParameters](./LibraryManagement.Api/RequestParameters/PaginationParameters.cs):

| Параметр | По умолчанию | Ограничения |
| --- | --- | --- |
| `pageNumber` | `1` | Значения меньше 1 заменяются на 1 |
| `pageSize` | `10` | Значение ограничивается диапазоном от 1 до 50 |

Пример запроса:

```http
GET /api/books?pageNumber=1&pageSize=5
```

Ответ содержит JSON-массив записей и заголовки:

- `X-Total-Count` — общее количество записей с учётом фильтров.
- `X-Total-Pages` — общее количество страниц с учётом размера страницы.

При запросе страницы за пределами списка возвращается пустой массив.

### Фильтрация и сортировка книг

`GET /api/books` принимает необязательные параметры.

Фильтры из [BookFilteringParameters](./LibraryManagement.Api/RequestParameters/BookFilteringParameters.cs):

| Параметр | Тип | Условие отбора |
| --- | --- | --- |
| `title` | Строка | Название книги содержит указанную подстроку; пустое значение или строка только из пробелов игнорируются |
| `categoryId` | Целое число | Книга принадлежит категории с указанным идентификатором |
| `authorId` | Целое число | Среди авторов книги есть автор с указанным идентификатором |

Сортировка из [SortingParameters](./LibraryManagement.Api/RequestParameters/SortingParameters.cs):

| Параметр | Значения | По умолчанию |
| --- | --- | --- |
| `sortBy` | `Title`, `AvailableCopies` | Сортировка по `Id` |
| `sortDirection` | `Acs` (по возрастанию), `Desc` (по убыванию) | `Acs` |

Условия фильтрации можно использовать отдельно или вместе: книга должна удовлетворять каждому из них (логическое И). Фильтры применяются до подсчёта записей и пагинации, поэтому `X-Total-Count` показывает количество подходящих книг. Если совпадений нет, возвращается `200 OK` с пустым массивом `[]`, оба заголовка равны `0`.

Примеры запросов:

```http
GET /api/books?title=Good
GET /api/books?categoryId=1&sortBy=Title&sortDirection=Desc
GET /api/books?authorId=1&sortBy=AvailableCopies
GET /api/books?title=Good&categoryId=4&authorId=6&pageNumber=1&pageSize=5
```

### Правила для книг

- Название обязательно, до 200 символов. Количество экземпляров (`totalCopies`) — от 0. Нужны существующая категория и хотя бы один существующий автор; повторяющиеся `authorIds` учитываются один раз.
- При создании доступное количество экземпляров равно общему.
- При изменении `totalCopies` доступное количество меняется на ту же величину. Если новое общее количество меньше числа невозвращённых выдач книги, возвращается `409 Conflict`.
- Книгу, по которой есть выдачи (в том числе возвращённые), удалить нельзя — `409 Conflict`.

### Правила для авторов

- Имя и фамилия обязательны, до 50 символов каждое; необязательный год рождения — от −2000 до 2026.
- `PATCH /api/authors/{id}` принимает документ JSON Patch и требует заголовок `Content-Type: application/json-patch+json`. Поддерживаются поля `firstName`, `lastName` и `yearOfBirth`, значения проходят ту же проверку.
- Автора с книгами удалить нельзя — `409 Conflict`.

### Правила для категорий

Маршруты реализованы в [CategoriesEndpoints.cs](./LibraryManagement.Api/Endpoints/CategoriesEndpoints.cs) как группа Minimal API с тегом `Categories`.

- Поле `name` обязательно: не пустая строка и не только пробелы, до 50 символов. Проверка выполняется FluentValidation через [ValidationFilter](./LibraryManagement.Api/Filters/ValidationFilter.cs).
- Повтор названия возвращает `409 Conflict`. При обновлении можно оставить текущее название — проверка дубликатов исключает саму категорию.
- Категорию с книгами удалить нельзя — `409 Conflict`. Сначала нужно удалить эти книги или назначить им другую категорию.
- `GET /api/categories/{id}/books` возвращает книги категории с пагинацией и заголовками `X-Total-Count`, `X-Total-Pages`.

### Правила для читателей

Маршруты реализованы в [MembersEndpoints.cs](./LibraryManagement.Api/Endpoints/MembersEndpoints.cs).

- Имя и фамилия обязательны, до 50 символов каждое. Новый читатель всегда создаётся активным.
- `DELETE` не удаляет запись, а делает читателя неактивным; вернуть его можно через `PATCH /api/members/{id}/restore`. Деактивировать читателя с невозвращёнными книгами нельзя — `409 Conflict`.
- `GET /api/members` принимает параметр `status`: `active` (по умолчанию), `inactive` или `all`. Другое значение возвращает `400 Bad Request`.

### Правила для выдач

Логика выдачи и возврата находится в [LoansService](./LibraryManagement.Api/Services/LoansService.cs).

- Для `POST /api/loans` обязательны `bookId`, `memberId`, `loanDate` и `dueDate` (даты в формате `ГГГГ-ММ-ДД`). Дата возврата не может быть раньше даты выдачи.
- Книга и читатель должны существовать (иначе `400 Bad Request`). Если у книги нет доступных экземпляров или читатель неактивен, возвращается `409 Conflict`.
- При успешной выдаче доступное количество экземпляров книги уменьшается на единицу.
- `PATCH /api/loans/{id}/return` ставит сегодняшнюю дату в `returnDate` и возвращает экземпляр в наличие. Повторный возврат даёт `409 Conflict`.
- Статус вычисляется автоматически: `Returned` — если книга возвращена; `Overdue` — если не возвращена и срок прошёл; иначе `Active`.

`GET /api/loans` принимает параметры:

| Параметр | Значения | Условие отбора |
| --- | --- | --- |
| `loanStatus` | `Active`, `Returned`, `Overdue` | Выдачи с указанным статусом |
| `bookId` | Целое число | Выдачи указанной книги |
| `memberId` | Целое число | Выдачи указанного читателя |
| `sortBy` | `LoanDate`, `DueDate` | Поле сортировки; без него — по `Id` |
| `sortDirection` | `Acs`, `Desc` | Направление сортировки, по умолчанию `Acs` |

### Примеры запросов

Запросы можно выполнить в Scalar. Для `POST` и `PUT` используйте `Content-Type: application/json`.

Создание категории — `POST /api/categories`:

```json
{
  "name": "Detective"
}
```

Создание автора — `POST /api/authors`:

```json
{
  "firstName": "Agatha",
  "lastName": "Christie",
  "yearOfBirth": 1890
}
```

Частичное обновление автора — `PATCH /api/authors/1` с заголовком `Content-Type: application/json-patch+json`:

```json
[
  { "op": "replace", "path": "/yearOfBirth", "value": 1903 }
]
```

Создание книги — `POST /api/books`:

```json
{
  "title": "Murder on the Orient Express",
  "totalCopies": 3,
  "categoryId": 1,
  "authorIds": [1]
}
```

Создание читателя — `POST /api/members`:

```json
{
  "firstName": "Alice",
  "lastName": "Morgan"
}
```

Выдача книги — `POST /api/loans`:

```json
{
  "bookId": 1,
  "memberId": 1,
  "loanDate": "2026-11-25",
  "dueDate": "2026-12-09"
}
```

Возврат книги — `PATCH /api/loans/1/return` без тела запроса.

В примерах замените идентификаторы на значения из ответов API. Для `PUT` используются те же поля, что и для создания соответствующего ресурса.

Готовые GET-запросы для категорий также есть в [LibraryManagement.Api.http](./LibraryManagement.Api/LibraryManagement.Api.http).

## Структура проекта

| Путь | Назначение |
| --- | --- |
| [Program.cs](./LibraryManagement.Api/Program.cs) | Регистрация сервисов, middleware и маршрутов |
| [Controllers](./LibraryManagement.Api/Controllers/) | API книг, авторов и выдач |
| [Endpoints](./LibraryManagement.Api/Endpoints/) | Minimal API категорий и читателей |
| [Services](./LibraryManagement.Api/Services/) | Бизнес-логика книг, авторов и выдач и их интерфейсы |
| [Errors](./LibraryManagement.Api/Errors/) | Описание ошибок предметной области для ErrorOr |
| [Entities](./LibraryManagement.Api/Entities/) | Сущности базы данных |
| [DTOs](./LibraryManagement.Api/DTOs/) | Модели запросов и ответов |
| [DataAccess](./LibraryManagement.Api/DataAccess/) | Контекст EF Core и демонстрационные данные |
| [Migrations](./LibraryManagement.Api/Migrations/) | Миграции схемы PostgreSQL |
| [Mapping](./LibraryManagement.Api/Mapping/) | Преобразования Mapperly |
| [Validators](./LibraryManagement.Api/Validators/) | Правила FluentValidation |
| [Filters](./LibraryManagement.Api/Filters/) | Валидация Minimal API и логирование действий контроллера авторов |
| [Middleware](./LibraryManagement.Api/Middleware/) | Обработка исключений и заголовок времени ответа `X-Response-Time-Ms` |
| [RequestParameters](./LibraryManagement.Api/RequestParameters/) | Параметры пагинации, фильтрации и сортировки |
| [Common](./LibraryManagement.Api/Common/), [CustomResults](./LibraryManagement.Api/CustomResults/), [Extensions](./LibraryManagement.Api/Extensions/) | Имена заголовков ответа, результат с пагинацией и вспомогательные методы |

## Сборка

```powershell
dotnet build
```

В текущем решении отдельного проекта автоматических тестов нет.
