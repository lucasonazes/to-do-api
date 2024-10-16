namespace api.Models;

public class Project
{
    public Project() 
    {
        StartDate = DateTime.Now;
    }

    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime FinalDate { get; set; }
}
