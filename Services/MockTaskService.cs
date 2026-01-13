using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google_Tasks_Client.Models;

namespace Google_Tasks_Client.Services
{
    public class MockTaskService : ITaskService
    {
        public async Task<List<TaskListItem>> GetTaskListsAsync()
        {
            // Simulate network delay
            await Task.Delay(500);

            return new List<TaskListItem>
            {
                new TaskListItem { Id = "1", Title = "My Tasks", Updated = DateTime.Now },
                new TaskListItem { Id = "2", Title = "Shopping List", Updated = DateTime.Now.AddDays(-1) },
                new TaskListItem { Id = "3", Title = "Work Projects", Updated = DateTime.Now.AddDays(-2) }
            };
        }

        public async Task<List<TaskItem>> GetTasksAsync(string taskListId)
        {
            await Task.Delay(500);

            if (taskListId == "1") // My Tasks
            {
                return new List<TaskItem>
                {
                    new TaskItem { Id = "101", Title = "Buy Milk", Status = "needsAction" },
                    new TaskItem { Id = "102", Title = "Walk the dog", Status = "completed", Due = DateTime.Now.AddHours(2) },
                    new TaskItem { Id = "103", Title = "Pay bills", Notes = "Electricity and Internet", Status = "needsAction" }
                };
            }
            else if (taskListId == "2") // Shopping List
            {
                return new List<TaskItem>
                {
                    new TaskItem { Id = "201", Title = "Apples", Status = "needsAction" },
                    new TaskItem { Id = "202", Title = "Bananas", Status = "completed" },
                    new TaskItem { Id = "203", Title = "Bread", Status = "needsAction" }
                };
            }
            
            return new List<TaskItem>();
        }
    }
}
