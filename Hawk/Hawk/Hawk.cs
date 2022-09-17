using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Hawk.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Hawk
{
    public class Hawk
    {
        [FunctionName("Hawk")]
        public async Task ProcessRawFDRData([EventHubTrigger("evt-blackbox-fdrraw", Connection = "EventHubConnectionString")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    // Replace these two lines with your processing logic.
                    RawDataMessage? dataMsg = JsonSerializer.Deserialize<RawDataMessage>(eventData.EventBody.ToString());
                    if (dataMsg != null)
                        DecodeSubframe(dataMsg!);
                                            
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }

        private void DecodeSubframe(RawDataMessage dataMsg)
        {

        }
    }
}

