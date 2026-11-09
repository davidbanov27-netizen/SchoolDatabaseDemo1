namespace SchoolDatabaseDemo;

// Модел на данните: един ред от таблица Students.
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ClassName { get; set; } = "";
    public double? AverageGrade { get; set; }
    public string? Email { get; set; }
}
