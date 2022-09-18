using System;
using Azure.Messaging.EventHubs;
using System.Diagnostics;
using Azure.Storage.Blobs;
using Loon.Models;
using Azure.Messaging.EventHubs.Processor;
using System.Text;
using System.Text.Json;

namespace Loon.Services
{
    public class DataReceivedEventArgs
    {
        public DecodedDataMessage? Message { get; set; }
    }

    public interface IDataSubscriptionSubscriber
    {
        void ProcessNewData(DecodedDataMessage message);
    }

    /// <summary>
    /// The data subscription service is responsible for connecting
    /// to the event hub and sending data to any and all subscribers
    /// </summary>
    public class DataSubscriptionService : IDisposable
    {
        public class Options
        {
            public string? EventHubNamespaceConnectionString { get; set; }
            public string? EventHubName { get; set; }
            public string? EventHubConsumerGroup { get; set; }
            public string? BlobStorageConnectionString { get; set; }
            public string BlobStorageContainerName { get; set; } = "loon-evh-control";
        }

        private List<IDataSubscriptionSubscriber> _subscribers = new List<IDataSubscriptionSubscriber>();

        private DateTime? _lastTimeCheckpointed;
        private EventProcessorClient _processor;

        public DataSubscriptionService(Options options)
        {
            var blobContainerClient = new BlobContainerClient(options.BlobStorageConnectionString, options.BlobStorageContainerName);
            _processor = new EventProcessorClient(blobContainerClient, options.EventHubConsumerGroup, options.EventHubNamespaceConnectionString, options.EventHubName);
            _processor.ProcessEventAsync += ProcessEventHandler;
            _processor.ProcessErrorAsync += ProcessErrorAsync;
        }

        private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            Console.WriteLine("Error:" + arg.Exception.Message);
            return Task.FromResult(0);
        }

        public void AddSubscriber(IDataSubscriptionSubscriber subscriber)
        {
            lock (_subscribers)
            {
                _subscribers.Add(subscriber);
            }
        }

        private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
        {
            // Write the body of the event to the console window
            Console.WriteLine("\tReceived event: {0}", Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));

            if(_lastTimeCheckpointed == null || DateTime.UtcNow - _lastTimeCheckpointed! > TimeSpan.FromSeconds(30))
            {
                await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
                _lastTimeCheckpointed = DateTime.UtcNow;
            }

            string json = eventArgs.Data.EventBody.ToString();
            DecodedDataMessage? message = JsonSerializer.Deserialize<DecodedDataMessage>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            });
            if (message == null)
                return;

            lock(_subscribers)
            {
                foreach(var sub in _subscribers)
                {
                    sub.ProcessNewData(message);
                }
            }
        }

        public void RemoveSubscriber(IDataSubscriptionSubscriber subscriber)
        {
            lock (_subscribers)
            {
                _subscribers.RemoveAll(x =>
                {
                    return x == subscriber;
                });
            }
        }

        public async Task ConnectToEventHubAsync()
        {
            await _processor.StartProcessingAsync();
        }

        public void Dispose()
        {
            _processor.ProcessEventAsync -= ProcessEventHandler;
            _processor.StopProcessing();
        }
    }
}

