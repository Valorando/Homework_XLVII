using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Homework_07_12_III
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Задание 2 - сервер

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 8888;

            Socket ls = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ls.Bind(new IPEndPoint(ip, port));
            ls.Listen(10);
            List<(Socket, string)> Clients = new List<(Socket, string)>();

            List<string> words = new List<string>
            {
                "Винница", "Луцк", "Днепр", "Донецк", "Житомир", "Ужгород",
                "Запорожье", "Ивано-Франковск", "Киев", "Кропивницкий",
                "Луганск", "Львов", "Николаев", "Одесса", "Полтава",
                "Ровно", "Сумы", "Тернополь", "Харьков", "Херсон",
                "Хмельницкий", "Черкассы", "Чернигов", "Черновцы", "Симферополь"
            };


            string selectedWord = words[new Random().Next(words.Count)]; 

            string hiddenWord = new string('*', selectedWord.Length); 

            Console.WriteLine("Игра началась!");
            Console.WriteLine($"Слово: {hiddenWord}");

            while (true)
            {
                Socket c = await ls.AcceptAsync();
                byte[] nicknameData = new byte[256];
                int nicknameBytes = await c.ReceiveAsync(new ArraySegment<byte>(nicknameData), SocketFlags.None);
                string nickname = Encoding.UTF8.GetString(nicknameData, 0, nicknameBytes);

                Clients.Add((c, nickname));
                Console.WriteLine($"{nickname} присоединился к игре.");

                byte[] initialWordResponse = Encoding.UTF8.GetBytes($"Слово: {hiddenWord}");
                await c.SendAsync(new ArraySegment<byte>(initialWordResponse), SocketFlags.None);

                _ = Task.Run(() => ProcessClient(c, nickname, Clients, selectedWord, hiddenWord));
            }
        }

        static async Task ProcessClient(Socket client, string nickname, List<(Socket, string)> clients, string selectedWord, string hiddenWord)
        {
            byte[] data = new byte[256];
            while (true)
            {
                int bytes = await client.ReceiveAsync(new ArraySegment<byte>(data), SocketFlags.None);
                string letter = Encoding.UTF8.GetString(data, 0, bytes);

                if (letter.Length == 1 && (char.IsLetter(letter[0]) || letter[0] == '-'))
                {
                    char guessedLetter = letter[0];

                    StringBuilder updatedWord = new StringBuilder(hiddenWord);
                    bool letterGuessed = false;

                    for (int i = 0; i < selectedWord.Length; i++)
                    {
                        if (char.ToLower(selectedWord[i]) == char.ToLower(guessedLetter) || selectedWord[i] == '-')
                        {
                            updatedWord[i] = guessedLetter;
                            letterGuessed = true;
                        }
                    }

                    hiddenWord = updatedWord.ToString();

                    byte[] response = Encoding.UTF8.GetBytes($"Слово: {hiddenWord}");
                    await client.SendAsync(new ArraySegment<byte>(response), SocketFlags.None);
                    if (!hiddenWord.Contains('*'))
                    {
                        Console.WriteLine($"Игрок {nickname} отгадал слово: {selectedWord}");
                        Console.WriteLine("Игра завершена!");

                        response = Encoding.UTF8.GetBytes($"Игрок {nickname} отгадал слово: {selectedWord}");
                        foreach (var otherClient in clients)
                        {
                            await otherClient.Item1.SendAsync(new ArraySegment<byte>(response), SocketFlags.None);
                        }

                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                        Environment.Exit(0);
                    }
                }
            }
        }






    }
}
