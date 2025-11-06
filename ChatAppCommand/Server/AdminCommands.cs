using System;
using System.Collections.Generic;

namespace ChatServerApp
{
    public class AdminCommands
    {
        private ChatServer server;

        public AdminCommands(ChatServer server)
        {
            this.server = server;
        }

        public void Execute(string command, ClientHandler sender)
        {
            string[] parts = command.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            switch (parts[0].ToLower())
            {
                case "/ban":
                    if (parts.Length < 2)
                    {
                        sender.SendMessage("Using: /ban <user name>");
                        return;
                    }
                    server.BanUser(parts[1], sender);
                    break;

                case "/kick":
                    if (parts.Length < 2)
                    {
                        sender.SendMessage("Using: /kick <user name>");
                        return;
                    }
                    server.KickUser(parts[1], sender);
                    break;

                case "/msg":
                    if (parts.Length < 3)
                    {
                        sender.SendMessage("Using: /msg <ім'я> <message>");
                        return;
                    }
                    server.PrivateMessage(parts[1], parts[2], sender);
                    break;

                case "/all":
                    if (parts.Length < 2)
                    {
                        sender.SendMessage("Using: /all <message>");
                        return;
                    }
                    server.Broadcast($"[ADMIN]: {parts[1]}", sender);
                    break;

                default:
                    sender.SendMessage("Unknown team.");
                    break;
            }
        }
    }
}
