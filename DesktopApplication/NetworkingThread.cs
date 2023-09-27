using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using WebAPI.Models;

namespace DesktopApplication
{
    public class NetworkingThread
    {
        private Thread thread;
        private List<Client> clientsList;
        private Action refreshClientList;
        private CancellationTokenSource cancellationTokenSource;

        public bool IsWorking { get; private set; } = false; // To indicate if the networking thread is currently working

        public NetworkingThread(List<Client> clientsList, Action refreshClientList)
        {
            this.clientsList = clientsList;
            this.refreshClientList = refreshClientList;
            cancellationTokenSource = new CancellationTokenSource();
            thread = new Thread(NetworkingThreadLogic);
        }

        public void Start()
        {
            // Reset the CancellationTokenSource if it's already been cancelled
            if (cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Dispose();
                cancellationTokenSource = new CancellationTokenSource();
            }

            // If the thread is null or has previously been stopped, create a new one
            if (thread == null || thread.ThreadState == ThreadState.Stopped)
            {
                thread = new Thread(NetworkingThreadLogic);
            }

            thread.Start();
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            thread.Join(); // Wait for the thread to finish
        }

        private void NetworkingThreadLogic()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                CheckAndProcessJobs();
                Thread.Sleep(5000);
            }
        }

        private void CheckAndProcessJobs()
        {
            try
            {
                foreach (var client in clientsList)
                {
                    if (!client.IsBusy)
                    {
                        // Placeholder: Implement logic to check for jobs, download, and process them
                        client.IsBusy = true;
                        IsWorking = true;

                        // Placeholder: Process the job and when done:
                        client.IsBusy = false;
                        MainWindow.IncrementJobsCompleted();

                        IsWorking = false;
                        Application.Current.Dispatcher.Invoke(refreshClientList);
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => MessageBox.Show("An error occurred: " + ex.Message));
            }
        }
    }
}
