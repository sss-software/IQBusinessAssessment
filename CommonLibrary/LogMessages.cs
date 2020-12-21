using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary
{
    public static class LogMessages
    {
        public static string ProducerStartedMessage => "ConsoleServiceA started at: {0}";
        public static string ProducerEndedMessage => "ConsoleServiceA ended at: {0}";

        public static string ConsumerStartedMessage => "ConsoleServiceB started at: {0}";
        public static string ConsumerEndedMessage => "ConsoleServiceB ended at: {0}";

        public static string WaitingForMessage => "Waiting for messages . . . Press[ENTER] to exit.";
    }
}