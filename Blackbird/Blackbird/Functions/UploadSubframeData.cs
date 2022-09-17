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
        private UploadSubframeData(Services.IQARDataManagementService qarService)
        {
            _qarService = qarService;
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

            await _qarService.UploadSubframeAsync(acIdent, req.Body);

            return new OkObjectResult("Success");
        }
    }
}

