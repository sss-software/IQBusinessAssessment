using CommonLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace ConsoleServiceB.Services
{
    public class BootstrapperService : Bootstrapper
    {
        public override void ConfigureServices()
        {
            base.ConfigureServices();
            string logFilePath = ConfigurationManager.AppSettings["ConsumerLogFilePath"].ToString().Replace("{Date}", DateTime.Now.ToString("yyyyMMdd"));

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