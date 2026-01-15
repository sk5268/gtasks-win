using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google_Tasks_Client.Models;
using System.Linq;

namespace Google_Tasks_Client.Services
{
    public class TaskRepository : ITaskService
    {
        private readonly GoogleTaskService _remoteService;
        private readonly LocalTaskService _localService;
        
        // In-memory cache
        private List<TaskListItem> _taskLists = new();
        private Dictionary<string, List<TaskItem>> _tasksByList = new();

        public TaskRepository()
        {
            _remoteService = new GoogleTaskService();
            _localService = new LocalTaskService();
        }

        public async Task InitializeAsync()
        {
            // Load from local DB into memory
            _taskLists = await _localService.GetTaskListsAsync();
            foreach (var list in _taskLists)
            {
                var tasks = await _localService.GetTasksAsync(list.Id);
                _tasksByList[list.Id] = tasks;
            }
        }

        public async Task PersistAsync()
        {
            // Write all in-memory data to local DB
            await _localService.SaveTaskListsAsync(_taskLists);
            foreach (var kvp in _tasksByList)
            {
                await _localService.SaveTasksAsync(kvp.Key, kvp.Value);
            }
        }

        public Task<List<TaskListItem>> GetTaskListsAsync()
        {
            return Task.FromResult(_taskLists.ToList());
        }

        public Task<List<TaskItem>> GetTasksAsync(string taskListId)
        {
            if (_tasksByList.TryGetValue(taskListId, out var tasks))
            {
                return Task.FromResult(tasks.ToList());
            }
            return Task.FromResult(new List<TaskItem>());
        }

        public async Task<TaskListItem> AddTaskListAsync(string title)
        {
            var tempId = "temp_" + Guid.NewGuid().ToString();
            var newList = new TaskListItem { Id = tempId, Title = title };
            _taskLists.Add(newList);
            _tasksByList[tempId] = new List<TaskItem>();

            _ = Task.Run(async () => {
                try {
                    var remote = await _remoteService.AddTaskListAsync(title);
                    // Update memory
                    var local = _taskLists.FirstOrDefault(l => l.Id == tempId);
                    if (local != null) {
                        local.Id = remote.Id;
                        var tasks = _tasksByList[tempId];
                        _tasksByList.Remove(tempId);
                        _tasksByList[remote.Id] = tasks;
                    }
                } catch { }
            });

            return newList;
        }

        public async Task DeleteTaskListAsync(string taskListId)
        {
            var list = _taskLists.FirstOrDefault(l => l.Id == taskListId);
            if (list != null) _taskLists.Remove(list);
            _tasksByList.Remove(taskListId);

            _ = Task.Run(async () => {
                try {
                    await _remoteService.DeleteTaskListAsync(taskListId);
                } catch { }
            });
        }

        public async Task<TaskItem> AddTaskAsync(string taskListId, TaskItem task)
        {
            var tempId = "temp_" + Guid.NewGuid().ToString();
            var originalId = task.Id;
            task.Id = tempId;

            if (!_tasksByList.ContainsKey(taskListId)) _tasksByList[taskListId] = new List<TaskItem>();
            _tasksByList[taskListId].Insert(0, task);

            _ = Task.Run(async () => {
                try {
                    var remote = await _remoteService.AddTaskAsync(taskListId, task);
                    task.Id = remote.Id;
                } catch { }
            });

            return task;
        }

        public async Task<TaskItem> UpdateTaskAsync(string taskListId, TaskItem task)
        {
            // In memory it's already updated via binding usually, but let's ensure it's in our list
            _ = Task.Run(async () => {
                try {
                    await _remoteService.UpdateTaskAsync(taskListId, task);
                } catch { }
            });
            return task;
        }

        public async Task DeleteTaskAsync(string taskListId, string taskId)
        {
            if (_tasksByList.TryGetValue(taskListId, out var tasks))
            {
                var t = tasks.FirstOrDefault(x => x.Id == taskId);
                if (t != null) tasks.Remove(t);
            }

            _ = Task.Run(async () => {
                try {
                    await _remoteService.DeleteTaskAsync(taskListId, taskId);
                } catch { }
            });
        }

        public async Task SyncTaskListsAsync()
        {
            try {
                var remoteLists = await _remoteService.GetTaskListsAsync();
                // Merge into memory
                foreach (var remote in remoteLists)
                {
                    var existing = _taskLists.FirstOrDefault(l => l.Id == remote.Id);
                    if (existing == null) _taskLists.Add(remote);
                    else existing.Title = remote.Title;
                }
                _taskLists.RemoveAll(l => !remoteLists.Any(r => r.Id == l.Id) && !l.Id.StartsWith("temp_"));
            } catch { }
        }

        public async Task SyncTasksAsync(string taskListId)
        {
            try {
                var remoteTasks = await _remoteService.GetTasksAsync(taskListId);
                // Merge into memory
                if (!_tasksByList.ContainsKey(taskListId)) _tasksByList[taskListId] = new List<TaskItem>();
                
                var localTasks = _tasksByList[taskListId];
                
                // Update or Add
                foreach (var remote in remoteTasks)
                {
                    var existing = localTasks.FirstOrDefault(t => t.Id == remote.Id);
                    if (existing == null) localTasks.Add(remote);
                    else {
                        existing.Title = remote.Title;
                        existing.Notes = remote.Notes;
                        existing.Status = remote.Status;
                        existing.Due = remote.Due;
                    }
                }
                // Remove
                localTasks.RemoveAll(t => !remoteTasks.Any(r => r.Id == t.Id) && !t.Id.StartsWith("temp_"));
            } catch { }
        }

        public async Task SyncAllAsync()
        {
            await SyncTaskListsAsync();
            var lists = _taskLists.ToList();
            foreach (var list in lists)
            {
                await SyncTasksAsync(list.Id);
            }
        }
    }
}