using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Blackbird.Functions
{
    public class UploadDecodedSubframe
    {
        private Services.IFDRDataNotificationService _fdrNotificationService;
        public UploadDecodedSubframe(Services.IFDRDataNotificationService fdrNotificationService)
        {
            _fdrNotificationService = fdrNotificationService;
        }

        [FunctionName("UploadDecodedData")]
        public async Task<IActionResult> UploadDecodedData(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            if (req.ContentType != "application/json")
                return new BadRequestResult();

            var queryParams = req.GetQueryParameterDictionary();
            if (!queryParams.TryGetValue("ident", out string? acIdent))
            {
                return new BadRequestResult();
            }


            using (StreamReader reader = new StreamReader(req.Body))
            {
                string json = await reader.ReadToEndAsync();
                Dictionary<string, List<object>>? values = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, List<object>>>(json, new System.Text.Json.JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });


                if (values == null)
                    return new BadRequestResult();

                await _fdrNotificationService.NotifyOfNewDecodedDataAsync(acIdent, values);

            }


            return new OkResult();
        }
    }
}

