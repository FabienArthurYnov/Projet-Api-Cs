using Microsoft.EntityFrameworkCore;
using Api.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<Context>(opt =>
    // opt.UseInMemoryDatabase("eCommerce"));
    opt.UseSqlite("Data Source=eCommerce.db"));
    // opt.UseSqlServer("Server=localhost;Database=TodoList;User=sa;Password=your_password;");
    
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
