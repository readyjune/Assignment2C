using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string uploadedPythonFilePath;
        private string uploadedPythonFileName;
        private string providedIPAddress;
        private int providedPort;

        public MainWindow()
        {
            InitializeComponent();

            OpenDialog openDialog = new OpenDialog();

            if (openDialog.ShowDialog() == true)
            {
                providedIPAddress = openDialog.IPAddress;
                providedPort = openDialog.Port;
            }
            else
            {
                // Close the MainWindow if the user cancels the OpenDialog or doesn't provide valid input
                this.Close();
            }
        }
        private void BrowsePythonCodeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Python Files|*.py|All Files|*.*";
            openFileDialog.DefaultExt = ".py";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    uploadedPythonFilePath = openFileDialog.FileName;  // Save the file path
                    uploadedPythonFileName = System.IO.Path.GetFileName(uploadedPythonFilePath);
                    var pythonCodeContent = File.ReadAllText(uploadedPythonFilePath);
                    PythonCodeInput.Text = pythonCodeContent;  // Set the TextBox to the actual Python code
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 12345))  // Connect to the server
                {
                    NetworkStream stream = client.GetStream();
                    StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
                    StreamReader reader = new StreamReader(stream);

                    // Start the communication
                    writer.WriteLine("Upload");
                    writer.WriteLine(uploadedPythonFileName);
                    writer.WriteLine(PythonCodeInput.Text);

                    // Read the response from the server
                    string response = reader.ReadLine();
                    MessageBox.Show(response);  // Show the server's response
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while uploading the file: " + ex.Message);
            }
        }
    }
}
