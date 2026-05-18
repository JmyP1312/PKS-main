# Контрольная работа 1 (2 семестр)

## Приложение для управления библиотекой

## Описание

Desktop-приложение для ведения библиотечного каталога: книги, авторы, жанры.

## Возможности

- CRUD-операции для книг.
- CRUD-операции для авторов.
- CRUD-операции для жанров.
- Фильтрация и поиск записей.
- Работа с базой данных SQLite через Entity Framework Core.

## Технологии

- C# / .NET
- Avalonia (UI)
- Entity Framework Core
- SQLite

## Структура

- `Models/` — сущности предметной области.
- `Data/` — контекст БД.
- `Services/` — прикладная логика.
- `ViewModels/` — слой VM.
- `Views/` — окна и представления.
- `LibraryManager.csproj` — файл проекта.

## Запуск

Из папки `sem2/kr/kr1`:

```bash
dotnet restore LibraryManager.csproj
dotnet run --project LibraryManager.csproj
```
