using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing EF Core connection...");

        try
        {
            using var context = new TestDbContext();
            context.Database.EnsureCreated();

            Console.WriteLine("✅ Connected successfully. Database and table created.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Connection failed:");
            Console.WriteLine(ex.Message);
        }
    }
}
