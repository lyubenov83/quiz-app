using Microsoft.AspNetCore.Mvc;
using QuizServer.Data;
using QuizServer.Models;
using Microsoft.EntityFrameworkCore;

namespace QuizServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly QuizDbContext _context;

        public CategoriesController(QuizDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            var categories = await _context.Categories
                .Select(c => c.Name)
                .ToListAsync();

            return Ok(categories);
        }
    }
}
