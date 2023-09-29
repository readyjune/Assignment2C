using ClientServerSide.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// So if you do run without debug for ClientServerSide, the port number will be 12345;
// To run different instance of ClientServerSide to pretend different computer,
// You have to change the port number 
// How -> Go to the Properties -> Debug Tab -> type 12346 in the Command Line Argument part.
// After that you can run the different instance of ClientServerSide with port 12346;

// In the Client OpenDialog xaml page, you need to type 127.0.0.1 and 12345 or 12346 whatever you run on server.



namespace ClientServer
{
    internal class Program
    {
        private static TcpListener _listener;
        private static IPAddress _ipAddress; // Add a field to store the IP address
        private static int _port; // Add a field to store the port number

        static void Main(string[] args)
        {
            if (args.Length != 2 || !IPAddress.TryParse(args[0], out _ipAddress) || !int.TryParse(args[1], out _port))
            {
                Console.WriteLine("Usage: ClientServerSide <IP Address> <Port>");
                return;
            }

            StartServer();
        }

        private static void StartServer()
        {
            _listener = new TcpListener(_ipAddress, _port);
            _listener.Start();

            Console.WriteLine($"Server started on IP {_ipAddress} and port {_port}. Waiting for client connections...");

            while (true)
            {
                var client = _listener.AcceptTcpClient();
                HandleClient(client);
            }
        }

        private static async Task HandleClient(TcpClient client)
        {
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };  // Add a StreamWriter for sending responses

            var messageType = reader.ReadLine();
            if (messageType == "Connection")
            {
                var ip = reader.ReadLine();
                var port = reader.ReadLine();
                Console.WriteLine($"IP {ip} and port {port} is logged in.");

                // Display the list of all clients
                //await DisplayAllClients(); //Issue code

                // Send a response back to the client
                writer.WriteLine("Connection successful");
            }
            else if (messageType == "Upload")
            {
                var filename = reader.ReadLine();
                var code = reader.ReadLine();
                Console.WriteLine($"Received Python code from file: {filename}");
                Console.WriteLine("----- Python Code -----");
                Console.WriteLine(code);
                Console.WriteLine("----- End of Python Code -----");

                // Send a response back to the client
                writer.WriteLine("Upload successful");
            }

            client.Close();
        }


        private static async Task<List<Client>> FetchAllClientsFromAPI()
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://localhost:{_port}/api/Clients");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Client>>(content);
            }
            return new List<Client>();
        }
        private static async Task DisplayAllClients()
        {
            var clients = await FetchAllClientsFromAPI();
            Console.WriteLine("All Clients:");
            foreach (var client in clients)
            {
                Console.WriteLine($"IP: {client.IPAddress}, Port: {client.Port}");
            }
        }


    }
}

