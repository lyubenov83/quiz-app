namespace QuizServer.Models
{
    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    public class Question
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public Difficulty Rank { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<string> PossibleAnswers { get; set; } = new();
        public int CorrectAnswer { get; set; }
    }
}
