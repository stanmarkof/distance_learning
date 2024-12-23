namespace distant.Models
{
    public class Material
    {
        public int Id { get; set; }
        public int LessonId { get; set; } // Связь с уроком
        public string Title { get; set; } // Заголовок материала
        public string Content { get; set; } // Содержимое материала


        public Lesson? Lesson { get; set; } // Навигационное свойство
    }
}
