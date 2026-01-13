using System.Collections.Generic;
using System.Threading.Tasks;
using Google_Tasks_Client.Models;

namespace Google_Tasks_Client.Services
{
    public interface ITaskService
    {
        Task<List<TaskListItem>> GetTaskListsAsync();
        Task<List<TaskItem>> GetTasksAsync(string taskListId);
    }
}
