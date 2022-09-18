using System;
using Azure.Storage.Files.DataLake;
using Hawk.Decode;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Hawk.Startup))]
namespace Hawk
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddSingleton<Services.AzureBlobConfigurationService.Options>(e =>
            {
                return new Services.AzureBlobConfigurationService.Options()
                {
                    ConfigurationFileSystem = "configs",
                    DataLakeServiceClient = new DataLakeServiceClient(Environment.GetEnvironmentVariable("AzureDataLakeConnectionString"))
                };
            });

            builder.Services.AddScoped<Services.IFDRConfigurationService, Services.AzureBlobConfigurationService>();

            builder.Services.AddSingleton<Services.EventHubFDRNotificationService.Options>(e =>
            {
                return new Services.EventHubFDRNotificationService.Options()
                {
                    EventHubConnectionString = Environment.GetEnvironmentVariable("EventHubConnectionString"),
                    EventHubName = "evh-blackbox-decoded"
                };
            });
            builder.Services.AddScoped<Services.IFDRNotificationService, Services.EventHubFDRNotificationService>();
        }
    }
}

