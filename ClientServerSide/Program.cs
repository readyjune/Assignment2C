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
        private static TcpListener _listener;
        private const int Port = 12345;

        static void Main(string[] args)
        {
            StartServer();
        }

        private static void StartServer()
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();

            Console.WriteLine($"Server started on port {Port}. Waiting for client connections...");

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                HandleClient(client);
            }
        }

        private static void HandleClient(TcpClient client)
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream);

            var messageType = reader.ReadLine();
            if (messageType == "Connection")
            {
                var ip = reader.ReadLine();
                var port = reader.ReadLine();
                Console.WriteLine($"IP {ip} and port {port} is logged in.");
            }
            else if (messageType == "Upload")
            {
                var filename = reader.ReadLine();
                var code = reader.ReadLine();
                Console.WriteLine($"Received Python code from file: {filename}");
                Console.WriteLine("----- Python Code -----");
                Console.WriteLine(code);
                Console.WriteLine("----- End of Python Code -----");
            }

            client.Close();
        }
    }
}

