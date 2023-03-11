using System;
using Leng.Application;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Leng.Function.MtgJsonToDb
{
    public class MtgJsonToDb
    {
        private readonly ILogger _logger;

        public MtgJsonToDb(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MtgJsonToDb>();
        }

        [Function("MtgJsonToDb")]
        public void Run([TimerTrigger("0 */5 * * * *"
            #if DEBUG
                ,RunOnStartup=true
            #endif
            )] MyInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");

            MtgJsonToDbHandler mtgJsonToDbHandler = new MtgJsonToDbHandler();
            mtgJsonToDbHandler.Handle();
        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
