using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ClientDesktop
{
    public class PythonCodeManager : INotifyPropertyChanged
    {
        private ObservableCollection<string> _selectedFileNames = new ObservableCollection<string>();

        public ObservableCollection<string> SelectedFileNames
        {
            get { return _selectedFileNames; }
            set
            {
                if (_selectedFileNames != value)
                {
                    _selectedFileNames = value;
                    OnPropertyChanged(nameof(SelectedFileNames));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddSelectedFileName(string fileName)
        {
            _selectedFileNames.Add(fileName);
            OnPropertyChanged(nameof(SelectedFileNames));
        }

        public void ClearSelectedFileNames()
        {
            _selectedFileNames.Clear();
            OnPropertyChanged(nameof(SelectedFileNames));
        }

        public void SubmitPythonCode(string code)
        {
            // Optionally, you can perform any necessary validations on the code.

            // Notify the Server thread that Python code has been submitted.
            // In addition to submitting code, update the list of selected file names.
            AddSelectedFileName(code);

            JobStatus = "Processing";
            // Mock delay to simulate processing
            Task.Delay(2000).Wait();
            JobStatus = "Idle";
            CompletedJobs++;
        }

        private string _jobStatus = "Idle";
        public string JobStatus
        {
            get { return _jobStatus; }
            set
            {
                if (_jobStatus != value)
                {
                    _jobStatus = value;
                    OnPropertyChanged(nameof(JobStatus));
                }
            }
        }

        private int _completedJobs = 0;
        public int CompletedJobs
        {
            get { return _completedJobs; }
            set
            {
                if (_completedJobs != value)
                {
                    _completedJobs = value;
                    OnPropertyChanged(nameof(CompletedJobs));
                }
            }
        }

    }
}
