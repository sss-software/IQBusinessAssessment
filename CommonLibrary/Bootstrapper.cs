using ConsumerLibrary.Interfaces;
using ConsumerLibrary.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProducerLibrary.Interfaces;
using ProducerLibrary.Services;
using Serilog;
using System;
using System.Collections.Generic;
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

        public ServiceCollection ServiceCollection => (ServiceCollection)serviceCollection;

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

        public Dictionary<string, string> GetSettingsDictionary()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings.Add("HostName", ConfigurationManager.AppSettings["HostName"].ToString());
            settings.Add("UserName", ConfigurationManager.AppSettings["UserName"].ToString());
            settings.Add("Password", ConfigurationManager.AppSettings["Password"].ToString());
            settings.Add("QueueName", ConfigurationManager.AppSettings["QueueName"].ToString());
            settings.Add("ProducerMessagePrefix", ConfigurationManager.AppSettings["ProducerMessagePrefix"].ToString());
            settings.Add("ConsumerMessagePrefix", ConfigurationManager.AppSettings["ConsumerMessagePrefix"].ToString());
            return settings;
        }

        public IServiceProvider ServiceProvider { get; set; }

        public virtual void ConfigureServices()
        {
            serviceCollection.AddTransient<IMessageProducer, MessageProducer>();
            serviceCollection.AddTransient<IMessageConsumer, MessageConsumer>();
            serviceCollection.AddSingleton(LoggerFactory.Create(builder => builder.AddSerilog()));
        }
    }
}