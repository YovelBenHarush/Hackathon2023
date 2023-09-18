using Hackathon2023;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var askDBRunner = new AskDBRunner("MSSQLSERVER", "master");
        await askDBRunner.RunAsync();
    }

}