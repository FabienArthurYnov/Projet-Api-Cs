namespace Api.Models;

public class User {
    public int UserId { get; set;}
    public string? FirstName { get; set;}
    public string? LastName { get; set;}
    public string? Password { get; set;}  // ye bad, maybe in the future
    public string? Email { get; set;}
}