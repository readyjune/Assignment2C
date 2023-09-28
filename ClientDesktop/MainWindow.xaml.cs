using System;
using System.Collections.Generic;
using System.Linq;
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
using WebAPI.Models;
using System.IO;


namespace ClientDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        private PythonCodeManager pythonCodeManager; // Create an instance of PythonCodeManager
        private ServerThreadManager serverThreadManager;
        private NetworkingThreadManager networkingThreadManager;
        public MainWindow()
        {
            InitializeComponent();
            // Create an instance of PythonCodeManager.
            pythonCodeManager = new PythonCodeManager();
            // Set the DataContext to this instance.
            DataContext = pythonCodeManager;
            // Bind the SelectedFileList.ItemsSource to the SelectedFileNames ObservableCollection
            SelectedFileList.ItemsSource = pythonCodeManager.SelectedFileNames;

            serverThreadManager = new ServerThreadManager();
            networkingThreadManager = new NetworkingThreadManager();

            StartThreads();

        }
        private void StartThreads()
        {
            serverThreadManager.Start();
            networkingThreadManager.Start();
        }
        private void BrowsePythonFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Python Files (*.py)|*.py|All Files (*.*)|*.*"; // Filter for Python files
            openFileDialog.Title = "Select a Python File";

            if (openFileDialog.ShowDialog() == true)
            {
                // Get the selected file path
                string selectedFilePath = openFileDialog.FileName;

                // Get the selected file name (without path)
                string selectedFileName = System.IO.Path.GetFileName(selectedFilePath);

                // Read the content of the selected Python file
                string pythonCode = System.IO.File.ReadAllText(selectedFilePath);

                // Set the content in the TextBox
                PythonCodeTextBox.Text = selectedFileName;

            }
        }


        private void SubmitCodeButton_Click(object sender, RoutedEventArgs e)
        {
           // string pythonCode = PythonCodeTextBox.Text; // Get the Python code from the TextBox'
            string selectedFileName = PythonCodeTextBox.Text;
            Job newJob = new Job
            {
                //PythonCode = pythonCode,
                FileName = selectedFileName
                // Set other properties as needed.
            };
            JobProgressBar.IsIndeterminate = true;

            // Submit the job to the Server thread
            pythonCodeManager.SubmitPythonCode(newJob.FileName);

            JobProgressBar.IsIndeterminate = false;
            JobStatusTextBlock.Text = pythonCodeManager.JobStatus;
            JobsCompletedTextBlock.Text = $"Jobs Completed: {pythonCodeManager.CompletedJobs}";

            // To clear the TextBox or perform other actions.
            PythonCodeTextBox.Clear();


        }

        private void QueryNetworkingThreadButton_Click(object sender, RoutedEventArgs e)
        {
            // This is just a mock. In a real-world scenario, fetch the status from NetworkingThreadManager.
            NetworkingStatusTextBlock.Text = "Networking Status: Active";
        }

        private void StartApplicationButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
