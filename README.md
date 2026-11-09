# SchoolDatabaseDemo

Решение на практическите задачи от **Урок 11** (SQLite база и таблица) и **Урок 12** (CRUD приложение), включително допълнителните задачи.

Технологии: WPF · C# · .NET 10 · Microsoft.Data.Sqlite

## Как се стартира
1. Отворете `SchoolDatabaseDemo.csproj` във Visual Studio 2026.
2. NuGet възстановява `Microsoft.Data.Sqlite` автоматично.
3. F5. При старт се създава `school.db` в папката на приложението (`bin\Debug\net10.0-windows`).

## Файлове
- `DatabaseService.cs` – целият SQL код (CREATE TABLE, INSERT, SELECT, UPDATE, DELETE, проверка на базата)
- `MainWindow.xaml(.cs)` – интерфейс и обработчици
- `Student.cs` – модел

## Покрити задачи
Урок 11: проект, пакет, интерфейс, connection string, CREATE TABLE, try/catch, проверка (повторно натискане без срив).
Доп.: таблица Subjects ✔, колона Email ✔ (с ALTER TABLE за стара база), бутон „Провери база“ ✔, DatabaseService ✔, Git ✔.

Урок 12: полета + DataGrid, OpenConnection, INSERT/SELECT/UPDATE/DELETE с параметри, избор на ред, потвърждение за изтриване, проверка на засегнатите редове, try/catch, валидация (празно име, оценка 2–6).
Доп.: „Изчисти полетата“ ✔, търсене по име ✔, брой записи ✔, забрана за изтриване без избран ред ✔, DatabaseService ✔.
