using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ChatAppServer
{
    public static class ChatServer
    {
        private static TcpListener listener;
        private static List<ClientHandler> clients = new List<ClientHandler>();
        private static HashSet<string> bannedUsers = new HashSet<string>();
        private static object locker = new object();

        public static void Start()
        {
            listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            Console.WriteLine("Сервер запущено. Очікується підключення клієнтів...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ClientHandler handler = new ClientHandler(client);
                new Thread(handler.HandleClient).Start();
            }
        }

        public static void Broadcast(string message, ClientHandler sender)
        {
            lock (locker)
            {
                foreach (var client in clients)
                    if (client != sender)
                        client.SendMessage(message);
            }
        }

        public static void AddClient(ClientHandler client)
        {
            lock (locker) clients.Add(client);
        }

        public static void RemoveClient(ClientHandler client)
        {
            lock (locker) clients.Remove(client);
        }

        public static bool IsBanned(string name) => bannedUsers.Contains(name.ToLower());

        public static void BanUser(string name)
        {
            bannedUsers.Add(name.ToLower());
            lock (locker)
            {
                foreach (var c in clients)
                {
                    if (c.UserName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        c.SendMessage("Вас забанено адміністратором. З'єднання буде розірвано.");
                        c.Disconnect();
                        break;
                    }
                }
            }
            Console.WriteLine($"Користувача {name} забанено.");
        }

        // --- новий метод для кікання ---
        public static void KickUser(string name)
        {
            lock (locker)
            {
                foreach (var c in clients)
                {
                    if (c.UserName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        c.SendMessage("Вас кікнуто адміністратором. З'єднання буде розірвано.");
                        c.Disconnect();
                        break;
                    }
                }
            }
            Console.WriteLine($"Користувача {name} кікнуто.");
        }
    }
}
