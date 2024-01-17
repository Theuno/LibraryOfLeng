using Leng.Application.Services;
using Leng.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var host = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, config) => {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); // Ensure this line is added
        config.AddEnvironmentVariables();
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services => {
        services.AddLogging();
        services.AddDbContextFactory<LengDbContext>(options => options
                    .UseSqlServer(Environment.GetEnvironmentVariable("sqlConnectionString"))
                    );
        services.AddTransient<IMTGDbService, MTGDbService>(serviceProvider =>
        {
            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<LengDbContext>>();
            var logger = serviceProvider.GetRequiredService<ILogger<MTGDbService>>();
            return new MTGDbService(dbContextFactory, logger);
        });
    })
    .Build();

host.Run();

