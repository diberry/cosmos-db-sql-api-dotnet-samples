﻿// <using> Using blocks
using Microsoft.Azure.Cosmos;
// </using>

// <credentials> Account endpoint and key credentials
string endpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT")!;
string key = Environment.GetEnvironmentVariable("COSMOS_KEY")!;
// </credentials>

// <client> New instance of CosmosClient class
using CosmosClient client = new(endpoint, key);
// </client>

// <new-database> Database reference with creation if it does not already exist
Database database = await client.CreateDatabaseIfNotExistsAsync(
    id: "tododatabase"
);

Console.WriteLine($"New database:\t{database.Id}");
// </new-database>

// <new-container> Container reference with creation if it does not alredy exist
Container container = await database.CreateContainerIfNotExistsAsync(
    id: "taskscontainer",
    partitionKeyPath: "/partitionKey",
    throughput: 400
);

Console.WriteLine($"New container:\t{container.Id}");
// </new-container>

// <new-item> Create new object and upsert (create or replace) to container
TodoItem newItem = new(
    id: "fb59918b-fb3d-4549-9503-38bee83a6e1d",
    partitionKey: "personal-tasks-user-88033a55",
    description: "Dispose of household trash",
    done: false,
    priority: 2
);

TodoItem createdItem = await container.UpsertItemAsync<TodoItem>(
    item: newItem,
    partitionKey: new PartitionKey("personal-tasks-user-88033a55")
);

Console.WriteLine($"Created item:\t{createdItem.id}\t[{createdItem.partitionKey}]");
// </new-item>

// <query> Create query using a SQL string and parameters
var query = new QueryDefinition(
    query: "SELECT * FROM todo t WHERE t.partitionKey = @key"
)
    .WithParameter("@key", "personal-tasks-user-88033a55");

using FeedIterator<TodoItem> feed = container.GetItemQueryIterator<TodoItem>(
    queryDefinition: query
);

while (feed.HasMoreResults)
{
    FeedResponse<TodoItem> response = await feed.ReadNextAsync();
    foreach (TodoItem item in response)
    {
        Console.WriteLine($"Found item:\t{createdItem.description}");
    }
}
// </query>