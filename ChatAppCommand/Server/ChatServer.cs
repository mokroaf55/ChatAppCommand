using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServerApp
{
    public class ChatServer
    {
        private TcpListener listener;
        private bool isRunning;
        private List<ClientHandler> clients = new List<ClientHandler>();
        private readonly object lockObj = new object();

        public ChatServer(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            listener.Start();
            isRunning = true;

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();

            Console.WriteLine("The server is running and waiting for connections....");
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("New client connected.");

                ClientHandler handler = new ClientHandler(client, this);
                lock (lockObj)
                {
                    clients.Add(handler);
                }

                Thread clientThread = new Thread(handler.Handle);
                clientThread.Start();
            }
        }

        public void Broadcast(string message, ClientHandler sender)
        {
            lock (lockObj)
            {
                foreach (var client in clients)
                {
                    if (client != sender)
                    {
                        client.SendMessage(message);
                    }
                }
            }
        }

        public void RemoveClient(ClientHandler client)
        {
            lock (lockObj)
            {
                clients.Remove(client);
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener.Stop();

            lock (lockObj)
            {
                foreach (var client in clients)
                {
                    client.Disconnect();
                }
            }
        }
        public void BanUser(string userName, ClientHandler admin)
        {
            var client = clients.Find(c => c.UserName == userName);
            if (client != null)
            {
                client.IsBanned = true;
                client.SendMessage("You have been banned by the administrator..");
                client.Disconnect();
                Console.WriteLine($"{userName} banned by admin {admin.UserName}");
            }
        }

        public void KickUser(string userName, ClientHandler admin)
        {
            var client = clients.Find(c => c.UserName == userName);
            if (client != null)
            {
                client.SendMessage("You have been removed by the administrator..");
                client.Disconnect();
                Console.WriteLine($"{userName} deleted by admin {admin.UserName}");
            }
        }

        public void PrivateMessage(string targetName, string message, ClientHandler sender)
        {
            var client = clients.Find(c => c.UserName == targetName);
            if (client != null)
            {
                client.SendMessage($"(Private from {sender.UserName}): {message}");
            }
            else
            {
                sender.SendMessage($"User {targetName} not found.");
            }
        }
        public void AdminCommand(string command, ClientHandler admin)
        {
            AdminCommands adminCmd = new AdminCommands(this);
            adminCmd.Execute(command, admin);
        }

    }
}
