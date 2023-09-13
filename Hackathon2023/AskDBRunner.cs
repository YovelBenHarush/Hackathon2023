using Azure.AI.OpenAI;
using Grpc.Net.Client;
using Microsoft.DataSec.DspmForDatabases.Tools.HackathonScanEngineRunner;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Newtonsoft.Json;
using System.CodeDom;
using System.Text;

namespace Hackathon2023
{
    public class AskDBRunner
    {
        private const string HostName = "40.113.170.252";
        private const string Port = "54859";

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

            var schemas = await GetSchemas();

            SchemasStr = StringifySchemas(schemas);

            Console.WriteLine(SchemasStr);

            do
            {
                do
                {
                    Console.WriteLine("What would you like to know about your database?");

                    userRequest = Console.ReadLine();
                }
                while (userRequest == null);

                var query = await BuildQuery(schemas, userRequest);

                if (IsQueryValid(query))
                {
                    var result = await RunQuery(query);

                    Console.WriteLine("Query result:");
                    Console.WriteLine(result);
                }
                else
                {
                    Console.WriteLine("Invalid query");
                }
            }
            while (!isSuccess);
        }

        private async Task<List<string>> ExtractTableNames(string userRequest)
        {
            var request = @$"Please return a json valid list of SQL table names mentioned in the following user request (return only the list and no further words): {userRequest}";

            var completions = await _aiClient.GetChatCompletionsAsync(new List<(ChatRole Role, string Message)> { (ChatRole.User, request) }, default);

            var tableNamesStr = completions.Choices.FirstOrDefault()?.Message.Content;

            if (tableNamesStr != null)
            {
                Console.WriteLine($"Extracted table names: {tableNamesStr}");

                var tableNames = JsonConvert.DeserializeObject<List<string>>(tableNamesStr);

                if (tableNames != null)
                {
                    var parsedTableNames = new List<string>();
                    foreach (var tableName in tableNames)
                    {
                        var parsedTableName = tableName;
                        if (tableName.Contains('.'))
                        {
                            parsedTableName = tableName.Split('.')[1];
                        }

                        parsedTableNames.Add(parsedTableName);
                    }

                    foreach (var tableName in parsedTableNames)
                    {
                        Console.WriteLine(tableName);
                    }

                    return parsedTableNames;
                }
            }

            return new List<string>();
        }

        private async Task<Dictionary<string, string>> GetSchemasForTables(List<string> tableNames)
        {
            var schemas = new Dictionary<string, string>();

            foreach (var tableName in tableNames)
            {
                var schemaQuery = @$"SELECT COLUMN_NAME AS columnName, DATA_TYPE AS columnType
                    FROM INFORMATION_SCHEMA.COLUMNS WITH(NOLOCK)
                    WHERE TABLE_NAME LIKE '%{tableName}%'";

                Console.WriteLine(schemaQuery);

                var reply = await _grpcClient.ExecuteQueryAsync(
                                   new QueryRequest { InstanceName = _instanceName, DbName = _dbName, Query = schemaQuery });

                Console.WriteLine($"Schema for table {tableName}: {reply}");

                schemas.Add(tableName, reply.QueryResults);
            }

            return schemas;
        }

        private async Task<Dictionary<string, List<string>>> GetSchemas()
        {
            var schemas = new Dictionary<string, List<string>>();

            var schemaQuery = @$"SELECT TABLE_NAME As tableName, COLUMN_NAME AS columnName, DATA_TYPE AS columnType
                FROM INFORMATION_SCHEMA.COLUMNS WITH(NOLOCK);";

            Console.WriteLine(schemaQuery);

            var reply = await _grpcClient.ExecuteQueryAsync(
                                new QueryRequest { InstanceName = _instanceName, DbName = _dbName, Query = schemaQuery });

            Console.WriteLine($"Schema for database: {reply.QueryResults}");

            schemas = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(reply.QueryResults) ?? schemas;

            foreach (var schema in schemas)
            {
                Console.WriteLine($"Schema for table {schema.Key}: {string.Join(',',schema.Value)}");
            }

            return schemas;
        }

        private async Task<string> BuildQuery(Dictionary<string, List<string>> schemas, string userRequest)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Given the following SQL tables and their schemas:");
            sb.AppendLine(SchemasStr);

            sb.AppendLine($"Convert the following free-text user request to a SYNTHACTICALY VALID SQL query and return it (return only the query and no further words):");
            sb.AppendLine(userRequest);

            var finalReq = sb.ToString();
            Console.WriteLine(finalReq);

            var completions = await _aiClient.GetChatCompletionsAsync(new List<(ChatRole Role, string Message)> { (ChatRole.User, finalReq) }, default);

            var query = completions.Choices.FirstOrDefault()?.Message.Content;

            if (query == null)
            {
                Console.WriteLine("Failed to generate query");
                return string.Empty;
            }
            else
            {
                Console.WriteLine("Generated query:");
                Console.WriteLine(query);

                return query;
            }
        }

        private bool IsQueryValid(string query)
        {
            var result = Parser.Parse(query);

            if (result.Errors.Any())
            {
                Console.WriteLine($"Errors: {result.Errors.First()}");
            }

            return !result.Errors.Any();
        }

        private async Task<string> RunQuery(string query)
        {
            var reply = await _grpcClient.ExecuteQueryAsync(
                                   new QueryRequest { InstanceName = _instanceName, DbName = _dbName, Query = query });

            return reply.QueryResults;
        }

        private string StringifySchemas(Dictionary<string, List<string>> schemas)
        {
            var sb = new StringBuilder();
            var schemasDict = new Dictionary<string, List<(string ColumnName, string ColumnType)>>();

            var tables = schemas["tableName"];
            var columns = schemas["columnName"];
            var types = schemas["columnType"];

            for (int i = 0; i < tables.Count(); i++)
            {
                if (!schemasDict.ContainsKey(tables[i]))
                {
                    schemasDict.Add(tables[i], new List<(string ColumnName, string ColumnType)>());
                }

                schemasDict[tables[i]].Add((columns[i], types[i]));
            }

            foreach (var schema in schemasDict)
            {
                sb.AppendLine($"tableName: {schema.Key}");
                sb.AppendLine($"tableSchema: ");
                foreach (var column in schema.Value)
                {
                    sb.AppendLine($"columnName: {column.ColumnName} columnType: {column.ColumnType}");
                }
            }

            return sb.ToString();
        }

        private IList<ChatMessage> GetDefaultChatInstructions()
        {
            var combinedInstructions = $@"You are an expert in T-SQL and more specifically:
                                         1. you are an expert in converting user free-text requests to the equivilent (SYNTHACTICALY VALID) SQL query";
            var clarifications = @"IMPORTANT: Do not make up table names on your own! Only take table names from the user request!";

            Console.WriteLine(combinedInstructions);

            return new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, combinedInstructions),
                new ChatMessage(ChatRole.System, clarifications)
            };
        }

        public string SchemasStr { get; set; } = string.Empty;
    }
}
