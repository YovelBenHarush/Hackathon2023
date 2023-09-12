using Grpc.Net.Client;
using Hackathon2023;

Console.WriteLine("Starting...");

// The port number must match the port of the gRPC server.
using var channel = GrpcChannel.ForAddress("https://localhost:49157");
var client = new VAGrpc.VAGrpcClient(channel);
var reply = await client.ExecuteQueryAsync(
                  new QueryRequest { InstanceName = "SQLEXPRESS", DbName = "master", Query = "SELECT * FROM sys.tables" });
Console.WriteLine($"returnCode: {reply.ReturnCode}, results: {reply.QueryResults}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();