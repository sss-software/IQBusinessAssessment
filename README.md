# IQBusinessAssessment
IQ Business - Assessment Project of Neil Slambert
The solutions consists of 2 dotnet core console applications: ConsoleServiceA and ConsoleServiceB.
Each of the console app uses the RabbitMQ.Client library to communicate with a RabbitMQ instance
The 1st console application (ConsoleServiceA) reads an input string via Console.ReadLine() – "Name"
It then sends a string message – "Hello my name is, {Name}" to the 2nd console application (ConsoleServiceB)
The 2nd console application (ConsoleServiceB) listens for a string message
Upon receiving a message, it validates it to ensure the application can respond to the 'Name' received.
Output to the console a response – "Hello {ReceivedName}, I am your father!"
