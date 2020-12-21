using CommonLibrary;
using ConsumerLibrary.Interfaces;
using ConsumerLibrary.Services;
using System;

namespace ConsoleServiceB
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper bootstrapper = new Bootstrapper();
            var serviceProvider = bootstrapper.ServiceProvider;
            try
            {
                IMessageConsumer consumer = (IMessageConsumer)serviceProvider.GetService(typeof(IMessageConsumer));
                consumer.ConsumeMessage();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            finally
            {
                bootstrapper.DisposeServices();
            }
        }
    }
}
