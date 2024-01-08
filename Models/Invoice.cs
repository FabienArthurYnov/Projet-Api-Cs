namespace Api.Models;

public class Invoice {
    public int InvoiceId { get; set;}
    public int UserId { get; set;}
    public string? NameProduct { get; set;}
    public string? TypeProduct { get; set;}
    public float Price { get; set;}
    public int StatusProduct { get; set;}
}