namespace api.Models;

public class Project
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime FinalDate { get; set; }

    public List<Task>? Tasks { get; set; } = new List<Task>();
}
