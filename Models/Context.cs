using Microsoft.EntityFrameworkCore;

namespace Api.Models;

public class Context : DbContext
{
    public Context(DbContextOptions<Context> options) 
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<CartProduct> CartProducts { get; set; } = null!;
    public DbSet<Cart> Carts { get; set; } = null!;
    public DbSet<InvoiceProduct> InvoiceProducts { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    public DbSet<CommandProduct> CommandProducts { get; set; } = null!;
    public DbSet<Command> Commands { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
}