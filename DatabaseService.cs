using Microsoft.Data.Sqlite;

namespace SchoolDatabaseDemo;

// Целият SQL код е тук (допълнителна задача: DatabaseService).
// MainWindow само чете полетата, вика методите и показва резултата.
public class DatabaseService
{
    // school.db се създава в папката на приложението при първото изпълнение.
    private static readonly string DbPath =
        Path.Combine(AppContext.BaseDirectory, "school.db");

    private readonly string _connectionString = $"Data Source={DbPath}";

    public string DatabaseFile => DbPath;

    // Метод, който връща отворена връзка (урок 12, стъпка "Метод за отваряне на връзка").
    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    // ---------- Урок 11: създаване на базата и таблиците ----------

    public void CreateDatabase()
    {
        using var connection = OpenConnection();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    ClassName TEXT NOT NULL,
                    AverageGrade REAL,
                    Email TEXT
                );";
            command.ExecuteNonQuery();
        }

        // Допълнителна задача 1: таблица Subjects.
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Subjects (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    HoursPerWeek INTEGER
                );";
            command.ExecuteNonQuery();
        }

        // Допълнителна задача 2: колона Email. Ако school.db е създадена от
        // по-стара версия без Email, добавяме колоната с ALTER TABLE.
        if (!ColumnExists(connection, "Students", "Email"))
        {
            using var command = connection.CreateCommand();
            command.CommandText = "ALTER TABLE Students ADD COLUMN Email TEXT";
            command.ExecuteNonQuery();
        }
    }

    private static bool ColumnExists(SqliteConnection connection, string table, string column)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table})";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    // Допълнителна задача 3: "Провери база" – връща текст със състоянието.
    public string CheckDatabase()
    {
        if (!File.Exists(DbPath))
            return "Файлът school.db не съществува. Натиснете „Създай база“.";

        using var connection = OpenConnection();

        var tables = new List<string>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText =
                "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name";
            using var reader = command.ExecuteReader();
            while (reader.Read())
                tables.Add(reader.GetString(0));
        }

        if (tables.Count == 0)
            return "Файлът school.db съществува, но няма таблици.";

        var parts = new List<string>();
        foreach (var table in tables)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {table}";
            long count = (long)(command.ExecuteScalar() ?? 0L);
            parts.Add($"{table}: {count} записа");
        }
        return "Базата е наред. " + string.Join("; ", parts);
    }

    // ---------- Урок 12: CRUD ----------

    // Create → INSERT
    public int AddStudent(string name, string className, double? grade, string? email)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Students (Name, ClassName, AverageGrade, Email)
            VALUES ($name, $class, $grade, $email)";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$class", className);
        command.Parameters.AddWithValue("$grade", (object?)grade ?? DBNull.Value);
        command.Parameters.AddWithValue("$email", (object?)email ?? DBNull.Value);
        return command.ExecuteNonQuery();
    }

    // Read → SELECT (с незадължително търсене по име – допълнителна задача 2 от урок 12)
    public List<Student> GetStudents(string? search = null)
    {
        var students = new List<Student>();

        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT Id, Name, ClassName, AverageGrade, Email
            FROM Students
            WHERE $search = '' OR Name LIKE '%' || $search || '%'
            ORDER BY Id";
        command.Parameters.AddWithValue("$search", search?.Trim() ?? "");

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            students.Add(new Student
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                ClassName = reader.GetString(2),
                AverageGrade = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                Email = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }
        return students;
    }

    // Брой на всички записи (допълнителна задача 3 от урок 12)
    public long CountStudents()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Students";
        return (long)(command.ExecuteScalar() ?? 0L);
    }

    // Update → UPDATE (WHERE Id = $id е задължително)
    public int UpdateStudent(int id, string name, string className, double? grade, string? email)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Students SET
                Name = $name,
                ClassName = $class,
                AverageGrade = $grade,
                Email = $email
            WHERE Id = $id";
        command.Parameters.AddWithValue("$name", name);
        command.Parameters.AddWithValue("$class", className);
        command.Parameters.AddWithValue("$grade", (object?)grade ?? DBNull.Value);
        command.Parameters.AddWithValue("$email", (object?)email ?? DBNull.Value);
        command.Parameters.AddWithValue("$id", id);
        return command.ExecuteNonQuery();
    }

    // Delete → DELETE (WHERE Id = $id е задължително)
    public int DeleteStudent(int id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Students WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        return command.ExecuteNonQuery();
    }
}
