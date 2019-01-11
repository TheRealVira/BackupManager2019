using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BackupManager.Annotations;

namespace BackupManager
{
    public class MyManager:INotifyPropertyChanged
    {
        public MyManager(string selectedDirectories)
        {
            SelectedDirectories = selectedDirectories;
        }

        private string _outputDirectory;
        public string OutputDirectory
        {
            get => _outputDirectory;
            set
            {
                if (!value.Equals(_outputDirectory))
                {
                    _outputDirectory = value;
                    OnPropertyChanged(nameof(OutputDirectory));
                }
            }
        }

        private string _selectedDirectories;
        public string SelectedDirectories
        {
            get => _selectedDirectories;
            set
            {
                if (value.Equals(_selectedDirectories)) return;

                _selectedDirectories = value;
                OnPropertyChanged(nameof(SelectedDirectories));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
