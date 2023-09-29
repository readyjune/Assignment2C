using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int port = 12345;  // You can choose any available port
            TcpListener server = null;

            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");  // Listen on localhost
                server = new TcpListener(localAddr, port);
                server.Start();

                Console.WriteLine("Server started...");

                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    NetworkStream stream = client.GetStream();
                    StreamReader reader = new StreamReader(stream);
                    StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

                    string message = reader.ReadLine();

                    switch (message)
                    {
                        case "Hello":
                            writer.WriteLine("Hello Client!");
                            break;

                        case "Upload":
                            string fileName = reader.ReadLine();
                            string fileContent = reader.ReadLine();
                            File.WriteAllText($"./{fileName}", fileContent);
                            writer.WriteLine("File uploaded successfully!");

                            // Print the uploaded filename and its content
                            Console.WriteLine($"Received file: {fileName}");
                            Console.WriteLine("File Content:");
                            Console.WriteLine("--------------");
                            Console.WriteLine(fileContent);
                            Console.WriteLine("--------------");
                            break;

                        case "Goodbye":
                            writer.WriteLine("Goodbye Client!");
                            break;

                        default:
                            writer.WriteLine("Unknown command!");
                            break;
                    }

                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"SocketException: {e}");
            }
            finally
            {
                server?.Stop();
            }
        }
    }
}
