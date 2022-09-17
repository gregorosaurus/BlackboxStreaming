﻿using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Azure.Storage.Files.DataLake;

[assembly: FunctionsStartup(typeof(Blackbird.Startup))]
namespace Blackbird
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddSingleton<Services.AzureQARDataService.Options>(e =>
            {
                return new Services.AzureQARDataService.Options()
                {
                    DataLakeClient = new DataLakeServiceClient(Environment.GetEnvironmentVariable("AzureFDRDataStorage"))
                };
            });
            builder.Services.AddScoped<Services.IQARDataManagementService, Services.AzureQARDataService>();

            builder.Services.AddSingleton<Services.EventHubFDRNotificationService.Options>(e =>
            {
                return new Services.EventHubFDRNotificationService.Options()
                {
                    EventHubConnectionString = Environment.GetEnvironmentVariable("AzureFDREventHubConnectionString"),
                    EventHubName = Environment.GetEnvironmentVariable("AzureFDRRawEventHubName")
                };
            });
            builder.Services.AddScoped<Services.IFDRDataNotificationService, Services.EventHubFDRNotificationService>();
        }
    }
}

