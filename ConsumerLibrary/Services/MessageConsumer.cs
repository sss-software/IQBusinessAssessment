using ConsumerLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsumerLibrary.Services
{
    public class MessageConsumer : IMessageConsumer
    {
        private readonly ILogger logger;
        public MessageConsumer(ILogger<MessageConsumer> logger)
        {
            this.logger = logger;
        }
        public void ConsumeMessage()
        {
            logger.LogInformation("Consume message started . . .");
            try
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
                                Console.WriteLine("Waiting for messages . . .");
                                var consumer = new EventingBasicConsumer(channel);
                                consumer.Received += (model, e) =>
                                {
                                    var body = e.Body.ToArray();
                                    var message = GetMessageString(body);
                                    string name = string.Empty;
                                    if (IsValidMessage(message, out name))
                                    {
                                        string response = BuildMessage(name);
                                        logger.LogInformation(response);
                                        Console.WriteLine(response);
                                    }
                                    else
                                    {
                                        string error = "Message validation failed.";
                                        Console.WriteLine(error);
                                        logger.LogError(error);
                                    }
                                };
                            }
                        }
                    }
                }
                logger.LogInformation("Consume message completed.");
            }
            catch(Exception e)
            {
                logger.LogCritical(e, e.Message, new object[] { });
            }
        }

        public string GetMessageString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
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

        private Dictionary<string, string> GetConfigurationDictionary()
        {
            Dictionary<string, string> result = new Dictionary<string, string>
            {
                ["HostName"] = "localhost",
                ["UserName"] = "guest",
                ["Password"] = "guest",
                ["QueueName"] = "iq-assessment"
            };
            return result;
        }

        private bool IsValidMessage(string message, out string userInput)
        {
            var definition = new { MessagePart = "", UserInput = "" };
            var messageObj = JsonConvert.DeserializeAnonymousType(message, definition);
            bool validation = ((messageObj != null) &&
                                (!string.IsNullOrEmpty(messageObj.MessagePart)) &&
                                (messageObj.MessagePart.StartsWith("Hello my name is,")) &&
                                (!string.IsNullOrEmpty(messageObj.UserInput)));
            userInput = messageObj.UserInput;
            return validation;
        }

        private string BuildMessage(string name)
        {
            return string.Format("Hello {0}, I am your father!", name);
        }
    }
}
