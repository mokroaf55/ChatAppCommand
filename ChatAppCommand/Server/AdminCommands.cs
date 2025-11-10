using System;
using System.Linq;

namespace ChatAppServer
{
    public static class AdminCommands
    {
        public static void Execute(string command)
        {
            string[] parts = command.Split(' ');
            string cmd = parts[0].ToLower();

            switch (cmd)
            {
                case "/broadcast":
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("Вкажіть повідомлення для розсилки.");
                        return;
                    }
                    string message = string.Join(' ', parts.Skip(1));
                    ChatServer.Broadcast($"[Адмін]: {message}", null);
                    break;

                case "/kick":
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("Вкажіть ім'я користувача для кікання.");
                        return;
                    }
                    ChatServer.KickUser(parts[1]);
                    break;

                case "/ban":
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("Вкажіть ім'я користувача для бану.");
                        return;
                    }
                    ChatServer.BanUser(parts[1]);
                    break;

                default:
                    Console.WriteLine("Невідома команда адміністратора.");
                    break;
            }
        }
    }
}
