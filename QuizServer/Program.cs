using Microsoft.EntityFrameworkCore;
using QuizServer.Data;
using QuizServer.Services; // ✅ Include the seeder namespace

var builder = WebApplication.CreateBuilder(args);

// 🔧 Connection string to MySQL
var connectionString = "Server=localhost;Port=3306;Database=quizdb;Uid=root;Pwd=Summerof2025;";

builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ✅ Enable JSON enum and List<string> support
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ✅ Seed JSON questions at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seeder = new QuestionSeeder(services.GetRequiredService<QuizDbContext>());
    seeder.SeedQuestions("SeedData/questions.json"); // ✅ Adjust path if needed
}

// ✅ Serve static frontend files from wwwroot
app.UseDefaultFiles(); // Will load index.html by default
app.UseStaticFiles();  // Enable serving JS, CSS, etc.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
