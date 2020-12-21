using System;
using System.Collections.Generic;
using System.Text;

namespace ProducerLibrary.Interfaces
{
    public interface IMessageProducer
    {
        void SendMessage(string input, Dictionary<string, string> settingsDictionary);
    }
}