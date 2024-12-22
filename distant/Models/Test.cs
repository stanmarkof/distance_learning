namespace distant.Models
{
    public class Test
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; }
        public ICollection<Question> Questions { get; set; }
        public bool IsOpen { get; set; }  // Для открытия и закрытия теста
    }
}
