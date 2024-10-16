namespace api.Models;

public class Task
{
    public Task() {
        CreatedAt = DateTime.Now;
        Status = "unfinished";
    }

    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; }
    public User? User { get; set; }
    public Project? Project { get; set; }
}
