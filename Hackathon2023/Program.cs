using Grpc.Net.Client;
using Hackathon2023;

var askDBRunner = new AskDBRunner("MSSQLSERVER", "master");
await askDBRunner.RunAsync();