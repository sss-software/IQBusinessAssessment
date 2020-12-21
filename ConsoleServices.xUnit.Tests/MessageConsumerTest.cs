using ConsumerLibrary.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace ConsoleServices.Tests
{
    public class MessageConsumerTest
    {
        private readonly Mock<ILogger<MessageConsumer>> mockLogger = new Mock<ILogger<MessageConsumer>>();

        [Theory, MemberData(nameof(MessageTestData))]
        public void IsValidMessageTest(string message, bool expected)
        {
            var consumer = new MessageConsumer(mockLogger.Object);

            bool actual = consumer.IsValidMessage(message.ToString(), out string userInput);

            Assert.Equal(expected, actual);
        }

        public static IEnumerable<object[]> MessageTestData
        {
            get
            {
                var message1 = JsonConvert.SerializeObject(new { MessagePart = "Hello my name is, ", UserInput = "Neil" }).ToString();
                var message2 = JsonConvert.SerializeObject(new { MessagePart = "Hello, I am , ", UserInput = "Neil" }).ToString();
                var message3 = JsonConvert.SerializeObject(new { MessagePart = "  ", UserInput = "      " }).ToString();
                var message4 = JsonConvert.SerializeObject(new { MessagePart = "This is my name: ", UserInput = "" }).ToString();
                var message5 = JsonConvert.SerializeObject(new { MessagePart = "Hello my name is, ", UserInput = "Peter George Johnson  " }).ToString();
                return new[]
                {
                     new object[] { message1, true },
                     new object[] { message2, false },
                     new object[] { message3, false },
                     new object[] { message4, false },
                     new object[] { message5, true }
                };
            }
        }
    }
}