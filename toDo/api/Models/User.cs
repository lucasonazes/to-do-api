namespace api.Models;

public class User
{
    public User() 
    {
        RegisteredIn = DateTime.Now;
    }

    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public DateTime RegisteredIn { get; set; }
    public List<Task>? Tasks { get; set; }
}
