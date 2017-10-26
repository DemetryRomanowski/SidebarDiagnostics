using System;
using System.ComponentModel;

namespace SidebarDiagnostics.Models
{
    public class UpdateModel : INotifyPropertyChanged
    {
        public void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Double _progress { get; set; } = 0d;

        public Double Progress
        {
            get => _progress;
            set
            {
                _progress = value;

                NotifyPropertyChanged("Progress");
                NotifyPropertyChanged("ProgressNormalized");
            }
        }

        public Double ProgressNormalized => _progress / 100d;
    }
}
