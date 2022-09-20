using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Blackbird.Models;

namespace Blackbird.Services
{
    public class EventHubFDRNotificationService : IFDRDataNotificationService
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

        public async Task NotifyOfNewSubframeDataAsync(string acIdent, byte[] subframeData)
        {
            RawDataMessage msg = new RawDataMessage(acIdent, subframeData);
            EventData eventData = new EventData(JsonSerializer.Serialize(msg, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
            await _producer.SendAsync(new EventData[] { eventData });
        }

        public Task NotifyOfNewDecodedDataAsync(string acIdent, Dictionary<string, List<object>> values)
        {
            throw new NotImplementedException();
        }
    }
}

