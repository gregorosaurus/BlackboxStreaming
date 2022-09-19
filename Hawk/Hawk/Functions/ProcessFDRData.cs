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
        private ILogger _log;

        public ProcessFDRData(
            IFDRConfigurationService fdrConfigService,
            IFDRNotificationService fdrNotificationService,
            ConfigurationCache configCache,
            ILogger<ProcessFDRData> log)
        {
            _fdrConfigService = fdrConfigService;
            _fdrNotificationService = fdrNotificationService;
            _configCache = configCache;
            _log = log;
        }


        [FunctionName("service")]
        public async Task Run([ServiceBusTrigger("sbt-blackbox-fdrraw", "sbs-blackbox-hawk", Connection = "AzureServiceBusConnectionString")] string sbMessageContent)
        {
#if DEBUG
        DateTime startTime = DateTime.UtcNow;
            _log.LogInformation($"Started processing message @ {startTime.ToString("yyyy-MM-dd\\THH:mm:ss")}");
#endif
        var exceptions = new List<Exception>();
            try
            {
                RawDataMessage? dataMessage = JsonSerializer.Deserialize<RawDataMessage>(sbMessageContent,
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                if (dataMessage != null)
                    await DecodeRawSubframeDataAsync(dataMessage!);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
#if DEBUG
            DateTime endTime = DateTime.UtcNow;
            _log.LogTrace($"Finished processing message @ {endTime.ToString("yyyy-MM-dd\\THH:mm:ss")}");
            _log.LogTrace($"Total processing time {(endTime - startTime).TotalSeconds}");
#endif

        }

        private async Task DecodeRawSubframeDataAsync(RawDataMessage message)
        {
            DataFrameConfiguration? config = _configCache.FindDataFrameConfig(message.AircraftIdentifier);
            if(config == null)
            {
                _log.LogWarning($"Unable to find configuration for aircraft with ident: {message.AircraftIdentifier!}");
                return;
            }

            FDRDecoder decoder = new FDRDecoder(config!);
            var decodedValues = decoder.DecodeSubframe(message.SubframeBinaryData);

            await _fdrNotificationService.SendNotificationOfDecodedDataAsync(message.AircraftIdentifier, decodedValues);
        }
    }
}

