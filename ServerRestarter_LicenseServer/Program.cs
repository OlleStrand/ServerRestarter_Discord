using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Messaging;

namespace ServerRestarter_LicenseServer
{
    class Program
    {
        private static readonly string MainPath = @"FormatName:Direct=TCP:173.249.11.2\private$\MainQueue";

        static void Main(string[] args)
        {
            //TODO SEND BACK MESSAGE

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
