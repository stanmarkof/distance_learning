namespace distant.Models
{
    public class Material
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string FilePath { get; set; }
        public Lesson Lesson { get; set; }
    }
}