using Microsoft.Extensions.Configuration;
using QuizClient.Models;
using QuizClient.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Load configuration
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var baseUrl = config["Server:BaseUrl"] ?? "https://localhost:7175";
var apiKey = config["Server:ApiKey"] ?? "DEV_SUPER_SECRET_KEY_123";
var defaultCount = int.TryParse(config["Game:DefaultCount"], out var c) ? c : 10;

var api = new ApiClient(baseUrl, apiKey);

Console.WriteLine("=== QUIZ CLIENT (.NET) ===");
Console.WriteLine($"Server: {baseUrl}");
Console.WriteLine();

try
{
    // Show categories (public)
    var cats = await api.GetCategoriesAsync();
    if (cats.Count == 0)
    {
        Console.WriteLine("No categories returned by the server.");
        return;
    }

    Console.WriteLine("Available categories:");
    for (int i = 0; i < cats.Count; i++)
        Console.WriteLine($"  {i + 1}) {cats[i]}");

    Console.WriteLine();
    Console.Write("Choose mode [N]ormal / [S]urvival (default N): ");
    var mode = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();
    if (string.IsNullOrEmpty(mode)) mode = "N";

    List<Question> questions;

    if (mode == "S")
    {
        Console.Write($"Total questions (default {defaultCount}): ");
        var totalStr = Console.ReadLine();
        if (!int.TryParse(totalStr, out var total) || total <= 0) total = defaultCount;

        questions = await api.GetSurvivalAsync(total);
        Console.WriteLine($"Loaded {questions.Count} survival questions (sorted by difficulty).");
    }
    else
    {
        Console.Write("Enter categories by number (comma separated) or leave blank for ALL: ");
        var input = (Console.ReadLine() ?? "").Trim();

        IEnumerable<string> selectedCats;
        if (string.IsNullOrWhiteSpace(input))
        {
            selectedCats = cats; // all
        }
        else
        {
            var indexes = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var chosen = new List<string>();
            foreach (var idxStr in indexes)
            {
                if (int.TryParse(idxStr, out var idx) && idx >= 1 && idx <= cats.Count)
                    chosen.Add(cats[idx - 1]);
            }
            selectedCats = chosen.Count > 0 ? chosen : cats;
        }

        Console.Write($"How many questions (default {defaultCount}): ");
        var countStr = Console.ReadLine();
        if (!int.TryParse(countStr, out var count) || count <= 0) count = defaultCount;

        questions = await api.GetNormalAsync(selectedCats, count);
        Console.WriteLine($"Loaded {questions.Count} random questions.");
    }

    if (questions.Count == 0)
    {
        Console.WriteLine("No questions returned. Exiting.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("=== START QUIZ ===");
    int score = 0;

    int qNumber = 1;
    foreach (var q in questions)
    {
        Console.WriteLine();
        Console.WriteLine($"Q{qNumber}. [{q.Category}] [{q.Rank}] {q.Text}");
        for (int i = 0; i < q.PossibleAnswers.Count; i++)
            Console.WriteLine($"   {i + 1}) {q.PossibleAnswers[i]}");

        int choice = 0;
        while (choice < 1 || choice > q.PossibleAnswers.Count)
        {
            Console.Write("Your answer (1-4): ");
            var input = Console.ReadLine();
            int.TryParse(input, out choice);
        }

        bool correct = (choice - 1) == q.CorrectAnswer;
        if (correct)
        {
            score++;
            Console.WriteLine("✅ Correct!");
        }
        else
        {
            Console.WriteLine($"❌ Wrong! Correct: {q.PossibleAnswers[q.CorrectAnswer]}");
        }

        Console.WriteLine($"Score: {score}/{qNumber} ({(int)Math.Round(100.0 * score / qNumber)}%)");
        qNumber++;
    }

    Console.WriteLine();
    Console.WriteLine("=== QUIZ FINISHED ===");
    Console.WriteLine($"Final score: {score}/{questions.Count} ({(int)Math.Round(100.0 * score / questions.Count)}%)");
}
catch (HttpRequestException ex)
{
    Console.WriteLine("HTTP error: " + ex.Message);
}
catch (Exception ex)
{
    Console.WriteLine("Unexpected error: " + ex.Message);
}
