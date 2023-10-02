using ClientServer.Services;
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
using WebAPI.Models;
using Client = ClientServerSide.Models.Client;

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
        private static readonly ClientApiService _clientApiService = new ClientApiService("http://localhost:5074/api");
        static void Main(string[] args)
        {
            // Attach the ProcessExit event handler
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            if (args.Length != 2 || !IPAddress.TryParse(args[0], out _ipAddress) || !int.TryParse(args[1], out _port))
            {
                Console.WriteLine("Usage: ClientServerSide <IP Address> <Port>");
                return;
            }

            StartServer();
        }

        private static async void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            var clientApiService = new ClientApiService("http://localhost:5074/api");
            await clientApiService.DeleteClientAsync(_ipAddress.ToString(), _port);
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
            var writer = new StreamWriter(stream) { AutoFlush = true };

            var messageType = reader.ReadLine();
            if (messageType == "Connection")
            {
                var ip = reader.ReadLine();
                var port = int.Parse(reader.ReadLine());
                Console.WriteLine($"IP {ip} and port {port} log in successfully.");

                // Register the client in the database using the API
                await _clientApiService.RegisterClientAsync(ip, port);

                writer.WriteLine("Connection successful");
            }
            else if (messageType == "Upload")
            {
                var filename = reader.ReadLine();
                var code = reader.ReadLine();

                // Get the client's IP and use the server's listening port
                var clientIP = _ipAddress.ToString();

                // Update the client's record with the uploaded Python code via the API
                await _clientApiService.UpdateClientPythonCode(clientIP, _port, code);

                Console.WriteLine($"Received Python code from file: {filename}");
                Console.WriteLine("----- Python Code -----");
                Console.WriteLine(code);
                Console.WriteLine("----- End of Python Code -----");

                writer.WriteLine("Upload successful");
            }
            else if (messageType == "HelpRequest")
            {
                // Here, you'd probably get the Python code from the database/API, run it, 
                // and return the results. This is a placeholder for future implementation.
                var helperIp = reader.ReadLine();
                var helperPort = reader.ReadLine();

                Console.WriteLine($"Received a help request from IP: {helperIp} and Port: {helperPort}.");

                // For now, just acknowledge the help request
                writer.WriteLine("Yes, I need help!");
            }
            else if (messageType == "Output")
            {
                var clientIP = _ipAddress.ToString();
                
                var output = reader.ReadLine();

                // Store the output in the client's record
                await _clientApiService.UpdateClientOutputAsync(clientIP, _port, output);

                Console.WriteLine($"Received output from IP: {clientIP} and Port: {_port}.");
            }


            client.Close();
        }


    }
}

