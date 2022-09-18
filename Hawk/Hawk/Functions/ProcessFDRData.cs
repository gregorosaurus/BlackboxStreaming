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
using Hawk.Decode.Configuration.Model;

namespace Hawk.Functions
{
    public class ProcessFDRData
    {

        private IFDRConfigurationService _fdrConfigService;
        private IFDRNotificationService _fdrNotificationService;
        private ConfigurationCache _configCache;
        public ProcessFDRData(
            IFDRConfigurationService fdrConfigService,
            IFDRNotificationService fdrNotificationService,
            ConfigurationCache configCache)
        {
            _fdrConfigService = fdrConfigService;
            _fdrNotificationService = fdrNotificationService;
            _configCache = configCache;
        }


        [FunctionName("ProcessFDRSubframe")]
        public async Task ProcessFDRSubframe([EventHubTrigger("evh-blackbox-fdrraw", Connection = "EventHubConnectionString", ConsumerGroup = "hawk")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    RawDataMessage? dataMessage = JsonSerializer.Deserialize<RawDataMessage>(eventData.EventBody.ToString(),
                        new JsonSerializerOptions()
                        {
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                    if (dataMessage != null)
                        await DecodeRawSubframeDataAsync(dataMessage!, log);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }
        }

        private async Task DecodeRawSubframeDataAsync(RawDataMessage message, ILogger log)
        {
            DataFrameConfiguration? config = _configCache.FindDataFrameConfig(message.AircraftIdentifier);
            if(config == null)
            {
                log.LogWarning($"Unable to find configuration for aircraft with ident: {message.AircraftIdentifier!}");
                return;
            }

            FDRDecoder decoder = new FDRDecoder(config!);
            var decodedValues = decoder.DecodeSubframe(message.SubframeBinaryData);

            await _fdrNotificationService.SendNotificationOfDecodedDataAsync(message.AircraftIdentifier, decodedValues);
        }
    }
}

