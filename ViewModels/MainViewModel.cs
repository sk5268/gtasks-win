using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Google_Tasks_Client.Models;
using Google_Tasks_Client.Services;
using Google_Tasks_Client.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

namespace Google_Tasks_Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITaskService _taskService;
        private TaskListItem? _selectedTaskList;
        private string _newTaskTitle = string.Empty;
        private string _newTaskListTitle = string.Empty;
        private readonly DispatcherQueue _dispatcherQueue;

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

        private readonly DispatcherTimer _pollTimer;

        public MainViewModel()
        {
            _taskService = new TaskRepository();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            
            AddTaskCommand = new RelayCommand(async _ => await AddTaskAsync(), _ => !string.IsNullOrWhiteSpace(NewTaskTitle));
            DeleteTaskCommand = new RelayCommand(async param => await DeleteTaskAsync(param as TaskItem));
            ToggleTaskStatusCommand = new RelayCommand(async param => await ToggleTaskStatusAsync(param as TaskItem));
            
            AddTaskListCommand = new RelayCommand(async _ => await AddTaskListAsync(), _ => !string.IsNullOrWhiteSpace(NewTaskListTitle));
            DeleteTaskListCommand = new RelayCommand(async param => await DeleteTaskListAsync(param as TaskListItem));

            // Initial immediate load and background sync
            _ = InitialLoadAsync();

            // Setup polling timer (10 seconds)
            _pollTimer = new DispatcherTimer();
            _pollTimer.Interval = System.TimeSpan.FromSeconds(10);
            _pollTimer.Tick += async (s, e) => await RefreshActiveListAsync();
            _pollTimer.Start();
        }

        private async Task InitialLoadAsync()
        {
            // 1. Load from DB (into Repository memory)
            await _taskService.InitializeAsync();
            
            // 2. Refresh UI from local cache
            await LoadTaskListsUIAsync();

            // 3. One-time boot sync from API (Everything)
            await Task.Run(async () => {
                await _taskService.SyncAllAsync();
                
                // 4. Update UI as soon as response arrives
                _dispatcherQueue.TryEnqueue(async () => {
                    await LoadTaskListsUIAsync();
                    if (SelectedTaskList != null)
                    {
                        await LoadTasksUIAsync(SelectedTaskList.Id);
                    }
                });
            });
        }

        private async Task RefreshActiveListAsync()
        {
            if (SelectedTaskList == null) return;
            
            try
            {
                await _taskService.SyncTasksAsync(SelectedTaskList.Id);
                var tasks = await _taskService.GetTasksAsync(SelectedTaskList.Id);
                SyncTasksCollection(tasks);
            }
            catch { }
        }

        private void SyncTaskListsCollection(List<TaskListItem> newList)
        {
            for (int i = TaskLists.Count - 1; i >= 0; i--)
            {
                if (!newList.Any(l => l.Id == TaskLists[i].Id)) TaskLists.RemoveAt(i);
            }

            foreach (var item in newList)
            {
                var existing = TaskLists.FirstOrDefault(l => l.Id == item.Id);
                if (existing == null) TaskLists.Add(item);
                else existing.Title = item.Title;
            }

            if (SelectedTaskList == null && TaskLists.Count > 0)
            {
                SelectedTaskList = TaskLists[0];
            }
        }

        private void SyncTasksCollection(List<TaskItem> newList)
        {
            for (int i = Tasks.Count - 1; i >= 0; i--)
            {
                if (!newList.Any(t => t.Id == Tasks[i].Id)) Tasks.RemoveAt(i);
            }

            foreach (var item in newList)
            {
                var existing = Tasks.FirstOrDefault(t => t.Id == item.Id);
                if (existing == null) Tasks.Add(item);
                else
                {
                    existing.Title = item.Title;
                    existing.Notes = item.Notes;
                    existing.Status = item.Status;
                    existing.Due = item.Due;
                }
            }
        }

        private async Task AddTaskListAsync()
        {
            if (string.IsNullOrWhiteSpace(NewTaskListTitle)) return;
            var title = NewTaskListTitle;
            NewTaskListTitle = string.Empty;
            var createdList = await _taskService.AddTaskListAsync(title);
            TaskLists.Add(createdList);
            SelectedTaskList = createdList;
        }

        private async Task DeleteTaskListAsync(TaskListItem? list)
        {
            if (list == null) return;
            await _taskService.DeleteTaskListAsync(list.Id);
            TaskLists.Remove(list);
            if (SelectedTaskList == list)
            {
                SelectedTaskList = TaskLists.Count > 0 ? TaskLists[0] : null;
            }
        }

        private async Task AddTaskAsync()
        {
            if (SelectedTaskList == null || string.IsNullOrWhiteSpace(NewTaskTitle)) return;
            var titleInput = NewTaskTitle;
            NewTaskTitle = string.Empty;

            var (title, due) = ReminderParser.Parse(titleInput);
            var newTask = new TaskItem { Title = title, Due = due };
            
            // UI already optimistic because we add it here
            Tasks.Insert(0, newTask);
            await _taskService.AddTaskAsync(SelectedTaskList.Id, newTask);
        }

        private async Task DeleteTaskAsync(TaskItem? task)
        {
            if (task == null || SelectedTaskList == null) return;
            await _taskService.DeleteTaskAsync(SelectedTaskList.Id, task.Id);
            Tasks.Remove(task);
        }

        private async Task ToggleTaskStatusAsync(TaskItem? task)
        {
            if (task == null || SelectedTaskList == null) return;
            await _taskService.UpdateTaskAsync(SelectedTaskList.Id, task);
        }

        public async Task LoadTaskListsUIAsync()
        {
            var lists = await _taskService.GetTaskListsAsync();
            SyncTaskListsCollection(lists);
        }

        public async Task LoadTasksUIAsync(string listId)
        {
            var tasks = await _taskService.GetTasksAsync(listId);
            SyncTasksCollection(tasks);
        }

        public async Task LoadTasksAsync(string listId)
        {
            // 1. Show cached tasks immediately
            await LoadTasksUIAsync(listId);

            // 2. Fetch changes for THIS list in background
            _ = Task.Run(async () => {
                try {
                    await _taskService.SyncTasksAsync(listId);
                    var updated = await _taskService.GetTasksAsync(listId);
                    _dispatcherQueue.TryEnqueue(() => SyncTasksCollection(updated));
                } catch { }
            });
        }

        public async Task ShutdownAsync()
        {
            await _taskService.PersistAsync();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
