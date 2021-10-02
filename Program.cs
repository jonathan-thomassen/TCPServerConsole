using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using OblStudieakt1Program3Sem;

namespace TCPServerConsole
{
    class Program
    {
        public static List<FootballPlayer> players = new List<FootballPlayer>();

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, 2121);
            listener.Start();
            Console.WriteLine("Server started.");

            while (true)
            {
                TcpClient socket = listener.AcceptTcpClient();
                Console.WriteLine("New client: " + socket.Client.RemoteEndPoint);

                Task.Run(() => { NewClient(socket); });
            }
        }

        static void NewClient(TcpClient socket)
        {
            NetworkStream ns = socket.GetStream();
            StreamReader reader = new StreamReader(ns);
            StreamWriter writer = new StreamWriter(ns);

            bool quit = false;
            while (!quit)
            {
                writer.WriteLine("Football Player server. Choose function:\nEnter \"HentAlle\" and a blank line for a list of all players.\nEnter \"Hent\" and \"[ID]\" for information about a specific player.\nEnter \"Gem\" and \"[JSON-serialized football-player]\" to enter a new player into the database.");
                writer.Flush();

                string line1 = reader.ReadLine();
                string line2 = reader.ReadLine();
                if (line1.ToLower().StartsWith("hentalle"))
                {
                    if (players.Count > 0)
                    {
                        writer.WriteLine(JsonSerializer.Serialize(players));
                    }
                    else
                    {
                        writer.WriteLine("List of players is empty. Please add players.");
                    }
                }
                else if (line1.ToLower().StartsWith("hent"))
                {
                    if (int.TryParse(line2, out int id))
                    {
                        FootballPlayer player = players.Find(p => p.Id == id);
                        if (player != null)
                        {
                            writer.WriteLine(JsonSerializer.Serialize(player));
                        }
                        else
                        {
                            writer.WriteLine("Player does not exist in database. Please try another id.");
                        }
                    }
                    else
                    {
                        writer.WriteLine("Not a valid id number. Please enter an integer.");
                    }
                }
                else if (line1.ToLower().StartsWith("gem"))
                {
                    try
                    {
                        FootballPlayer player = JsonSerializer.Deserialize<FootballPlayer>(line2);
                        players.Add(player);
                        writer.WriteLine("Player successfully added to database.");
                    }
                    catch
                    {
                        writer.WriteLine("Error encountered, player not added to database. Please check JSON and try again.");

                    }
                }
                else
                {
                    writer.WriteLine("Command not recognized. Please try again.");
                }
                writer.Flush();

                writer.WriteLine("Send blank line to continue. Send \"Q\" to quit:");
                writer.Flush();

                string line3 = reader.ReadLine();
                if (line3.ToLower().StartsWith("q"))
                {
                    quit = true;
                }
            }

            socket.Close();
        }
    }
}
