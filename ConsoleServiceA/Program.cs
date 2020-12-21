using CommonLibrary;
using ConsoleServiceA.Services;
using Microsoft.Extensions.Logging;
using ProducerLibrary.Interfaces;
using ProducerLibrary.Services;
using Serilog;
using System;
using System.Collections.Generic;

namespace ConsoleServiceA
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BootstrapperService bootstrapper = new BootstrapperService();
            var serviceProvider = bootstrapper.ServiceProvider;
            Dictionary<string, string> settings = bootstrapper.GetSettingsDictionary();
            Log.Information(string.Format(LogMessages.ProducerStartedMessage, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            string input;
            try
            {
                do
                {
                    Console.Write("Enter your name: ");
                    input = Console.ReadLine();
                    if (!input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        var producer = (IMessageProducer)serviceProvider.GetService(typeof(IMessageProducer));
                        producer.SendMessage(input, settings);
                    }
                } while (!input.Equals("exit", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception e)
            {
                Log.Fatal(e.Message);
                Environment.Exit(-1);
            }
            finally
            {
                Log.Information(string.Format(LogMessages.ProducerEndedMessage, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                Log.CloseAndFlush();
                bootstrapper.DisposeServices();
                Environment.Exit(0);
            }
        }
    }
}