using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalR.Client.Hubs;

namespace SignalRTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var connection = new HubConnection("http://localhost/WebApplication2/");

            var hub = connection.CreateProxy("Chat");

            hub.On<string>("AddMessage", Console.WriteLine);

            connection.Start().ContinueWith(t =>
                {
                    hub.Invoke("Send", "Hello there from command line");
                });

            Console.ReadLine();
        }
    }
}
