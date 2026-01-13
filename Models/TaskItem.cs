using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Google_Tasks_Client.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _status = "needsAction";

        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime? Due { get; set; }

        public string Status 
        { 
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }
        
        // Helper for Binding
        public bool IsCompleted 
        { 
            get => Status == "completed";
            set => Status = value ? "completed" : "needsAction";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}