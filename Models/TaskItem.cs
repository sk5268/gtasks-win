using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Google_Tasks_Client.Models
{
    public class TaskItem : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _title = string.Empty;
        private string _notes = string.Empty;
        private string _status = "needsAction";
        private DateTime? _due;
        private string _parentId = string.Empty;
        private ObservableCollection<TaskItem> _subtasks = new();

        public string Id 
        { 
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }
        
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

        public string Notes 
        { 
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public DateTime? Due 
        { 
            get => _due;
            set
            {
                if (_due != value)
                {
                    _due = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FormattedDue));
                }
            }
        }

        public string FormattedDue => Due?.ToShortDateString() ?? string.Empty;

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
        
        public bool IsCompleted
        {
            get => Status == "completed";
            set => Status = value ? "completed" : "needsAction";
        }

        public string ParentId
        {
            get => _parentId;
            set
            {
                if (_parentId != value)
                {
                    _parentId = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<TaskItem> Subtasks
        {
            get => _subtasks;
            set
            {
                if (_subtasks != value)
                {
                    _subtasks = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
