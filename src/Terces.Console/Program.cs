using Azure.Identity;
using CommandLine;
using Terces;
using Terces.Azure;
using Terces.Console;
using Terces.Databases;

var credential = new DefaultAzureCredential();
const string defaultStore = "default";
const string azureCredential = "azure";
var client = new AzureClient(credential);

var context = new OperationContext
{
    Stores = new Dictionary<string, ISecretStore>()
    {
        {
            defaultStore, new KeyVaultSecretStore(new Uri("https://kv-secmgt-demo-cus-001.vault.azure.net/"),
                credential)
        }
    },
    Credentials = new Dictionary<string, object>()
    {
        {
            azureCredential, credential
        }
    },
    Rotators = new Dictionary<string, IRotator>()
    {
        { PostgreSqlFlexibleServerAdministratorRotator.StrategyType, new PostgreSqlFlexibleServerAdministratorRotator(client, TimeProvider.System) },
        { PostgreSqlUserRotator.StrategyType, new PostgreSqlUserRotator(TimeProvider.System) },
        { StorageAccountKeyRotator.StrategyType, new StorageAccountKeyRotator(client, TimeProvider.System) },
        { ManualSecretRotator.StrategyType, new ManualSecretRotator(TimeProvider.System) }
    },
    Force = false,
    IsWhatIf = false
};

List<ResourceConfiguration> resources = [
    new()
    {
        Name = "demo-pgsql",
        StrategyType = PostgreSqlFlexibleServerAdministratorRotator.StrategyType,
        StoreName = defaultStore,
        TargetResourceId = "/subscriptions/676db776-160e-411c-a9d8-6d799acf039f/resourceGroups/terces-demo-01/providers/Microsoft.DBforPostgreSQL/flexibleServers/pgsql-secmgt-demo-cus-001"
    },
    new()
    {
        Name = "demo-pgsql-user",
        StrategyType = PostgreSqlUserRotator.StrategyType,
        StoreName = defaultStore,
        TargetResourceId = "/subscriptions/676db776-160e-411c-a9d8-6d799acf039f/resourceGroups/terces-demo-01/providers/Microsoft.DBforPostgreSQL/flexibleServers/pgsql-secmgt-demo-cus-001",
        DatabaseUser = new()
        {
            ServerSecretName = "demo-pgsql",
            NamePrefix = "u",
            Hostname = "pgsql-secmgt-demo-cus-001.postgres.database.azure.com"
        }
    }
];

var init = new InitializeCommand(context, resources);

return await CommandLine.Parser.Default.ParseArguments<InitializeOptions>(args)
    .MapResult<InitializeOptions, Task<int>>(async (opts) => await init.Execute(opts),
        (errors) => Task.FromResult(1));