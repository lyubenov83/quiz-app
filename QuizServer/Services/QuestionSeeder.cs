using System.Text.Json;
using QuizServer.Models;
using QuizServer.Data;

namespace QuizServer.Services
{
    public class QuestionSeeder
    {
        private readonly QuizDbContext _context;

        public QuestionSeeder(QuizDbContext context)
        {
            _context = context;
        }

        public void SeedQuestions(string filePath)
        {
            if (_context.Questions.Any()) return;

            var json = File.ReadAllText(filePath);
            var questions = JsonSerializer.Deserialize<List<Question>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (questions != null)
            {
                _context.Questions.AddRange(questions);
                _context.SaveChanges();
            }
        }
    }
}
