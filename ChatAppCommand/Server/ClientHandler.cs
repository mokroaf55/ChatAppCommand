using System;
using System.IO;
using System.Net.Sockets;

namespace ChatAppServer
{
    public class ClientHandler
    {
        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        public string UserName { get; private set; } = "";
        private bool isAdmin = false;

        public ClientHandler(TcpClient tcpClient)
        {
            client = tcpClient;
            var stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };
        }

        public void HandleClient()
        {
            try
            {
                writer.WriteLine("Введіть своє ім'я:");
                string name = reader.ReadLine();

                if (ChatServer.IsBanned(name))
                {
                    writer.WriteLine("Вас забанено. Вхід заборонено.");
                    client.Close();
                    return;
                }

                UserName = name;
                if (UserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    isAdmin = true;
                    writer.WriteLine("Ви увійшли як адміністратор. Команди: /ban [ім'я], /exit");
                }

                ChatServer.AddClient(this);
                Console.WriteLine($"{UserName} приєднався до чату.");
                ChatServer.Broadcast($"{UserName} приєднався до чату!", this);

                string message;
                while ((message = reader.ReadLine()) != null)
                {
                    if (isAdmin && message.StartsWith("/ban "))
                        ChatServer.BanUser(message.Substring(5));
                    else if (isAdmin && message.StartsWith("/exit"))
                        break;
                    else
                    {
                        string fullMessage = $"{UserName}: {message}";
                        Console.WriteLine(fullMessage);
                        ChatServer.Broadcast(fullMessage, this);
                    }
                }
            }
            catch
            {
                // клієнт відключився
            }
            finally
            {
                ChatServer.RemoveClient(this);
                client.Close();
                Console.WriteLine($"{UserName} вийшов із чату.");
                ChatServer.Broadcast($"{UserName} покинув чат.", this);
            }
        }

        public void SendMessage(string message)
        {
            try { writer.WriteLine(message); } catch { }
        }

        public void Disconnect()
        {
            try { client.Close(); } catch { }
        }
    }
}
