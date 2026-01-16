using Microsoft.EntityFrameworkCore;
using Google_Tasks_Client.Models;
using System;
using System.IO;

namespace Google_Tasks_Client.Services
{
    public class LocalTaskItem : TaskItem
    {
        public string TaskListId { get; set; } = string.Empty;
    }

    public class AppDbContext : DbContext
    {
        public DbSet<TaskListItem> TaskLists { get; set; }
        public DbSet<LocalTaskItem> Tasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "google_tasks_cache.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>().HasKey(t => t.Id);
            modelBuilder.Entity<TaskListItem>().HasKey(t => t.Id);
        }
    }
}
