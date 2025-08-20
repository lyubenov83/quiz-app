using Microsoft.EntityFrameworkCore;
using QuizServer.Data;
using QuizServer.Services; // Seeder
using QuizServer.Security; // ✅ Middleware

var builder = WebApplication.CreateBuilder(args);

// 🔧 MySQL connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Server=localhost;Port=3306;Database=quizdb;Uid=root;Pwd=Summerof2025;";

builder.Services.AddDbContext<QuizDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// ✅ JSON setup
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
    seeder.SeedQuestions("SeedData/questions.json"); // Adjust path if needed
}

// ✅ Static front-end (index.html, script.js, style.css under wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ API-key middleware (protects only /api/questions/normal & /api/questions/survival)
app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
