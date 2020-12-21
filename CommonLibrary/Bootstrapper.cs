using ConsumerLibrary.Interfaces;
using ConsumerLibrary.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProducerLibrary.Interfaces;
using ProducerLibrary.Services;
using Serilog;
using System;
using System.Configuration;

namespace CommonLibrary
{
    public class Bootstrapper
    {
        private readonly IServiceCollection serviceCollection;

        public Bootstrapper()
        {
            serviceCollection = new ServiceCollection();
            ConfigureServices();
        }

        public void DisposeServices()
        {
            if (ServiceProvider == null)
            {
                return;
            }
            if (ServiceProvider is IDisposable)
            {
                ((IDisposable)ServiceProvider).Dispose();
            }
        }

        public IServiceProvider ServiceProvider { get; private set; }
        private void ConfigureServices()
        {
            serviceCollection.AddTransient<IMessageProducer, MessageProducer>();
            serviceCollection.AddTransient<IMessageConsumer, MessageConsumer>();
            
            serviceCollection.AddSingleton(LoggerFactory.Create(builder => builder.AddSerilog()));
            var logFilePath = ConfigurationManager.AppSettings["ProducerLogFilePath"].ToString().Replace("{Date}", DateTime.Now.ToString("yyyyMMdd"));
            Log.Logger = new LoggerConfiguration().WriteTo.File(logFilePath).CreateLogger();

            serviceCollection.AddLogging(loggingBuilder =>
            {
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
                loggingBuilder.ClearProviders().AddSerilog(logger: Log.Logger, dispose: true);
            });
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
