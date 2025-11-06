using System;

namespace ChatServerApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Chat Server";
            ChatServer server = new ChatServer(5000); 
            server.Start();

            Console.WriteLine("Server is running. Press Enter to stop....");
            Console.ReadLine();

            server.Stop();
        }
    }
}
