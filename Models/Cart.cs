namespace Api.Models;

public class Cart {
    public int CartId { get; set;}
    public int UserId { get; set;}
    public float Price { get; set;}
    public int StatusProduct { get; set;}
}