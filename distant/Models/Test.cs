namespace distant.Models
{
    public class Test
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int LessonId { get; set; }
        public Lesson? Lesson { get; set; }

        // Сделать поле nullable
        public ICollection<Question>? Questions { get; set; }


    }
}
