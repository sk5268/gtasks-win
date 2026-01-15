using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Util.Store;
using Google_Tasks_Client.Models;

namespace Google_Tasks_Client.Services
{
    public class GoogleTaskService : ITaskService
    {
        private TasksService? _service;
        private readonly string[] _scopes = { TasksService.Scope.Tasks };
        private readonly string _appName = "Google Tasks Client";

        private async System.Threading.Tasks.Task EnsureInitializedAsync()
        {
            if (_service != null) return;

            UserCredential credential;

            // BEST PRACTICE: Load secrets from a file that is NOT committed to source control
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                // The token is stored in a local folder so the user stays logged in
                string credPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GoogleTasksClientToken");
                
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    _scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
            }

            _service = new TasksService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _appName,
            });
        }

        public async Task<List<TaskListItem>> GetTaskListsAsync()
        {
            await EnsureInitializedAsync();
            var request = _service!.Tasklists.List();
            var result = await request.ExecuteAsync();

            return result.Items.Select(item => {
                DateTime? updated = null;
                if (DateTime.TryParse(item.Updated, out var dt)) updated = dt;
                
                return new TaskListItem
                {
                    Id = item.Id,
                    Title = item.Title,
                    Updated = updated
                };
            }).ToList();
        }

        public async Task<TaskListItem> AddTaskListAsync(string title)
        {
            await EnsureInitializedAsync();
            var taskList = new Google.Apis.Tasks.v1.Data.TaskList { Title = title };
            var result = await _service!.Tasklists.Insert(taskList).ExecuteAsync();
            
            DateTime? updated = null;
            if (DateTime.TryParse(result.Updated, out var dt)) updated = dt;

            return new TaskListItem
            {
                Id = result.Id,
                Title = result.Title,
                Updated = updated
            };
        }

        public async System.Threading.Tasks.Task DeleteTaskListAsync(string taskListId)
        {
            await EnsureInitializedAsync();
            await _service!.Tasklists.Delete(taskListId).ExecuteAsync();
        }

        public async System.Threading.Tasks.Task<List<TaskItem>> GetTasksAsync(string taskListId)
        {
            await EnsureInitializedAsync();
            var request = _service!.Tasks.List(taskListId);
            request.ShowCompleted = true;
            request.MaxResults = 100;
            var result = await request.ExecuteAsync();

            if (result.Items == null) return new List<TaskItem>();

            return result.Items.Select(item => {
                DateTime? due = null;
                if (DateTime.TryParse(item.Due, out var dt)) due = dt;

                return new TaskItem
                {
                    Id = item.Id,
                    Title = item.Title ?? "",
                    Notes = item.Notes ?? "",
                    Status = item.Status ?? "needsAction",
                    Due = due
                };
            }).ToList();
        }

        public async Task<TaskItem> AddTaskAsync(string taskListId, TaskItem task)
        {
            await EnsureInitializedAsync();
            var googleTask = new Google.Apis.Tasks.v1.Data.Task
            {
                Title = task.Title,
                Notes = task.Notes,
                Status = task.Status,
                Due = task.Due?.ToString("yyyy-MM-ddTHH:mm:ss.fffK")
            };

            var request = _service!.Tasks.Insert(googleTask, taskListId);
            var item = await request.ExecuteAsync();

            DateTime? due = null;
            if (DateTime.TryParse(item.Due, out var dt)) due = dt;

            return new TaskItem
            {
                Id = item.Id,
                Title = item.Title,
                Notes = item.Notes,
                Status = item.Status,
                Due = due
            };
        }

        public async Task<TaskItem> UpdateTaskAsync(string taskListId, TaskItem task)
        {
            await EnsureInitializedAsync();
            var googleTask = new Google.Apis.Tasks.v1.Data.Task
            {
                Id = task.Id,
                Title = task.Title,
                Notes = task.Notes,
                Status = task.Status,
                Due = task.Due?.ToString("yyyy-MM-ddTHH:mm:ss.fffK")
            };

            var request = _service!.Tasks.Update(googleTask, taskListId, task.Id);
            var item = await request.ExecuteAsync();

            DateTime? due = null;
            if (DateTime.TryParse(item.Due, out var dt)) due = dt;

            return new TaskItem
            {
                Id = item.Id,
                Title = item.Title,
                Notes = item.Notes,
                Status = item.Status,
                Due = due
            };
        }

        public async System.Threading.Tasks.Task DeleteTaskAsync(string taskListId, string taskId)
        {
            await EnsureInitializedAsync();
            var request = _service!.Tasks.Delete(taskListId, taskId);
            await request.ExecuteAsync();
        }

        public System.Threading.Tasks.Task SyncTaskListsAsync() => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task SyncTasksAsync(string taskListId) => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task SyncAllAsync() => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task InitializeAsync() => System.Threading.Tasks.Task.CompletedTask;
        public System.Threading.Tasks.Task PersistAsync() => System.Threading.Tasks.Task.CompletedTask;
    }
}
