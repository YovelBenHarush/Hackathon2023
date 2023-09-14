using Grpc.Net.Client;
using Hackathon2023;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var askDBRunner = new AskDBRunner("MSSQLSERVER", "master");
        await askDBRunner.RunAsync();   
    }

}