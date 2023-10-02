using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for OpenDialog.xaml
    /// </summary>
    public partial class OpenDialog : Window
    {
        public string IPAddress { get; private set; }
        public int Port { get; private set; }

        public OpenDialog()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidIPAddress(IPInput.Text) && IsValidPort(PortInput.Text) && CanConnectToServer(IPInput.Text, int.Parse(PortInput.Text)))
            {
                IPAddress = IPInput.Text;
                Port = int.Parse(PortInput.Text);
                this.DialogResult = true;
                this.Close();
            }
        }

        private bool IsValidIPAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress) || !System.Net.IPAddress.TryParse(ipAddress, out _))
            {
                MessageBox.Show("Please enter a valid IP address.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool IsValidPort(string port)
        {
            if (string.IsNullOrWhiteSpace(port) || !int.TryParse(port, out int parsedPort) || parsedPort <= 0 || parsedPort > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool CanConnectToServer(string ipAddress, int port)
        {
            using (TcpClient client = new TcpClient())
            {
                try
                {
                    client.Connect(ipAddress, port);
                    return true; // Connection successful
                }
                catch
                {
                    MessageBox.Show($"Unable to connect to the server at {ipAddress}:{port}. Please ensure the server is running and the IP and port are correct.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false; // Connection failed
                }
            }
        }
    }
}
