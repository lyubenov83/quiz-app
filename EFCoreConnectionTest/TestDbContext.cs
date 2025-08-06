using Microsoft.EntityFrameworkCore;

public class TestDbContext : DbContext
{
    public DbSet<Question> Questions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseMySql(
            "Server=localhost;Port=3306;Database=quizdb;User Id=root;Password=Summerof2025;",
            ServerVersion.AutoDetect("Server=localhost;Port=3306;Database=quizdb;User Id=root;Password=Summerof2025;")
        );
    }
}
