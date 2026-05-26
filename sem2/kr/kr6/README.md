# Практическая работа 6

## Система управления товарами интернет-магазина на Blazor WebAssembly

Реализовано клиент-серверное приложение:

- `BlazorClient` - Blazor WebAssembly интерфейс.
- `BlazorServer` - ASP.NET Core host с REST API.
- `Shared` - общая модель `Product`.
- SQLite база данных через Entity Framework Core.
- API `GET /api/products`, `POST /api/products`, `PUT /api/products`, `PUT /api/products/{id}`, `DELETE /api/products/{id}`.
- Отображение товаров, добавление, изменение и удаление с подтверждением.
- Переключение светлой и темной темы с сохранением выбора в `localStorage`.

## Запуск

Из папки `sem2/kr/kr6` запустить сервер:

```bash
dotnet restore BlazorServer/BlazorServer.csproj
dotnet run --project BlazorServer/BlazorServer.csproj
```

Во втором терминале запустить Blazor WebAssembly клиент:

```bash
dotnet restore BlazorClient/BlazorClient.csproj
dotnet run --project BlazorClient/BlazorClient.csproj
```

Открыть `http://127.0.0.1:5173`. Клиент обращается к API сервера по адресу `http://127.0.0.1:5157/api/products`.

Если порт сервера занят, запустить его с другим адресом и обновить `ApiBaseUrl` в `BlazorClient/wwwroot/appsettings.json`.

База `store.db` создается автоматически и заполняется тестовыми товарами.
