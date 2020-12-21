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
        private Dictionary<string, string> settings;

        public MessageConsumer(ILogger<MessageConsumer> logger)
        {
            this.logger = logger;
        }

        public Dictionary<string, string> SettingsDictionary
        {
            get
            {
                return settings;
            }
            set
            {
                settings = value;
            }
        }

        public string BuildResponseMessage(string messagePrefix, string name)
        {
            return string.Format(messagePrefix, name);
        }

        public void ConsumeMessage(Dictionary<string, string> settings)
        {
            this.settings = settings;
            try
            {
                using IConnection connection = GetConnection(settings["HostName"], settings["UserName"], settings["Password"]);
                {
                    if (connection != null)
                    {
                        using IModel channel = GetChannel(connection, settings["QueueName"]);
                        {
                            if (channel != null)
                            {
                                Console.WriteLine("Waiting for messages . . . Press[ENTER] to exit.");
                                var consumer = new EventingBasicConsumer(channel);
                                consumer.Received += (model, e) =>
                                {
                                    var body = e.Body.ToArray();
                                    var message = GetMessageString(body);
                                    string name = string.Empty;
                                    if (IsValidMessage(message, out name))
                                    {
                                        string response = BuildResponseMessage(settings["ConsumerMessagePrefix"], name);
                                        logger.LogInformation(response);
                                        Console.WriteLine(response);
                                    }
                                    else
                                    {
                                        string error = "Message validation failed.";
                                        logger.LogError(error);
                                        Console.WriteLine(error);
                                    }
                                };
                                channel.BasicConsume(queue: settings["QueueName"], autoAck: true, consumer: consumer);
                                logger.LogInformation("Consumer responded.");
                                Console.WriteLine("RESPONSES: ");
                                Console.ReadLine();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, e.Message, new object[] { });
            }
        }

        public bool IsValidMessage(string message, out string userInput)
        {
            userInput = string.Empty;
            try
            {
                string validator = settings["ProducerMessagePrefix"];
                var definition = new { MessagePart = "", UserInput = "" };
                var messageObj = JsonConvert.DeserializeAnonymousType(message, definition);
                bool validation = ((messageObj != null) &&
                                    (!string.IsNullOrEmpty(messageObj.MessagePart)) &&
                                    (messageObj.MessagePart.StartsWith(validator)) &&
                                    (!string.IsNullOrEmpty(messageObj.UserInput)));
                userInput = messageObj.UserInput;
                return validation;
            }
            catch (Exception e)
            {
                logger.LogCritical(e, e.Message, new object[] { });
                return false;
            }
        }

        private IModel GetChannel(IConnection connection, string queueName)
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

        private IConnection GetConnection(string hostName, string userName, string password)
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

        private string GetMessageString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }
}