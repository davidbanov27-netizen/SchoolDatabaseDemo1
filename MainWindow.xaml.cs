using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SchoolDatabaseDemo;

public partial class MainWindow : Window
{
    private readonly DatabaseService _db = new();

    // Id на избрания в DataGrid ученик (null = няма избран ред).
    private int? _selectedId;

    public MainWindow()
    {
        InitializeComponent();

        // Базата и таблиците се създават автоматично при старт (IF NOT EXISTS),
        // така че LoadStudents винаги намира таблица.
        try
        {
            _db.CreateDatabase();
            LoadStudents();
            ShowStatus("Базата е готова.");
        }
        catch (Exception ex)
        {
            ShowStatus("Грешка: " + ex.Message);
        }
    }

    // ---------- Помощни методи ----------

    private void ShowStatus(string message) => StatusTextBlock.Text = message;

    private void LoadStudents()
    {
        var students = _db.GetStudents(SearchBox.Text);
        StudentsGrid.ItemsSource = students;
        CountTextBlock.Text = $"Записи: {students.Count} (общо {_db.CountStudents()})";
    }

    private void ClearFields()
    {
        NameBox.Clear();
        ClassBox.Clear();
        GradeBox.Clear();
        EmailBox.Clear();
        _selectedId = null;
        StudentsGrid.SelectedItem = null;
        UpdateButton.IsEnabled = false;
        DeleteButton.IsEnabled = false;
    }

    // Чете и валидира полетата. Връща false и показва съобщение при проблем.
    private bool TryReadInput(out string name, out string className, out double? grade, out string? email)
    {
        name = NameBox.Text.Trim();
        className = ClassBox.Text.Trim();
        grade = null;
        email = string.IsNullOrWhiteSpace(EmailBox.Text) ? null : EmailBox.Text.Trim();

        if (name.Length == 0)
        {
            ShowStatus("Въведете име.");
            NameBox.Focus();
            return false;
        }
        if (className.Length == 0)
        {
            ShowStatus("Въведете клас.");
            ClassBox.Focus();
            return false;
        }

        string gradeText = GradeBox.Text.Trim().Replace(',', '.');
        if (gradeText.Length > 0)
        {
            if (!double.TryParse(gradeText, NumberStyles.Float, CultureInfo.InvariantCulture, out double g)
                || g < 2 || g > 6)
            {
                ShowStatus("Невалидна оценка. Въведете число между 2 и 6 (например 5.50).");
                GradeBox.Focus();
                return false;
            }
            grade = g;
        }
        return true;
    }

    // ---------- Урок 11 ----------

    private void CreateDatabaseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _db.CreateDatabase();
            LoadStudents();
            ShowStatus("Базата е готова.");
        }
        catch (Exception ex)
        {
            ShowStatus("Грешка: " + ex.Message);
        }
    }

    private void CheckDatabaseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ShowStatus(_db.CheckDatabase());
        }
        catch (Exception ex)
        {
            ShowStatus("Грешка: " + ex.Message);
        }
    }

    // ---------- Урок 12: CRUD ----------

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadInput(out var name, out var className, out var grade, out var email))
            return;

        try
        {
            int affected = _db.AddStudent(name, className, grade, email);
            LoadStudents();
            ClearFields();
            ShowStatus(affected > 0 ? "Ученикът е добавен." : "Няма добавен запис.");
        }
        catch (Exception ex)
        {
            ShowStatus("Грешка при добавяне: " + ex.Message);
        }
    }

    private void StudentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StudentsGrid.SelectedItem is Student student)
        {
            _selectedId = student.Id;
            NameBox.Text = student.Name;
            ClassBox.Text = student.ClassName;
            GradeBox.Text = student.AverageGrade?.ToString("0.00", CultureInfo.InvariantCulture) ?? "";
            EmailBox.Text = student.Email ?? "";
            UpdateButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;   // изтриване само с избран ред
        }
        else
        {
            _selectedId = null;
            UpdateButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
        }
    }

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedId is not int id)
        {
            ShowStatus("Изберете ученик от таблицата.");
            return;
        }
        if (!TryReadInput(out var name, out var className, out var grade, out var email))
            return;

        try
        {
            int affected = _db.UpdateStudent(id, name, className, grade, email);
            LoadStudents();
            ShowStatus(affected == 0 ? "Няма променен запис." : "Ученикът е обновен.");
        }
        catch (Exception ex)
        {
            ShowStatus("Грешка при обновяване: " + ex.Message);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedId is not int id)
        {
            ShowStatus("Изберете ученик, който да бъде изтрит.");
            return;
        }

        var answer = MessageBox.Show(
            $"Сигурни ли сте, че искате да изтриете „{NameBox.Text}“?",
            "Потвърждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (answer != MessageBoxResult.Yes)
            return;

        try
        {
            int affected = _db.DeleteStudent(id);
            ClearFields();
            LoadStudents();
            ShowStatus(affected == 0 ? "Няма изтрит запис." : "Ученикът е изтрит.");
        }
        catch (Exception ex)
        {
            ShowStatus("Грешка при изтриване: " + ex.Message);
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ClearFields();
        ShowStatus("Полетата са изчистени.");
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // TextChanged се вика и по време на InitializeComponent – тогава пропускаме.
        if (!IsLoaded) return;

        try
        {
            LoadStudents();
        }
        catch (Exception ex)
        {
            ShowStatus("Грешка при търсене: " + ex.Message);
        }
    }
}
