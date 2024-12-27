using distant.Models;

namespace distant.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Убираем коллекцию студентов, так как связь теперь будет только через Student
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>(); // По умолчанию пустой список

    }
}
