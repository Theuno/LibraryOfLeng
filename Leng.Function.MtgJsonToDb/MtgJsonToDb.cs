using Castle.Core.Logging;
using Leng.Application.FunctionHandlers;
using Leng.Application.Services;
using Leng.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace Leng.Function.MtgJsonToDb {
    public class MtgJsonToDb
    {
        private readonly ILogger<MtgJsonToDb> _logger;
        private readonly IMTGDbService _dbService;
        private readonly Microsoft.Extensions.Logging.ILoggerFactory _loggerFactory;


        public MtgJsonToDb(Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, IMTGDbService dbService)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MtgJsonToDb>();
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
        }

        [Function("MtgJsonToDb")]
        public void Run([TimerTrigger("0 30 3 * * 1-5"
        , RunOnStartup = true
        )] MyInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");

            var handlerLogger = _loggerFactory.CreateLogger<MtgJsonToDbHandler>();
            MtgJsonToDbHandler mtgJsonToDbHandler = new MtgJsonToDbHandler(handlerLogger, _dbService);

            Task handlerTask = mtgJsonToDbHandler.Handle();
            handlerTask.Wait();
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
