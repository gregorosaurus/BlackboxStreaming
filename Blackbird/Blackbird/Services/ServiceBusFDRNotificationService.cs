using System;
using System.Collections.Generic;
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
            public string? RawTopicName { get; set; }
            public string? DecodedTopicName { get; set; }
        }

        private Options _options;
        public ServiceBusFDRNotificationService(Options option)
        {
            _options = option;

            if (_options.RawTopicName == null)
                throw new ArgumentNullException("RawTopicName");

            if (_options.DecodedTopicName == null)
                throw new ArgumentNullException("DecodedTopicName");
        }

        public async Task NotifyOfNewDecodedDataAsync(string acIdent, Dictionary<string, List<object>> values)
        {
            await SendServiceBusMessage(acIdent, new DecodedDataMessage()
            {
                AircraftIdentifier = acIdent,
                DecodedValues = values,
                ProcessedTime = DateTime.UtcNow
            }, _options.DecodedTopicName!);
        }


        public async Task NotifyOfNewSubframeDataAsync(string acIdent, byte[] subframeData)
        {
            await SendServiceBusMessage(acIdent, new RawDataMessage(acIdent, subframeData), _options.RawTopicName!);
        }

        private async Task SendServiceBusMessage(string acIdent, object msgObj, string topic)
        {

            await using var client = new ServiceBusClient(_options.ServiceBusConnectionString);

            // create the sender
            ServiceBusSender sender = client.CreateSender(topic);

            var json = JsonSerializer.Serialize(msgObj, new JsonSerializerOptions()
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

