namespace distant.Models
{
    public class Lecturer : User
    {
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>(); // Навигационное свойство
    }
}
