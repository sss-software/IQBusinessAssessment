using CommonLibrary;
using ConsoleServiceB.Services;
using ConsumerLibrary.Interfaces;
using ConsumerLibrary.Services;
using Serilog;
using System;
using System.Collections.Generic;

namespace ConsoleServiceB
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BootstrapperService bootstrapper = new BootstrapperService();
            Dictionary<string, string> settings = bootstrapper.GetSettingsDictionary();
            var serviceProvider = bootstrapper.ServiceProvider;
            Log.Information(string.Format(LogMessages.ConsumerStartedMessage, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            try
            {
                IMessageConsumer consumer = (IMessageConsumer)serviceProvider.GetService(typeof(IMessageConsumer));
                consumer.ConsumeMessage(settings);
            }
            catch (Exception e)
            {
                Log.Fatal(e.Message);
                Environment.Exit(-1);
            }
            finally
            {
                Log.Information(string.Format(LogMessages.ConsumerEndedMessage, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                Log.CloseAndFlush();
                bootstrapper.DisposeServices();
                Environment.Exit(0);
            }
        }
    }
}