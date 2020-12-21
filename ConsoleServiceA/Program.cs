using CommonLibrary;
using Microsoft.Extensions.Logging;
using ProducerLibrary.Interfaces;
using ProducerLibrary.Services;
using Serilog;
using System;

namespace ConsoleServiceA
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper bootstrapper = new Bootstrapper();
            var serviceProvider = bootstrapper.ServiceProvider;
           
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
                        producer.SendMessage(input);
                    }
                } while (!input.Equals("exit", StringComparison.OrdinalIgnoreCase));
            }
            catch(Exception e)
            {
                Log.Fatal(e.Message);
            }
            finally
            {
                Log.Information("Application ended.");
                Log.CloseAndFlush();
                bootstrapper.DisposeServices();
            }
        }
    }
}
