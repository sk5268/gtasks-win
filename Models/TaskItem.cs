using System;

namespace Google_Tasks_Client.Models
{
    public class TaskItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "needsAction"; // "needsAction" or "completed"
        public DateTime? Due { get; set; }
        
        // Helper for Binding
        public bool IsCompleted 
        { 
            get => Status == "completed";
            set => Status = value ? "completed" : "needsAction";
        }
    }
}