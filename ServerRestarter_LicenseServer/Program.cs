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
        //private static readonly string MainPath = @"FormatName:Direct=TCP:173.249.11.2\private$\MainQueue";
        private static CancellationToken cancellationToken;

        static void Main(string[] args)
        {
            string MainPath = @"FormatName:Direct=TCP:173.249.11.2\private$\";

            using (MessageQueue input = new MessageQueue(MainPath + "MainQueue", QueueAccessMode.Receive)
            {
                MessageReadPropertyFilter = { Id = true, Body = true },
                Formatter = new ActiveXMessageFormatter()
            })
            {
                Console.WriteLine("Running...");
                while (!cancellationToken.IsCancellationRequested)
                {
                    Message message = input.Receive();
                    Console.WriteLine("Message Received");

                    string data = message.Body.ToString();
                    Console.WriteLine($"Received: {data}");

                    string[] dataArray = data.Split('|');

                    using (MessageQueue output = new MessageQueue(MainPath + dataArray[3], QueueAccessMode.Send))
                    {
                        if (!MessageQueue.Exists(output.Path))
                        {
                            try
                            {
                                MessageQueue.Create(output.Path);

                                Console.WriteLine("Queue Created:");
                                Console.WriteLine($"Path: {output.Path}");
                            }
                            catch (MessageQueueException mqx)
                            {
                                Console.WriteLine(mqx.ToString());
                            }
                        }
                    }
                }
            }

            Console.ReadKey();
        }
    }
}

//Handle this on the server. Ignore on client stuff later
/*if (!MessageQueue.Exists(msmq.Path))
{
    try
    {
        MessageQueue.Create(msmq.Path);
    }
    catch (MessageQueueException mqx)
    {
        Console.WriteLine(mqx.ToString());
    }
    Console.WriteLine("Queue Created:");
    Console.WriteLine($"Path: {msmq.Path}");
    Console.WriteLine($"FormatName: {msmq.FormatName}");
}*/
