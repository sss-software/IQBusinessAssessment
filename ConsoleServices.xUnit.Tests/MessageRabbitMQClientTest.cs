using ConsumerLibrary.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ProducerLibrary.Services;
using RabbitMQ.Client.Events;
using RabbitMQ.Fakes.DotNetStandard;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ConsoleServices.Tests
{
    public class MessageRabbitMQClientTest
    {
        private readonly string queueName = "iq-assessment";
        private readonly string producerMessagePrefix = "Hello my name is,";
        private readonly string consumerMessagePrefix = "Hello {0}, I am your father!";
        private readonly Mock<ILogger<MessageProducer>> mockProducerLogger = new Mock<ILogger<MessageProducer>>();

        private readonly Mock<ILogger<MessageConsumer>> mockConsumerLogger = new Mock<ILogger<MessageConsumer>>();

        [Theory, MemberData(nameof(MessageTestData))]
        public void ConsumeMessageTest(string producerMessage)
        {
            string expectedResponseMessage = string.Empty;
            string actualResponseMessage = string.Empty;
            string name;

            var rabbitServer = GetConfiguredTestRabbitServer();
            SendMessage(rabbitServer, producerMessage);

            var connectionFactory = new FakeConnectionFactory(rabbitServer);
            using var connection = connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();
            var consumer = new MessageConsumer(mockConsumerLogger.Object)
            {
                SettingsDictionary = new Dictionary<string, string>
                {
                    ["ProducerMessagePrefix"] = producerMessagePrefix
                }
            };

            var received = channel.BasicGet(queueName, autoAck: true);
            var messageBody = Encoding.ASCII.GetString(received.Body.ToArray());
            if (consumer.IsValidMessage(messageBody, out name))
            {
                actualResponseMessage = consumer.BuildResponseMessage(consumerMessagePrefix, name);
                expectedResponseMessage = string.Format(consumerMessagePrefix, name);
            }

            int expectedMessageCount = 0;
            int actualMessageCount = rabbitServer.Queues[queueName].Messages.Count;

            Assert.NotNull(producerMessage);
            Assert.NotNull(actualResponseMessage);
            Assert.NotNull(expectedResponseMessage);

            Assert.Equal(expectedMessageCount, actualMessageCount);
            Assert.Equal(expectedResponseMessage, actualResponseMessage);
        }

        [Theory]
        [InlineData("Neil", 1)]
        public void SendMessageTest(string message, int expected)
        {
            var rabbitServer = GetConfiguredTestRabbitServer();
            var connectionFactory = new FakeConnectionFactory(rabbitServer);
            ConfigureQueueBinding(rabbitServer, queueName);

            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var messageBody = Encoding.ASCII.GetBytes(message);
                channel.BasicPublish(exchange: "", routingKey: queueName, mandatory: false, basicProperties: null, body: messageBody);
            }
            int actual = rabbitServer.Queues["iq-assessment"].Messages.Count;

            Assert.Equal(actual, expected);
        }

        private RabbitServer GetConfiguredTestRabbitServer()
        {
            var rabbitServer = new RabbitServer();
            if (rabbitServer.Queues != null)
            {
                rabbitServer.Queues.Clear();
                if (rabbitServer.Queues.ContainsKey(queueName))
                {
                    rabbitServer.Queues[queueName].Messages.Clear();
                }
            }
            ConfigureQueueBinding(rabbitServer, queueName);
            return rabbitServer;
        }

        private void ConfigureQueueBinding(RabbitServer rabbitServer, string queueName)
        {
            var connectionFactory = new FakeConnectionFactory(rabbitServer);
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queueName, "", "", null);
            }
        }

        private void SendMessage(RabbitServer rabbitServer, string message)
        {
            var connectionFactory = new FakeConnectionFactory(rabbitServer);

            using (var connection = connectionFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var messageBody = Encoding.ASCII.GetBytes(message);
                    channel.BasicPublish(exchange: "", routingKey: queueName, mandatory: false, basicProperties: null, body: messageBody);
                }
            }
        }

        public static IEnumerable<object[]> MessageTestData
        {
            get
            {
                var message1 = JsonConvert.SerializeObject(new { MessagePart = "Hello my name is, ", UserInput = "Neil" }).ToString();
                //var message2 = JsonConvert.SerializeObject(new { MessagePart = "Hello, I am , ", UserInput = "Neil" }).ToString();
                //var message3 = JsonConvert.SerializeObject(new { MessagePart = "  ", UserInput = "      " }).ToString();
                //var message4 = JsonConvert.SerializeObject(new { MessagePart = "This is my name: ", UserInput = "" }).ToString();
                //var message5 = JsonConvert.SerializeObject(new { MessagePart = "Hello my name is, ", UserInput = "Peter George Johnson  " }).ToString();
                return new[]
                {
                     new object[] { message1},
                     //new object[] { message2},
                     //new object[] { message3},
                     //new object[] { message4},
                     //new object[] { message5}
                };
            }
        }
    }
}