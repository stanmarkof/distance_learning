namespace distant.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Options { get; set; } // JSON-encoded options
        public string CorrectAnswer { get; set; }
        public int TestId { get; set; }
        public Test? Test { get; set; }
    }
}
