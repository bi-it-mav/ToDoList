namespace ToDoList.Models
{
    public class ToDo
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastModifiedAt { get; set; } = DateTime.Now;
        public DateTime? DueAt { get; set; }
        public bool IsDone { get; set; } = false;
        public string Description { get; set; } = "";
    }
}
