using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Blackbird
{
    public class UploadSubframeData
    {
        private Services.IQARDataManagementService _qarService;
        private Services.IFDRDataNotificationService _fdrNotificationService;
        public UploadSubframeData(Services.IQARDataManagementService qarService, Services.IFDRDataNotificationService fdrNotificationService)
        {
            _qarService = qarService;
            _fdrNotificationService = fdrNotificationService;
        }

        [FunctionName("UploadSubframeData")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Subframe request called.");

            var queryParams = req.GetQueryParameterDictionary();
            if(!queryParams.TryGetValue("ident", out string? acIdent))
            {
                return new BadRequestResult();
            }

            byte[] subframeBuffer = new byte[req.Body.Length];
            int bytesRead = await req.Body.ReadAsync(subframeBuffer);
            if (bytesRead != req.Body.Length)
                throw new Exception("Invalid read");


            //wait until the data is saved. 
            await _qarService.UploadSubframeAsync(acIdent, subframeBuffer);

            //we dont want to await for this task to complete
            #pragma warning disable CS4014
            _fdrNotificationService.NotifyOfNewSubframeDataAsync(acIdent, subframeBuffer);

            return new OkObjectResult("Success");
        }
    }
}

