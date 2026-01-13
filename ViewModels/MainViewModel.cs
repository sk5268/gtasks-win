using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Google_Tasks_Client.Models;
using Google_Tasks_Client.Services;
using Google_Tasks_Client.Helpers;
using Microsoft.UI.Xaml;
using System.Windows.Input;

namespace Google_Tasks_Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITaskService _taskService;
        private TaskListItem? _selectedTaskList;
        private bool _isLoading;
        private string _newTaskTitle = string.Empty;
        private string _newTaskListTitle = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TaskListItem> TaskLists { get; } = new();
        public ObservableCollection<TaskItem> Tasks { get; } = new();

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleTaskStatusCommand { get; }
        public ICommand AddTaskListCommand { get; }
        public ICommand DeleteTaskListCommand { get; }

        public string NewTaskTitle
        {
            get => _newTaskTitle;
            set
            {
                if (_newTaskTitle != value)
                {
                    _newTaskTitle = value;
                    OnPropertyChanged();
                    ((RelayCommand)AddTaskCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string NewTaskListTitle
        {
            get => _newTaskListTitle;
            set
            {
                if (_newTaskListTitle != value)
                {
                    _newTaskListTitle = value;
                    OnPropertyChanged();
                    ((RelayCommand)AddTaskListCommand).RaiseCanExecuteChanged();
                }
            }
        }

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
                    else
                    {
                        Tasks.Clear();
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

        private readonly DispatcherTimer _pollTimer;

        public MainViewModel()
        {
            // Now using the real Google API service
            _taskService = new GoogleTaskService();
            
            AddTaskCommand = new RelayCommand(async _ => await AddTaskAsync(), _ => !string.IsNullOrWhiteSpace(NewTaskTitle));
            DeleteTaskCommand = new RelayCommand(async param => await DeleteTaskAsync(param as TaskItem));
            ToggleTaskStatusCommand = new RelayCommand(async param => await ToggleTaskStatusAsync(param as TaskItem));
            
            AddTaskListCommand = new RelayCommand(async _ => await AddTaskListAsync(), _ => !string.IsNullOrWhiteSpace(NewTaskListTitle));
            DeleteTaskListCommand = new RelayCommand(async param => await DeleteTaskListAsync(param as TaskListItem));

            _ = LoadTaskListsAsync();

            // Setup polling timer (10 seconds)
            _pollTimer = new DispatcherTimer();
            _pollTimer.Interval = System.TimeSpan.FromSeconds(10);
            _pollTimer.Tick += async (s, e) => await RefreshAsync();
            _pollTimer.Start();
        }

        private async Task RefreshAsync()
        {
            if (IsLoading) return;

            // Silently refresh in the background
            try
            {
                var lists = await _taskService.GetTaskListsAsync();
                
                // Update TaskLists collection without clearing everything to avoid UI flicker
                // Simple sync: if count or IDs differ, just reload.
                if (lists.Count != TaskLists.Count)
                {
                    TaskLists.Clear();
                    foreach (var list in lists) TaskLists.Add(list);
                }

                if (SelectedTaskList != null)
                {
                    var tasks = await _taskService.GetTasksAsync(SelectedTaskList.Id);
                    // Simple sync for tasks
                    if (tasks.Count != Tasks.Count)
                    {
                        Tasks.Clear();
                        foreach (var t in tasks) Tasks.Add(t);
                    }
                }
            }
            catch
            {
                // Ignore background refresh errors to avoid interrupting the user
            }
        }

        private async Task AddTaskListAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskListTitle)) return;

            IsLoading = true;
            try
            {
                var createdList = await _taskService.AddTaskListAsync(NewTaskListTitle);
                TaskLists.Add(createdList);
                SelectedTaskList = createdList;
                NewTaskListTitle = string.Empty;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteTaskListAsync(TaskListItem? list)
        {
            if (list == null) return;

            IsLoading = true;
            try
            {
                await _taskService.DeleteTaskListAsync(list.Id);
                TaskLists.Remove(list);
                if (SelectedTaskList == list)
                {
                    SelectedTaskList = TaskLists.Count > 0 ? TaskLists[0] : null;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task AddTaskAsync()
        {
            if (SelectedTaskList == null || string.IsNullOrWhiteSpace(NewTaskTitle)) return;

            IsLoading = true;
            try
            {
                var newTask = new TaskItem { Title = NewTaskTitle };
                var createdTask = await _taskService.AddTaskAsync(SelectedTaskList.Id, newTask);
                Tasks.Insert(0, createdTask);
                NewTaskTitle = string.Empty;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteTaskAsync(TaskItem? task)
        {
            if (SelectedTaskList == null || task == null) return;

            IsLoading = true;
            try
            {
                await _taskService.DeleteTaskAsync(SelectedTaskList.Id, task.Id);
                Tasks.Remove(task);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ToggleTaskStatusAsync(TaskItem? task)
        {
            if (SelectedTaskList == null || task == null) return;

            // Note: The Property Setter already updated the local Status via UI binding (if TwoWay)
            // But we should ensure consistency or handling if binding didn't update it yet.
            // With Command on CheckBox, the Command fires after the Click, and the Binding usually updates.
            // However, to be safe, let's assume the UI state 'IsCompleted' is the desired state.
            
            // Wait, if we rely on TwoWay binding, the 'Status' is already updated in the object.
            // We just need to send the update to the server.
            
            IsLoading = true;
            try
            {
               var updatedTask = await _taskService.UpdateTaskAsync(SelectedTaskList.Id, task);
               // Optional: Update local object with server response if needed (e.g. ETag, updated timestamp)
            }
            finally
            {
                IsLoading = false;
            }
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
