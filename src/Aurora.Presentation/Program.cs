using Aurora.Presentation.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace Aurora.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using IHost host = CreateHostBuilder(args).Build();
                using (var scope = host.Services.CreateScope())
                {
                    scope.ServiceProvider.MigrateDatabase();
                    scope.ServiceProvider.StartRecurringJobs();
                }
                host.Run();
            }
            catch (Exception ex)
            {
                if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
                {
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console()
                        .CreateLogger();
                }

                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                   {
                       webBuilder
                        .UseStartup<Startup>();
                   })
                .UseSerilog((hostingContext, loggerConfiguration) =>
                   {
                       loggerConfiguration
                           .ReadFrom.Configuration(hostingContext.Configuration)
                           .Enrich.FromLogContext()
                           .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name!)
                           .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment);
                   });
    }
}