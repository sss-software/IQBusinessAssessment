using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using ProducerLibrary.Services;
using Xunit;

namespace ConsoleServices.Tests
{
    public class MessageProducerTest
    {
        private Mock<ILogger<MessageProducer>> mockLogger = new Mock<ILogger<MessageProducer>>();

        [Theory]
        [InlineData("Peter James McConnery", true)]
        [InlineData("Neil", true)]
        [InlineData("McDonald", true)]
        [InlineData("??%&*", false)]
        [InlineData("37377*", false)]
        [InlineData("     ", false)]
        [InlineData("", false)]
        [InlineData("\n", false)]
        [InlineData("\t", false)]
        public void IsValidInput_Test(string name, bool expected)
        {
            var producer = new MessageProducer(mockLogger.Object);
            bool actual = producer.IsValidInput(name);
            Assert.Equal(expected, actual);
        }
    }
}