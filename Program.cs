using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Homework_07_12
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Задание 1 - сервер
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 8888;

            Socket ls = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ls.Bind(new IPEndPoint(ip, port));
            ls.Listen(10);
            List<(Socket, string)> Clients = new List<(Socket, string)>();

            Random r = new Random();
            int num = r.Next(1, 11);

            Console.WriteLine("Игра началась!");

            while (true)
            {
                Socket c = await ls.AcceptAsync();
                byte[] nicknameData = new byte[256];
                int nicknameBytes = await c.ReceiveAsync(new ArraySegment<byte>(nicknameData), SocketFlags.None);
                string nickname = Encoding.UTF8.GetString(nicknameData, 0, nicknameBytes);

                Clients.Add((c, nickname));
                Console.WriteLine($"{nickname} присоединился к игре.");

                _ = Task.Run(() => ProcessClient(c, nickname, Clients, num));
            }
        }

        static async Task ProcessClient(Socket client, string nickname, List<(Socket, string)> clients, int num)
        {
            byte[] data = new byte[256];
            while (true)
            {
                int bytes = await client.ReceiveAsync(new ArraySegment<byte>(data), SocketFlags.None);
                string message = Encoding.UTF8.GetString(data, 0, bytes);
                string[] cm = message.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                if (cm.Length > 0)
                {
                    int cg = int.Parse(cm[0]);

                    if (cg > num)
                    {
                        byte[] response = Encoding.UTF8.GetBytes("Ваше число больше");
                        await client.SendAsync(new ArraySegment<byte>(response), SocketFlags.None);
                    }
                    else if (cg < num)
                    {
                        byte[] response = Encoding.UTF8.GetBytes("Ваше число меньше");
                        await client.SendAsync(new ArraySegment<byte>(response), SocketFlags.None);
                    }
                    else
                    {
                        string winMessage = ($"{nickname} отгадал число!");
                        Console.WriteLine(winMessage);
                        Console.WriteLine("Игра окончена");

                        byte[] response = Encoding.UTF8.GetBytes("Вы отгадали число");
                        await client.SendAsync(new ArraySegment<byte>(response), SocketFlags.None);

                        foreach (var otherClient in clients)
                        {
                            otherClient.Item1.Shutdown(SocketShutdown.Both);
                            otherClient.Item1.Close();
                        }
                        Environment.Exit(0);
                    }
                }
            }
        }
    }
}
