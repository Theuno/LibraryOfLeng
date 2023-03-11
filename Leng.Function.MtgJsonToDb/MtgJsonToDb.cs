using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Leng.Function.MtgJsonToDb
{
    public class MtgJsonToDb
    {
        [FunctionName("MtgJsonToDb")]
        public async Task Run([TimerTrigger("0 */5 * * * *"
            #if DEBUG
                ,RunOnStartup=true
            #endif
            )]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var _handler = new Leng.Application.MtgJsonToDbHandler();
            await _handler.Handle();
        }
    }
}
