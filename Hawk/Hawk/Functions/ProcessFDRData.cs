using System;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using Hawk.Models;
using Hawk.Services;
using Hawk.Decode;

namespace Hawk.Functions
{
    public class ProcessFDRData
    {
        private IFDRConfigurationService _fdrConfigService;
        private IFDRNotificationService _fdrNotificationService;
        public ProcessFDRData(IFDRConfigurationService fdrConfigService, IFDRNotificationService fdrNotificationService)
        {
            _fdrConfigService = fdrConfigService;
            _fdrNotificationService = fdrNotificationService;
        }


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
                        await DecodeRawSubframeDataAsync(dataMessage!, log);
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

        private async Task DecodeRawSubframeDataAsync(RawDataMessage message, ILogger log)
        {
            var config = await _fdrConfigService.FindConfigurationAsync(message.AircraftIdentifier!);
            if(config == null)
            {
                log.LogWarning($"Unable to find configuration for aircraft with ident: {message.AircraftIdentifier!}");
                return;
            }

            FDRDecoder decoder = new FDRDecoder(config!);
            var decodedValues = decoder.DecodeSubframe(message.SubframeBinaryData);

            await _fdrNotificationService.SendNotificationOfDecodedDataAsync(decodedValues);
        }
    }
}

