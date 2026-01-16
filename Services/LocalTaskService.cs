using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Google_Tasks_Client.Models;

namespace Google_Tasks_Client.Services
{
    public class LocalTaskService
    {
        public async Task<List<TaskListItem>> GetTaskListsAsync()
        {
            using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();
            return await db.TaskLists.AsNoTracking().OrderBy(l => l.Title).ToListAsync();
        }

        public async Task<List<TaskItem>> GetTasksAsync(string taskListId)
        {
            using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();
            var tasks = await db.Tasks
                .AsNoTracking()
                .Where(t => t.TaskListId == taskListId)
                .ToListAsync();
            return tasks.Select(t => (TaskItem)t).ToList();
        }

        public async Task SaveTaskListsAsync(List<TaskListItem> lists)
        {
            using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();
            
            var existing = await db.TaskLists.AsNoTracking().ToListAsync();
            var existingIds = existing.Select(l => l.Id).ToHashSet();
            var remoteIds = lists.Select(l => l.Id).ToHashSet();

            // 1. Delete
            var toDelete = existing.Where(l => !remoteIds.Contains(l.Id)).ToList();
            if (toDelete.Any()) db.TaskLists.RemoveRange(toDelete);

            // 2. Update or Insert
            foreach (var list in lists)
            {
                if (existingIds.Contains(list.Id))
                    db.TaskLists.Update(list);
                else
                    db.TaskLists.Add(list);
            }

            await db.SaveChangesAsync();
        }

        public async Task SaveTasksAsync(string taskListId, List<TaskItem> tasks)
        {
            using var db = new AppDbContext();
            await db.Database.EnsureCreatedAsync();

            var existingTasks = await db.Tasks
                .AsNoTracking()
                .Where(t => t.TaskListId == taskListId)
                .ToListAsync();
            
            var existingIds = existingTasks.Select(t => t.Id).ToHashSet();
            var remoteIds = tasks.Select(t => t.Id).ToHashSet();

            // 1. Delete
            var toDelete = existingTasks.Where(t => !remoteIds.Contains(t.Id)).ToList();
            if (toDelete.Any()) db.Tasks.RemoveRange(toDelete);

            // 2. Update or Insert
            foreach (var task in tasks)
            {
                var localTask = new LocalTaskItem
                {
                    Id = task.Id,
                    Title = task.Title ?? "",
                    Notes = task.Notes ?? "",
                    Status = task.Status ?? "needsAction",
                    Due = task.Due,
                    TaskListId = taskListId,
                    ParentId = task.ParentId ?? string.Empty
                };

                if (existingIds.Contains(task.Id))
                    db.Tasks.Update(localTask);
                else
                    db.Tasks.Add(localTask);
            }

            await db.SaveChangesAsync();
        }
    }
}