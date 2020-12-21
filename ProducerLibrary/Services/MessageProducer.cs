using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProducerLibrary.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProducerLibrary.Services
{
    public class MessageProducer : IMessageProducer
    {
        private readonly ILogger logger;

        public MessageProducer(ILogger<MessageProducer> logger)
        {
            this.logger = logger;
        }

        public void SendMessage(string input)
        {
            logger.LogInformation("Send message started . . .");
            if (IsValidInput(input))
            {
                Dictionary<string, string> config = GetConfigurationDictionary();
                using IConnection connection = GetConnection(config["HostName"], config["UserName"], config["Password"]);
                {
                    if (connection != null)
                    {
                        using IModel channel = GetChannel(connection, config["QueueName"]);
                        {
                            if (channel != null)
                            {
                                string message = BuildMessage(input);
                                byte[] messageBody = GetMessageByteArray(message);
                                PublishMessage(channel, config["QueueName"], messageBody);
                            }
                        }
                    }
                }
                logger.LogInformation("Send message completed.");
            }
            else
            {
                string error = "Input is invalid";
                Console.WriteLine(error);
                logger.LogError(error);
            }
        }

        private byte[] GetMessageByteArray(string message)
        {
            return Encoding.UTF8.GetBytes(message);
        }

        public IConnection GetConnection(string hostName, string userName, string password)
        {
            IConnection connection = null;
            try
            {
                ConnectionFactory connectionFactory = new ConnectionFactory
                {
                    HostName = hostName,
                    UserName = userName,
                    Password = password
                };
                connection = connectionFactory.CreateConnection();
                return connection;
            }
            catch (BrokerUnreachableException e)
            {
                logger.LogCritical(e, e.Message, new object[] { });
                throw new BrokerUnreachableException(e);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, e.Message, new object[] { });
                return connection;
            }
        }

        public IModel GetChannel(IConnection connection, string queueName)
        {
            try
            {
                var channel = connection.CreateModel();
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                return channel;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, e.Message, new object[] { });
                throw;
            }
        }

        public void PublishMessage(IModel channel, string queueName, byte[] messageBody)
        {
            try
            {
                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: messageBody);
                string message = "Message sent.";
                Console.WriteLine(message);
                logger.LogInformation(message);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, e.Message, new object[] { });
                throw;
            }
        }

        private Dictionary<string, string> GetConfigurationDictionary()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            result["HostName"] = "localhost";
            result["UserName"] = "guest";
            result["Password"] = "guest";
            result["QueueName"] = "iq-assessment";
            return result;
        }

        private string BuildMessage(string name)
        {
            var message = new { MessagePart = "Hello my name is, ", UserInput = name };
            return JsonConvert.SerializeObject(message);
        }

        private bool IsValidInput(string input)
        {
            return !string.IsNullOrEmpty(input);
        }
    }
}