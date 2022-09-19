using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Blackbird.Models;

namespace Blackbird.Services
{
    public class ServiceBusFDRNotificationService : IFDRDataNotificationService
    {
       

        public class Options
        {
            public string? ServiceBusConnectionString { get; set; }
            public string? TopicName { get; set; }
        }

        private Options _options;
        public ServiceBusFDRNotificationService(Options option)
        {
            _options = option;
        }

        public async Task NotifyOfNewSubframeDataAsync(string acIdent, byte[] subframeData)
        {
            await using var client = new ServiceBusClient(_options.ServiceBusConnectionString);

            // create the sender
            ServiceBusSender sender = client.CreateSender(_options.TopicName);

            RawDataMessage msg = new RawDataMessage(acIdent, subframeData);
            var json = JsonSerializer.Serialize(msg, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            ServiceBusMessage message = new ServiceBusMessage(new BinaryData(json))
            {
                ContentType = "application/json",
                TimeToLive = TimeSpan.FromMinutes(5)
            };
            message.ApplicationProperties.Add("aircraft", acIdent);

            // send the message
            await sender.SendMessageAsync(message);

        }
    }
}

