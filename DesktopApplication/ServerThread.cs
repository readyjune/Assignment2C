using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using WebAPI.Models;

namespace DesktopApplication
{
    public class ServerThread
    {
        private Thread thread;
        private List<Client> clientsList;
        private Action refreshClientList;

        public ServerThread(List<Client> clientsList, Action refreshClientList)
        {
            this.clientsList = clientsList;
            this.refreshClientList = refreshClientList;
            thread = new Thread(ServerThreadLogic);
        }

        public void Start()
        {
            thread.Start();
        }

        private void ServerThreadLogic()
        {
            while (true)
            {
                HandleClientRegistrations();
                Thread.Sleep(5000);
            }
        }

        private void HandleClientRegistrations()
        {
            try
            {
                // Placeholder: Implement your server logic related to client registrations
                Application.Current.Dispatcher.Invoke(refreshClientList);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("An error occurred: " + ex.Message));
            }
        }
    }
}
