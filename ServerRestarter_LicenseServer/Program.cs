using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;
using System.Threading;

namespace ServerRestarter_LicenseServer
{
    class Program
    {
        private static CancellationToken cancellationToken;

        static void Main(string[] args)
        {
            //string MainPath = @"FormatName:Direct=TCP:173.249.11.2\private$\";
            string MainPath = @".\private$\";

            using (MessageQueue input = new MessageQueue(MainPath + "MainQueue", QueueAccessMode.Receive)
            {
                MessageReadPropertyFilter = { Id = true, Body = true },
                Formatter = new XmlMessageFormatter(new string[] { "System.String,mscorlib" })
            })
            {
                Console.WriteLine("Running and Listening...");
                while (!cancellationToken.IsCancellationRequested)
                {
                    Message message = input.Receive();
                    Console.WriteLine($"Message Received - {DateTime.Now}");

                    var data = message.Body.ToString();
                    Console.WriteLine($"Received: {data}");

                    string[] dataArray = data.Split('|');

                    using (MessageQueue output = new MessageQueue(MainPath + "ReceiveQueue", QueueAccessMode.Send))
                    {
                        //Do Database stuff

                        Message msg = new Message
                        {
                            Body = "true",
                            Label = "LicenseResponse",
                            TimeToReachQueue = new TimeSpan(0, 0, 20),
                            TimeToBeReceived = new TimeSpan(0, 0, 40),
                            CorrelationId = message.Id
                        };

                        Console.WriteLine($"Sending Message: {msg.Body} with CorrleationId: {msg.CorrelationId}");
                        output.Send(msg);

                        Thread.Sleep(1000);
                        Console.WriteLine("\nListening...");
                    }
                }
            }

            Console.ReadKey();
        }
    }
}