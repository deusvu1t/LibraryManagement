# LibraryManagement

REST API для управления библиотечным каталогом: книгами, авторами и категориями. Приложение хранит данные в PostgreSQL, поддерживает книги с несколькими авторами и учитывает общее и доступное количество экземпляров.

## Возможности

- Создание, просмотр, обновление и удаление книг, авторов и категорий.
- Просмотр книг автора и информации об авторах и категории книги.
- Постраничное получение списков книг и авторов.
- Фильтрация книг по части названия, категории и автору с возможностью сочетать условия.
- Проверка входных данных и ответы об ошибках в формате Problem Details.
- Запрет удаления авторов и категорий, к которым привязаны книги.
- Интерактивная документация Scalar и спецификация OpenAPI в режиме Development.
- Заполнение базы демонстрационными данными в режиме Development.

Операции выдачи и возврата книг пока не реализованы. При создании книги доступное количество экземпляров равно общему; при обновлении общего количества доступное изменяется на ту же величину.

## Технологии

| Компонент | Использование |
| --- | --- |
| .NET 10 / ASP.NET Core | Web API: контроллеры для книг и авторов, Minimal API для категорий |
| Entity Framework Core 10 / Npgsql | Работа с PostgreSQL и миграции схемы |
| FluentValidation | Валидация категорий через endpoint-фильтры |
| Data Annotations | Валидация DTO книг и авторов |
| Mapperly | Генерация преобразований между сущностями и DTO |
| OpenAPI / Scalar | Спецификация API и интерфейс для выполнения запросов |

Версии зависимостей указаны в [файле проекта](./LibraryManagement.Api/LibraryManagement.Api.csproj).

