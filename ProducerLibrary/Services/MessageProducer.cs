using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProducerLibrary.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Text.RegularExpressions;

namespace ProducerLibrary.Services
{
    public class MessageProducer : IMessageProducer
    {
        private readonly ILogger logger;
        private Dictionary<string, string> settings;

        public MessageProducer(ILogger<MessageProducer> logger)
        {
            this.logger = logger;
        }

        public string BuildMessage(string name)
        {
            object messageObject = new { MessagePart = settings["ProducerMessagePrefix"], UserInput = name };
            return JsonConvert.SerializeObject(messageObject);
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

        public bool IsValidInput(string input)
        {
            string pattern = @"[a-zA-Z]";
            var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);
            return !string.IsNullOrEmpty(input) && match.Success;
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

        public void SendMessage(string input, Dictionary<string, string> settings)
        {
            logger.LogInformation("Sending message...");
            this.settings = settings;
            if (IsValidInput(input))
            {
                using IConnection connection = GetConnection(settings["HostName"], settings["UserName"], settings["Password"]);
                {
                    if (connection != null)
                    {
                        using IModel channel = GetChannel(connection, settings["QueueName"]);
                        {
                            if (channel != null)
                            {
                                string message = BuildMessage(input);
                                byte[] messageBody = GetMessageByteArray(message);
                                logger.LogInformation("Message to be send:" + message);
                                PublishMessage(channel, settings["QueueName"], messageBody);
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
    }
}