namespace Api.Models;

public class Product {
    public int ProductId { get; set;}
    public string? NameProduct { get; set;}  // string? -> can be NULL   string -> can't
    public string? TypeProduct { get; set;}
    public string? DescriptionProduct { get; set;}
    public float Price { get; set;}
    public bool StatusProduct {get; set;}
}