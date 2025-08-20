using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizServer.Data;
using QuizServer.Models;

namespace QuizServer.Controllers
{
    public class NormalRequest
    {
        public List<string>? Categories { get; set; }
        public int Count { get; set; } = 10;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class QuestionsController : ControllerBase
    {
        private readonly QuizDbContext _context;

        public QuestionsController(QuizDbContext context)
        {
            _context = context;
        }

        // ✅ 0) Public: Get questions by category (handles spaces & case)
        [HttpGet("byCategory/{category}")]
        public async Task<ActionResult<IEnumerable<Question>>> GetByCategory(string category)
        {
            var decodedCategory = Uri.UnescapeDataString(category); // "world%20history" -> "world history"

            var questions = await _context.Questions
                .Where(q => q.Category.ToLower() == decodedCategory.ToLower())
                .ToListAsync();

            if (questions == null || questions.Count == 0)
                return NotFound();

            return Ok(questions);
        }

        // ✅ 1A) Protected: Normal mode via GET with query (?categories=biology,geography&count=10)
        [HttpGet("normal")]
        public async Task<ActionResult<IEnumerable<Question>>> GetNormal(
            [FromQuery] string? categories = null,
            [FromQuery] int count = 10)
        {
            if (count <= 0) count = 10;

            List<string>? categoryList = null;
            if (!string.IsNullOrWhiteSpace(categories))
            {
                categoryList = categories
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(s => s.ToLower())
                    .ToList();
            }

            IQueryable<Question> query = _context.Questions;

            if (categoryList != null && categoryList.Count > 0)
            {
                query = query.Where(q => categoryList.Contains(q.Category.ToLower()));
            }

            var result = await query
                .OrderBy(q => EF.Functions.Random())
                .Take(count)
                .ToListAsync();

            if (result.Count == 0)
                return NotFound("No questions found for the requested criteria.");

            return Ok(result);
        }

        // ✅ 1B) Protected: Normal mode via POST with JSON body { categories: [], count: 10 }
        [HttpPost("normal")]
        public async Task<ActionResult<IEnumerable<Question>>> PostNormal([FromBody] NormalRequest req)
        {
            var count = req?.Count > 0 ? req!.Count : 10;

            IQueryable<Question> query = _context.Questions;

            if (req?.Categories != null && req.Categories.Count > 0)
            {
                var set = req.Categories.Select(c => c.ToLower()).ToHashSet();
                query = query.Where(q => set.Contains(q.Category.ToLower()));
            }

            var result = await query
                .OrderBy(q => EF.Functions.Random())
                .Take(count)
                .ToListAsync();

            if (result.Count == 0)
                return NotFound("No questions found for the requested criteria.");

            return Ok(result);
        }

        // ✅ 2) Protected: Survival mode (sorted by difficulty; optional total split ~1/3 each)
        [HttpGet("survival")]
        public async Task<ActionResult<IEnumerable<Question>>> GetSurvival([FromQuery] int? total = null)
        {
            if (total.HasValue && total.Value > 0)
            {
                var perBucket = Math.Max(1, total.Value / 3);

                var easy = await _context.Questions
                    .Where(q => q.Rank == Difficulty.Easy)
                    .OrderBy(q => EF.Functions.Random())
                    .Take(perBucket)
                    .ToListAsync();

                var medium = await _context.Questions
                    .Where(q => q.Rank == Difficulty.Medium)
                    .OrderBy(q => EF.Functions.Random())
                    .Take(perBucket)
                    .ToListAsync();

                var hard = await _context.Questions
                    .Where(q => q.Rank == Difficulty.Hard)
                    .OrderBy(q => EF.Functions.Random())
                    .Take(perBucket)
                    .ToListAsync();

                var combined = easy.Concat(medium).Concat(hard)
                                   .OrderBy(q => q.Rank)
                                   .ToList();

                if (combined.Count == 0)
                    return NotFound("No questions available for survival mode.");

                return Ok(combined);
            }

            var allSorted = await _context.Questions
                .OrderBy(q => q.Rank)
                .ToListAsync();

            if (allSorted.Count == 0)
                return NotFound("No questions available.");

            return Ok(allSorted);
        }
    }
}
