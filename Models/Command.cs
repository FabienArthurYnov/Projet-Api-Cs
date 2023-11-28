namespace Api.Models;

public class Command {
    public int CommandId { get; set;}
    public int UserId { get; set;}
    public string? NameProduct { get; set;}
    public string? TypeProduct { get; set;}
    public float Price { get; set;}
    public bool StatusProduct { get; set;}
}