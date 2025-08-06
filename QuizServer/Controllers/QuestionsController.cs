using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizServer.Data;
using QuizServer.Models;

namespace QuizServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly QuizDbContext _context;

        public QuestionsController(QuizDbContext context)
        {
            _context = context;
        }

        // ✅ Get questions by category (handles spaces & case)
        [HttpGet("byCategory/{category}")]
        public async Task<ActionResult<IEnumerable<Question>>> GetByCategory(string category)
        {
            var decodedCategory = Uri.UnescapeDataString(category); // Decode 'world%20history' to 'world history'

            var questions = await _context.Questions
                .Where(q => q.Category.ToLower() == decodedCategory.ToLower())
                .ToListAsync();

            if (questions == null || questions.Count == 0)
                return NotFound();

            return Ok(questions);
        }
    }
}
