using Loon.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<DataSubscriptionService>();
builder.Services.AddSingleton<DataSubscriptionService.Options>(e =>
{
    return new DataSubscriptionService.Options()
    {
        EventHubNamespaceConnectionString = builder.Configuration.GetValue<string>("EventHubConnectionString"),
        EventHubName = builder.Configuration.GetValue<string>("EventHubName"),
        EventHubConsumerGroup = builder.Configuration.GetValue<string>("EventHubConsumerGroup"),
        BlobStorageConnection = builder.Configuration.GetValue<string>("BlobStorageContainerName"),
        BlobStorageConnection = builder.Configuration.GetValue<string>("BlobStorageConnectionString"),
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

