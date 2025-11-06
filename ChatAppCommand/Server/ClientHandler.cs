using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ChatServerApp
{
    public class ClientHandler
    {
        private TcpClient client;
        private ChatServer server;
        private StreamReader reader;
        private StreamWriter writer;
        public string UserName { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool IsBanned { get; set; }

        public ClientHandler(TcpClient client, ChatServer server)
        {
            this.client = client;
            this.server = server;
            NetworkStream stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
        }

        public void Handle()
        {
            try
            {
                writer.WriteLine("Enter your username.:");
                UserName = reader.ReadLine();

                if (UserName.ToLower() == "admin")
                {
                    IsAdmin = true;
                    writer.WriteLine("You are logged in as an administrator..");
                }
                else
                {
                    writer.WriteLine($"Welcome, {UserName}!");
                }

                Console.WriteLine($"{UserName} connected to the server.");

                string message;
                while ((message = reader.ReadLine()) != null)
                {
                    if (IsBanned)
                    {
                        writer.WriteLine("You are blocked on the server..");
                        break;
                    }

                    if (IsAdmin && message.StartsWith("/"))
                    {
                        server.AdminCommand(message, this);
                    }
                    else
                    {
                        string formatted = $"{UserName}: {message}";
                        Console.WriteLine(formatted);
                        server.Broadcast(formatted, this);
                    }
                }
            }
            catch
            {
                Console.WriteLine($"{UserName} disconnected.");
            }
            finally
            {
                Disconnect();
                server.RemoveClient(this);
            }
        }

        public void SendMessage(string message)
        {
            try
            {
                writer.WriteLine(message);
            }
            catch
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            reader?.Close();
            writer?.Close();
            client?.Close();
        }
    }
}
