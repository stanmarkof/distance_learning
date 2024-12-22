

using System.ComponentModel.DataAnnotations;

namespace distant.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название урока обязательно")]
        public string Name { get; set; }
        public int? LecturerId { get; set; } // Поле становится необязательным
        public Lecturer? Lecturer { get; set; } // Ссылка может быть null

        public ICollection<Group> Groups { get; set; } = new List<Group>(); // Указываем значение по умолчанию
    }
}