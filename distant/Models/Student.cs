using distant.Models;

public class Student : User
{
    public int GroupId { get; set; } // Внешний ключ группы
    public Group Group { get; set; } // Навигационное свойство
    public ICollection<TestResult> TestResults { get; set; }
}
