namespace distant.Models
{
    public class TestResult
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }

        public int TestId { get; set; }
        public Test Test { get; set; }

        public int Score { get; set; }
    }
}
