using System;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using Hawk.Models;

namespace Hawk.Functions
{
    public class ProcessFDRData
    {
        [FunctionName("ProcessFDRSubframe")]
        public async Task ProcessFDRSubframe([EventHubTrigger("evt-blackbox-fdrraw", Connection = "EventHubConnectionString")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    RawDataMessage? dataMessage = JsonSerializer.Deserialize<RawDataMessage>(eventData.EventBody.ToString());
                    if (dataMessage != null)
                        await DecodeRawSubframeDataAsync(dataMessage!);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }

        private Task DecodeRawSubframeDataAsync(RawDataMessage message)
        {
            throw new NotImplementedException();
        }
    }
}

