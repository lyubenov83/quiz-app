using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuizServer.Data
{
    public class QuizDbContextFactory : IDesignTimeDbContextFactory<QuizDbContext>
    {
        public QuizDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<QuizDbContext>();

            optionsBuilder.UseMySql(
                "Server=localhost;Port=3306;Database=quizdb;User Id=root;Password=Summerof2025;",
                ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=quizdb;User Id=root;Password=Summerof2025;")
            );

            return new QuizDbContext(optionsBuilder.Options);
        }
    }
}
