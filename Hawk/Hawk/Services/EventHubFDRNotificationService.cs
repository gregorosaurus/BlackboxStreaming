using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Hawk.Models;

namespace Hawk.Services
{
    public class EventHubFDRNotificationService : IFDRNotificationService
    {
        public class Options
        {
            public string? EventHubConnectionString { get; set; }
            public string? EventHubName { get; set; }
        }

        private EventHubProducerClient _producer;

        public EventHubFDRNotificationService(Options option)
        {
            if (option.EventHubConnectionString == null)
                throw new ArgumentNullException("EventHubConnectionString");
            if (option.EventHubName == null)
                throw new ArgumentNullException("EventHubName");

            _producer = new EventHubProducerClient(option.EventHubConnectionString, option.EventHubName);
        }

        public async Task SendNotificationOfDecodedDataAsync(string acIdent, Dictionary<string, List<object>> decodedData)
        {
            DecodedDataMessage message = new DecodedDataMessage()
            {
                AircraftIdentifier = acIdent,
                ProcessedTime = DateTime.UtcNow,
                DecodedValues = decodedData
            };
            EventData eventData = new EventData(JsonSerializer.Serialize(message, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
            await _producer.SendAsync(new EventData[] { eventData });
        }
    }
}

