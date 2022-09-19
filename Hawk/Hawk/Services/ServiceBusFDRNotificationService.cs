using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Hawk.Models;

namespace Hawk.Services
{
    public class ServiceBusFDRNotificationService : IFDRNotificationService
    {
        public class Options
        {
            public string? ServiceBusConnectionString { get; set; }
            public string? TopicName { get; set; }
        }


        private Options _options;
        public ServiceBusFDRNotificationService(Options options)
        {
            _options = options;
        }

        public async Task SendNotificationOfDecodedDataAsync(string acIdent, Dictionary<string, List<object>> decodedData)
        {
            await using var client = new ServiceBusClient(_options.ServiceBusConnectionString);

            // create the sender
            ServiceBusSender sender = client.CreateSender(_options.TopicName);

            DecodedDataMessage message = new DecodedDataMessage()
            {
                AircraftIdentifier = acIdent,
                ProcessedTime = DateTime.UtcNow,
                DecodedValues = decodedData
            };
            string json = JsonSerializer.Serialize(message, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            ServiceBusMessage sbMessage = new ServiceBusMessage(new BinaryData(json))
            {
                ContentType = "application/json",
                TimeToLive = TimeSpan.FromMinutes(5)
            };
            sbMessage.ApplicationProperties.Add("aircraft", acIdent);

            // send the message
            await sender.SendMessageAsync(sbMessage);
        }
    }
}

