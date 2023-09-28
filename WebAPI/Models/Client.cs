using System.ComponentModel;

namespace WebAPI.Models
{
    public class Client : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string? IPAddress { get; set; }
        public int Port { get; set; }

        private int _jobsCompleted;
        public int JobsCompleted
        {
            get { return _jobsCompleted; }
            set
            {
                _jobsCompleted = value;
                OnPropertyChanged("JobsCompleted");
            }
        }




        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy != value)
                {
                    _isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
