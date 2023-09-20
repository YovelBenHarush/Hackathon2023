using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Hackathon2023;

public class Startup
{
    public void Configure(IApplicationBuilder app)
    {
		app.UseDeveloperExceptionPage();

        app.UseCors("MyPolicy");

		app.Run(async (context) =>
        {
            using StreamReader reader = new StreamReader(context.Request.Body);
            string body = await reader.ReadToEndAsync();

            Console.WriteLine($"Got request with body: {body}");

            var askDBRunner = new AskDBRunner("MSSQLSERVER", "master");
			var query = await askDBRunner.BuildQuery(body);
			var queryResults = await askDBRunner.AskDB(query);

			var response = new { GeneratedQuery = query, QueryResults = "queryResults" };
            var serialized = JsonSerializer.Serialize(response).Replace("\"queryResults\"", queryResults);
			await context.Response.WriteAsync(serialized);
		});
    }
}
