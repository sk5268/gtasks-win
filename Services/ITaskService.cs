using System.Collections.Generic;
using System.Threading.Tasks;
using Google_Tasks_Client.Models;

namespace Google_Tasks_Client.Services
{
    public interface ITaskService
    {
        Task<List<TaskListItem>> GetTaskListsAsync();
        Task<List<TaskItem>> GetTasksAsync(string taskListId);
        Task<TaskListItem> AddTaskListAsync(string title);
        Task DeleteTaskListAsync(string taskListId);
        Task<TaskItem> AddTaskAsync(string taskListId, TaskItem task);
        Task<TaskItem> UpdateTaskAsync(string taskListId, TaskItem task);
        Task DeleteTaskAsync(string taskListId, string taskId);
    }
}
