using Azure.AI.OpenAI;
using Grpc.Net.Client;
using Microsoft.DataSec.DspmForDatabases.Tools.HackathonScanEngineRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hackathon2023
{
    public class AskDBRunner
    {
        private const string HostName = "localhost";
        private const string Port = "49157";

        private readonly string _instanceName;
        private readonly string _dbName;

        private readonly VAGrpc.VAGrpcClient _grpcClient;
        private readonly AIClient _aiClient;

        public AskDBRunner(string instanceName, string dbName)
        {
            var channel = GrpcChannel.ForAddress($"http://{HostName}:{Port}");
            _grpcClient = new VAGrpc.VAGrpcClient(channel);
            _aiClient = new AIClient(GetDefaultChatInstructions());
            _instanceName = instanceName;
            _dbName = dbName;
        }

        public async Task RunAsync()
        {
            bool isSuccess = false;
            string? userRequest = null;

            Console.WriteLine("Starting...");
            do
            {
                do
                {
                    Console.WriteLine("What would you like to know about your database?");

                    userRequest = Console.ReadLine();
                }
                while (userRequest == null);

                var tableNames = await ExtractTableNames(userRequest);

                var schemas = await GetSchemasForTables(tableNames);

                var query = await BuildQuery(schemas, userRequest);

                if (!ValidateQuery(query))
                {
                    Console.WriteLine("Invalid query");
                }
                else
                {
                    var result = await RunQuery(query);

                    Console.WriteLine("Query result:");
                    Console.WriteLine(result);
                }
            }
            while (!isSuccess);
        }

        private async Task<List<string>> ExtractTableNames(string userRequest)
        {
            return new List<string>();
        }

        private async Task<List<string>> GetSchemasForTables(List<string> tableNames)
        {
            List<string> schemas = new List<string>();

            foreach (var tableName in tableNames)
            {
                var reply = await _grpcClient.ExecuteQueryAsync(
                                   new QueryRequest { InstanceName = _instanceName, DbName = _dbName, Query = "SELECT * FROM sys.tables" });

                schemas.Add(reply.QueryResults);
            }

            return schemas;
        }

        private async Task<string> BuildQuery(List<string> schemas, string userRequest)
        {
            return string.Empty;
        }

        private bool ValidateQuery(string query)
        {
            return false;
        }

        private async Task<string> RunQuery(string query)
        {
            var reply = await _grpcClient.ExecuteQueryAsync(
                                   new QueryRequest { InstanceName = _instanceName, DbName = _dbName, Query = "SELECT * FROM sys.tables" });

            return reply.QueryResults;
        }

        private IList<ChatMessage> GetDefaultChatInstructions()
        {
            var combinedInstructions = $@"You are an expert in SQL";

            Console.WriteLine(combinedInstructions);

            return new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, combinedInstructions),
            };
        }
    }
}
