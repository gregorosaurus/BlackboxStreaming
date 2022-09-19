using System;
using Loon.Models;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

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
            public string? ServiceBusConnectionString { get; set; }
            public string? ServiceBusTopic { get; set; }
            public string? ServiceBusSubscription { get; set; }
        }

        private List<IDataSubscriptionSubscriber> _subscribers = new List<IDataSubscriptionSubscriber>();

        private ServiceBusClient? _sbClient;

        private Options _options;
        private bool _isRunning = false;

        public DataSubscriptionService(Options options)
        {
            _options = options;
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

        private void ProcessMessage(ServiceBusReceivedMessage? sbMessage)
        {
            if (sbMessage == null)
                return;
            // Write the body of the event to the console window
            Console.WriteLine("\tReceived event: {0}",sbMessage.Body.ToString());

            string json = sbMessage.Body.ToString();
            DecodedDataMessage? decodedMessage = JsonSerializer.Deserialize<DecodedDataMessage>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            });
            if (decodedMessage == null)
                return;

            lock(_subscribers)
            {
                foreach(var sub in _subscribers)
                {
                    sub.ProcessNewData(decodedMessage);
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

        public Task Connect()
        {
            _isRunning = true;
            _sbClient = new ServiceBusClient(_options.ServiceBusConnectionString);
            ServiceBusReceiver receiver = _sbClient.CreateReceiver(_options.ServiceBusTopic, _options.ServiceBusSubscription, new ServiceBusReceiverOptions()
            {
                ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
            });
            return Task.Run(async () =>
            {
                while (this._isRunning)
                {
                    await foreach(var message in receiver.ReceiveMessagesAsync())
                    {
                        ProcessMessage(message);
                        
                    }
                }
            });
        }

        public void Dispose()
        {
            _sbClient?.DisposeAsync();
            _sbClient = null;
        }
    }
}

