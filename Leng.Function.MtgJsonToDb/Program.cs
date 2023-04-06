using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Leng.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Leng.Function.MtgJsonToDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

var host = new HostBuilder()
    .ConfigureAppConfiguration((hostingContext, config) => {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services => {
        services.AddDbContextFactory<LengDbContext>(options => options
                    .UseSqlServer(Environment.GetEnvironmentVariable("sqlConnectionString"))
                    );
    })
    .Build();

host.Run();

