namespace distant.Models
{
    public class Feedback
    {
        public int Id { get; set; } // Уникальный идентификатор
        public string UserName { get; set; } // Имя пользователя
        public string Email { get; set; } // Электронная почта пользователя
        public string Message { get; set; } // Сообщение обратной связи
        public DateTime DateSubmitted { get; set; } // Дата отправки отзыва
    }
}
