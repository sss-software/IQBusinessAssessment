using CommonLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Configuration;

namespace ConsoleServiceA.Services
{
    public class BootstrapperService : Bootstrapper
    {
        public override void ConfigureServices()
        {
            base.ConfigureServices();
            string logFilePath = ConfigurationManager.AppSettings["ProducerLogFilePath"].ToString().Replace("{Date}", DateTime.Now.ToString("yyyyMMdd"));

            Log.Logger = new LoggerConfiguration().WriteTo.File(logFilePath).CreateLogger();

            base.ServiceCollection.AddLogging(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
                loggingBuilder.ClearProviders().AddSerilog(logger: Log.Logger, dispose: true);
            });
            base.ServiceProvider = base.ServiceCollection.BuildServiceProvider();
        }
    }
}