using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Google_Tasks_Client.Models
{
    public class TaskListItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;

        public string Id { get; set; } = string.Empty;
        
        public string Title 
        { 
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? Updated { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}