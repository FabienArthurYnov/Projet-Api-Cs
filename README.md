# Projet d'API de suivie de stock, initiation C#

## To run :

- Install python 3.11

- Install .NET sdk 8.0

- Download the project

- Put the connectionString of the MySQL server in a connectionString.txt at the root of the project

- Run `dotnet run` at the project's root

---
## How to use :
access it with http://[server]:8080/api

Don't add the itemId, first field, when posting. It is added automatically.

- ./api/address : user's addresses.   int AddressId, int UserId, string AddressString

- ./api/cart : user's carts.   int CartId, int UserId, float Price, int StatusProdct
- ./api/cartproduct : link a product to a cart.   int CartProductId, int CartId, int ProductId
- ./api/command : user's commands.   int CommandId, int UserId, float Price, int StatusProdct
- ./api/commandproduct : link a product to a command.   int CommandProductId, int CommandId, int ProductId
- ./api/invoice : user's invoices.   int InvoiceId, int UserId, float Price, int StatusProdct
- ./api/invoiceproduct : link a product to an invoice.   int InvoiceProductId, int InvoiceId, int ProductId

- ./api/product : a Product.   int ProductId, string NameProduct, string TypeProduct, string DescriptionProduct, float price, int StatusProduct
- ./api/user : a User.   int UserId, string FirstName, string LastName, string Password, string Email
---

  

### Auteurs ;

-- Fabien ARTHUR

-- Alexandre JIN

-- Gabriel GARCIA

-- Mathieu DENIEUL LE DIRAISON
