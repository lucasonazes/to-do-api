namespace api.Models;

public class Task
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? DueDate { get; set; }
    public bool Finished { get; set; } = false;
    public int UserId { get; set; }
    public int TagId { get; set; }
    public int ProjectId { get; set; }
}
