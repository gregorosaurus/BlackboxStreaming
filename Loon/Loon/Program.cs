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
        ServiceBusConnectionString = builder.Configuration.GetValue<string>("ServiceBusConnectionString"),
        ServiceBusTopic = builder.Configuration.GetValue<string>("ServiceBusTopic"),
        ServiceBusSubscription = builder.Configuration.GetValue<string>("ServiceBusSubscription"),
    };
});

var app = builder.Build();

app.Services.GetService<DataSubscriptionService>()!.Connect();

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

