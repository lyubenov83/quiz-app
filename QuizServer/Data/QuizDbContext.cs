using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QuizServer.Models;

namespace QuizServer.Data
{
    public class QuizDbContext : DbContext
    {
        public QuizDbContext(DbContextOptions<QuizDbContext> options)
            : base(options)
        {
        }

        public DbSet<Question> Questions { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Seed default categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Math" },
                new Category { Id = 2, Name = "Science" },
                new Category { Id = 3, Name = "History" },
                new Category { Id = 4, Name = "Literature" }
            );

            // ✅ Store List<string> as JSON for PossibleAnswers
            modelBuilder.Entity<Question>()
                .Property(q => q.PossibleAnswers)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>());

            // ✅ Store enum Rank as string
            modelBuilder.Entity<Question>()
                .Property(q => q.Rank)
                .HasConversion<string>();
        }
    }
}
