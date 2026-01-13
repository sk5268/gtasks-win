using System;

namespace Google_Tasks_Client.Models
{
    public class TaskListItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime? Updated { get; set; }
    }
}
