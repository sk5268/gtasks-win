using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Google_Tasks_Client.Models;
using Google_Tasks_Client.Services;
using Microsoft.UI.Xaml;

namespace Google_Tasks_Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITaskService _taskService;
        private TaskListItem? _selectedTaskList;
        private bool _isLoading;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TaskListItem> TaskLists { get; } = new();
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public TaskListItem? SelectedTaskList
        {
            get => _selectedTaskList;
            set
            {
                if (_selectedTaskList != value)
                {
                    _selectedTaskList = value;
                    OnPropertyChanged();
                    if (_selectedTaskList != null)
                    {
                        _ = LoadTasksAsync(_selectedTaskList.Id);
                    }
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsLoadingVisibility));
                }
            }
        }

        public Visibility IsLoadingVisibility => IsLoading ? Visibility.Visible : Visibility.Collapsed;

        public MainViewModel()
        {
            // Now using the real Google API service
            _taskService = new GoogleTaskService();
            _ = LoadTaskListsAsync();
        }

        public async Task LoadTaskListsAsync()
        {
            IsLoading = true;
            try
            {
                var lists = await _taskService.GetTaskListsAsync();
                TaskLists.Clear();
                foreach (var list in lists)
                {
                    TaskLists.Add(list);
                }

                // Select first list by default
                if (TaskLists.Count > 0)
                {
                    SelectedTaskList = TaskLists[0];
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task LoadTasksAsync(string listId)
        {
            IsLoading = true;
            try
            {
                var tasks = await _taskService.GetTasksAsync(listId);
                Tasks.Clear();
                foreach (var task in tasks)
                {
                    Tasks.Add(task);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
