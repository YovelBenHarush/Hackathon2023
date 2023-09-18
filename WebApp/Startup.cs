using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Hackathon2023;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
        app.Run(async (context) =>
        {
            using StreamReader reader = new StreamReader(context.Request.Body);
            string body = await reader.ReadToEndAsync();

            Console.WriteLine($"Got request with body: {body}");

            var askDBRunner = new AskDBRunner("MSSQLSERVER", "master");
            var response = await askDBRunner.AskDB(body);
            await context.Response.WriteAsync(response);
        });
    }
}
