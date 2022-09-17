using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Blackbird.Functions
{
    public class CheckForStaleRecordings
    {
        private Services.IQARDataManagementService _qarService;
        public CheckForStaleRecordings(Services.IQARDataManagementService qarMgmtService)
        {
            _qarService = qarMgmtService;
        }

        [FunctionName("CheckForStaleRecordings")]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Checking for stale recordings: {DateTime.Now}");
            await _qarService.CloseStaleRecordingsAsync();
        }
    }
}