## Требования

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- Запущенный сервер [PostgreSQL](https://www.postgresql.org/download/).
- Утилита `dotnet-ef` версии 10.x для применения миграций.
- Клиент `psql` или другой инструмент для создания базы данных.

## Локальный запуск

Команды ниже выполняются из корня репозитория в PowerShell.

### 1. Восстановить зависимости

```powershell
dotnet restore
```

Если `dotnet-ef` ещё не установлен:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.14
```

Если инструмент уже установлен, проверить версию можно командой `dotnet ef --version`, а обновить — `dotnet tool update --global dotnet-ef --version 10.0.14`.

### 2. Настроить PostgreSQL

Создайте базу данных, например через `psql`:

```powershell
psql -h localhost -p 5432 -U postgres -c "CREATE DATABASE librarydb;"
```

В [настройках Development](./LibraryManagement.Api/appsettings.Development.json) задано подключение к `localhost:5432`, базе `librarydb`, с пользователем и паролем `postgres`.

Задайте строку подключения в текущей сессии PowerShell. Она будет использоваться и миграциями, и приложением; при необходимости замените параметры своими:

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

При запуске в Development [DataSeeder](./LibraryManagement.Api/DataAccess/DataSeeder.cs) добавляет недостающие демонстрационные записи: 7 авторов, 5 категорий и 9 книг. Уже существующие авторы проверяются по имени и фамилии, категории — по названию, книги — по заголовку.

## API

| Метод | Маршрут | Действие |
| --- | --- | --- |
| GET | `/api/books` | Список книг с фильтрацией и пагинацией |
| GET | `/api/books/{id}` | Книга с категорией и авторами |
| POST | `/api/books` | Создать книгу |
| PUT | `/api/books/{id}` | Обновить книгу |
| DELETE | `/api/books/{id}` | Удалить книгу |
| GET | `/api/authors` | Список авторов с пагинацией |
| GET | `/api/authors/{id}` | Автор и его книги |
| POST | `/api/authors` | Создать автора |
| PUT | `/api/authors/{id}` | Обновить автора |
| DELETE | `/api/authors/{id}` | Удалить автора без привязанных книг |
| GET | `/api/categories` | Все категории |
| GET | `/api/categories/{id}` | Категория по идентификатору |
| POST | `/api/categories` | Создать категорию |
| PUT | `/api/categories/{id}` | Обновить категорию |
| DELETE | `/api/categories/{id}` | Удалить категорию без привязанных книг |

Успешное создание возвращает `201 Created` с заголовком `Location`, обновление — `200 OK`, удаление — `204 No Content`. При неверных данных возвращается `400 Bad Request`, для отсутствующей записи — `404 Not Found`. Повтор названия категории и попытка удалить автора или категорию с книгами возвращают `409 Conflict`.

### Пагинация

Списки книг и авторов принимают параметры из [PaginationParameters](./LibraryManagement.Api/RequestParameters/PaginationParameters.cs):

| Параметр | По умолчанию | Ограничения |
| --- | --- | --- |
| `pageNumber` | `1` | Значения меньше 1 заменяются на 1 |
| `pageSize` | `10` | Значение ограничивается диапазоном от 1 до 50 |

Пример запроса:

```http
GET /api/books?pageNumber=1&pageSize=5
```

Ответ содержит JSON-массив записей, отсортированных по `Id`, и заголовки:

- `X-Total-Count` — общее количество записей.
- `X-Total-Pages` — общее количество страниц с учётом размера страницы.

При запросе страницы за пределами списка возвращается пустой массив. Список категорий возвращается целиком, без пагинации.

### Фильтрация книг

`GET /api/books` принимает необязательные query-параметры из [BookFilteringParameters](./LibraryManagement.Api/RequestParameters/BookFilteringParameters.cs). Фильтрация реализована в [BooksController](./LibraryManagement.Api/Controllers/BooksController.cs).

| Параметр | Тип | Условие отбора |
| --- | --- | --- |
| `title` | Строка | Название книги содержит указанную подстроку (`Contains`); пустое значение или строка только из пробелов игнорируются |
| `categoryId` | Целое число | Книга принадлежит категории с указанным идентификатором |
| `authorId` | Целое число | Среди авторов книги есть автор с указанным идентификатором |

Условия можно использовать отдельно или вместе. Если указано несколько фильтров, книга должна удовлетворять каждому из них (логическое И). Без фильтров возвращается общий список книг с пагинацией.

Примеры запросов:

```http
GET /api/books?title=Murder
GET /api/books?categoryId=1
GET /api/books?authorId=1
GET /api/books?title=Murder&categoryId=1&authorId=1&pageNumber=1&pageSize=5
```

Фильтры применяются до подсчёта записей и пагинации. Поэтому `X-Total-Count` показывает количество книг, соответствующих всем условиям, а `X-Total-Pages` — количество страниц отфильтрованного списка. Результаты по-прежнему сортируются по `Id`. Если совпадений нет, возвращается `200 OK` с пустым массивом `[]`, оба заголовка равны `0`.

### Категории

Маршруты реализованы в [CategoriesEndpoints.cs](./LibraryManagement.Api/Endpoints/CategoriesEndpoints.cs) как группа Minimal API `/api/categories` с тегом `Categories` в OpenAPI. Обработчики используют `TypedResults`: типы успешных ответов выводятся из сигнатур, а ответы об ошибках дополнительно описаны через `ProducesProblem`.

| Запрос | Успешный ответ | Ошибки |
| --- | --- | --- |
| `GET /api/categories` | `200 OK`, массив объектов с полями `id` и `name`; при отсутствии категорий — `[]` | — |
| `GET /api/categories/{id}` | `200 OK`, объект с полями `id` и `name` | `404`, если категория не найдена |
| `POST /api/categories` | `201 Created`, созданная категория и заголовок `Location` | `400` при ошибке валидации; `409`, если название уже занято |
| `PUT /api/categories/{id}` | `200 OK`, обновлённая категория | `400` при ошибке валидации; `404`, если категория не найдена; `409`, если название занято другой категорией |
| `DELETE /api/categories/{id}` | `204 No Content`, без тела ответа | `404`, если категория не найдена; `409`, если к ней привязаны книги |

Например, тело успешного ответа при создании категории:

```json
{
  "id": 6,
  "name": "Detective"
}
```

Значение `id` назначает база данных. Заголовок `Location` указывает на `GET /api/categories/{id}` через именованный маршрут `GetCategoryById`.

Для `POST` и `PUT` требуется JSON-объект с полем `name`: строка не должна быть пустой или состоять только из пробелов, максимальная длина — 50 символов. Проверка выполняется FluentValidation через [ValidationFilter](./LibraryManagement.Api/Filters/ValidationFilter.cs). Ошибки валидации возвращаются в формате Validation Problem Details со словарём `errors`, где ключ `Name` содержит массив сообщений. Ошибки отсутствующей категории и конфликта возвращаются в формате Problem Details с пояснением в `detail`.

При обновлении можно оставить текущее название: проверка дубликатов исключает саму категорию. Перед удалением категории с книгами нужно удалить эти книги или назначить им другую категорию.

Параметр `{id}` ограничен типом `int` на уровне маршрута. Например, запрос `/api/categories/abc` не соответствует маршруту и возвращает `404 Not Found`.

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

Создание книги — `POST /api/books`:

```json
{
  "title": "Murder on the Orient Express",
  "totalCopies": 3,
  "categoryId": 1,
  "authorIds": [1]
}
```

В примере книги замените `categoryId` и `authorIds` идентификаторами нужных записей из ответов API. Для `PUT` используются те же поля, что и для создания соответствующего ресурса.

Название категории обязательно и ограничено 50 символами. Имя и фамилия автора обязательны и ограничены 50 символами каждое; необязательный год рождения — от −2000 до 2026. Название книги обязательно и ограничено 200 символами, количество экземпляров неотрицательно. Для книги нужны существующая категория и хотя бы один существующий автор.

Готовые GET-запросы для категорий также есть в [LibraryManagement.Api.http](./LibraryManagement.Api/LibraryManagement.Api.http).

## Структура проекта

| Путь | Назначение |
| --- | --- |
| [Program.cs](./LibraryManagement.Api/Program.cs) | Регистрация сервисов, middleware и маршрутов |
| [Controllers](./LibraryManagement.Api/Controllers/) | API книг и авторов |
| [Endpoints](./LibraryManagement.Api/Endpoints/) | Minimal API категорий |
| [Entities](./LibraryManagement.Api/Entities/) | Сущности базы данных |
| [DTOs](./LibraryManagement.Api/DTOs/) | Модели запросов и ответов |
| [DataAccess](./LibraryManagement.Api/DataAccess/) | Контекст EF Core и демонстрационные данные |
| [Migrations](./LibraryManagement.Api/Migrations/) | Миграции схемы PostgreSQL |
| [Mapping](./LibraryManagement.Api/Mapping/) | Преобразования Mapperly |
| [Validators](./LibraryManagement.Api/Validators/) | Правила FluentValidation |
| [Filters](./LibraryManagement.Api/Filters/) | Валидация Minimal API и логирование действий авторов |
| [Middleware](./LibraryManagement.Api/Middleware/) | Обработка исключений и заголовок времени ответа `X-Response-Time-Ms` |
| [RequestParameters](./LibraryManagement.Api/RequestParameters/) | Параметры пагинации и фильтрации книг |

## Сборка

```powershell
dotnet build
```

В текущем решении отдельного проекта автоматических тестов нет.
