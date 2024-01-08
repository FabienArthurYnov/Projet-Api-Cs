using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using Api.Models;
using MySql.Data.MySqlClient;

namespace Controllers;
class ProductControllers : Controller
{
    public override void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
        // status ok if not overriden by unexpected events
        statusCode = 200;

        // will hold the products
        List<Product> products = new List<Product>();

        // open a new connection
        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        // opening the transaction, and doing it
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // create a new command and the reader for its output
            MySqlCommand command = new MySqlCommand("SELECT * FROM products;", connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) { // in case it's empty, there is nothing to read
                while (reader.Read()) {
                    // read the new product
                    Product product = new Product
                    {
                        ProductId = reader.GetInt32(0),
                        NameProduct = reader.GetString(1),
                        TypeProduct = reader.GetString(2),
                        DescriptionProduct = reader.GetString(3),
                        Price = reader.GetFloat(4),
                        StatusProduct = reader.GetInt32(5)
                    };
                    // add the new product to the list
                    products.Add(product);
                }
                // to json and change content
                string jsonproducts = JsonSerializer.Serialize(products);
                content = jsonproducts;
            } else {
                // there is no row, nothing to send
                content = "";
            }
            reader.Close();
            // success, commit
            transaction.Commit();
        } catch (Exception e) {
            // error, rollback
            Console.WriteLine("Error : " + e.Message);
            transaction.Rollback();
            Console.WriteLine("Transaction rolled back");
            // internal error
            statusCode = 500;
        }

        base.GetRequest(response, content, statusCode); // base keyword : like super() in java, call the parent class
    }


    public override void GetByIdRequest(HttpListenerResponse response, int id, string content = "", int statusCode = 501) {
        statusCode = 200;
        Product product = new Product();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // the command as a string with correct id
            string commandString = "SELECT * FROM products WHERE ProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                product = new Product
                {
                    ProductId = reader.GetInt32(0),
                    NameProduct = reader.GetString(1),
                    TypeProduct = reader.GetString(2),
                    DescriptionProduct = reader.GetString(3),
                    Price = reader.GetFloat(4),
                    StatusProduct = reader.GetInt32(5)
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in product  (id:"+id.ToString()+")");
                }
                string jsonproducts = JsonSerializer.Serialize(product);
                content = jsonproducts;
            } else {
                content = "";
            }
            reader.Close(); 
            transaction.Commit();
        } catch (Exception e) {
            Console.WriteLine("Error : " + e.Message);
            transaction.Rollback();
            Console.WriteLine("Transaction rolled back");
            statusCode = 500;
        }

        base.GetRequest(response, content, statusCode);
    }


    public override void DeleteRequest(HttpListenerResponse response, int id, string content = "", int statusCode = 501) {
        statusCode = 200;

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // the command as a string with correct id
            string commandString = "DELETE FROM products WHERE ProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            int nbOfAffectedRows = command.ExecuteNonQuery();
            if (nbOfAffectedRows == 0) {
                content = "No corresponding product of id " + id.ToString();
            } else if (nbOfAffectedRows == 1) {
                content = "Success : deleted product of id " + id.ToString();
            } else {
                content = "Warning : multiple deleted product of id " + id.ToString();
                Console.WriteLine("Warning : multiple deleted product with id " + id.ToString());
            }
            
            transaction.Commit();
        } catch (Exception e) {
            Console.WriteLine("Error : " + e.Message);
            transaction.Rollback();
            Console.WriteLine("Transaction rolled back");
            statusCode = 500;
        }

        base.GetRequest(response, content, statusCode);
    }

    public override void PostRequest(HttpListenerResponse response, HttpListenerRequest request, string content = "Unimplemented", int statusCode = 501) {
        statusCode = 201;  // Created status code for successful POST

        // Deserialize JSON content to product object
        StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
        string body = reader.ReadToEnd();
        Console.WriteLine(body);

        

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();

        MySqlTransaction transaction = connection.BeginTransaction();
        try
        {
            Product? JsonInfo  = JsonSerializer.Deserialize<Product>(body);
            // if it is null ; body couldn't be deserialized into product. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in product ; \n   " + body);
                content = "Error : Wrong body.\n" + body;
                statusCode = 400;
                throw new Exception();
            }

            string commandString = "INSERT INTO products (NameProduct, TypeProduct, DescriptionProduct, Price, StatusProduct) VALUES (@NameProduct, @TypeProduct, @DescriptionProduct, @Price, @StatusProduct);SELECT LAST_INSERT_ID();";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            command.Parameters.AddWithValue("@NameProduct", JsonInfo.NameProduct);
            command.Parameters.AddWithValue("@TypeProduct", JsonInfo.TypeProduct);
            command.Parameters.AddWithValue("@DescriptionProduct", JsonInfo.DescriptionProduct);
            command.Parameters.AddWithValue("@Price", JsonInfo.Price);
            command.Parameters.AddWithValue("@StatusProduct", JsonInfo.StatusProduct);

            // get the id
            int productId = Convert.ToInt32(command.ExecuteScalar());
            content = "Success : new product can be found at /api/product/" + productId.ToString();

            // Commit the transaction
            transaction.Commit();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            transaction.Rollback();
            Console.WriteLine("Transaction rolled back");
            content = "400: Bad request";
            statusCode = 400;  // Internal Server Error status code
        }
        finally
        {
            connection.Close();
        }

        base.PostRequest(response, request, content, statusCode);
    }


    public override void PutRequest(HttpListenerResponse response, int id, HttpListenerRequest request, string content = "", int statusCode = 501) {
        statusCode = 200;
        Product product = new Product();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // GET by id

            // the command as a string with correct id
            string commandString = "SELECT * FROM products WHERE ProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                product = new Product
                {
                    ProductId = reader.GetInt32(0),
                    NameProduct = reader.GetString(1),
                    TypeProduct = reader.GetString(2),
                    DescriptionProduct = reader.GetString(3),
                    Price = reader.GetFloat(4),
                    StatusProduct = reader.GetInt32(5)
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in product  (id:"+id.ToString()+")");
                }
                string jsonproducts = JsonSerializer.Serialize(product);
                content = jsonproducts;
            } else {
                content = "";
            }
            reader.Close(); 
            if (content == "") {throw new Exception("No product corresponding to Id");}

            // PUT
            Product? JsonInfo  = JsonSerializer.Deserialize<Product>(new StreamReader(request.InputStream, Encoding.UTF8).ReadToEnd());
            // if it is null ; body couldn't be deserialized into product. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in product \n");
                content = "Error : Wrong body.\n";
                statusCode = 400;
                throw new Exception("Wrong body in PUT product");
            }
            if (JsonInfo.NameProduct is not null) { product.NameProduct = JsonInfo.NameProduct;}
            if (JsonInfo.TypeProduct is not null) { product.TypeProduct = JsonInfo.TypeProduct;}
            if (JsonInfo.DescriptionProduct is not null) { product.DescriptionProduct = JsonInfo.DescriptionProduct;}
            if (JsonInfo.Price is not 0) { product.Price = JsonInfo.Price;}
            if (JsonInfo.StatusProduct is not 0) { product.StatusProduct = JsonInfo.StatusProduct;}

            // UPDATE
            string commandStringInsert = "UPDATE products SET NameProduct = @NameProduct, TypeProduct = @TypeProduct, DescriptionProduct = @DescriptionProduct, Price = @Price, StatusProduct = @StatusProduct WHERE ProductId = @ProductId;";
            MySqlCommand commandInsert = new MySqlCommand(commandStringInsert, connection, transaction);
            commandInsert.Parameters.AddWithValue("@ProductId", product.ProductId);
            command.Parameters.AddWithValue("@NameProduct", JsonInfo.NameProduct);
            command.Parameters.AddWithValue("@TypeProduct", JsonInfo.TypeProduct);
            command.Parameters.AddWithValue("@DescriptionProduct", JsonInfo.DescriptionProduct);
            command.Parameters.AddWithValue("@Price", JsonInfo.Price);
            command.Parameters.AddWithValue("@StatusProduct", JsonInfo.StatusProduct);
            commandInsert.ExecuteNonQuery();
            content = "Success : new product updated " + id.ToString();

            transaction.Commit();
        } catch (Exception e) {
            Console.WriteLine("Error : " + e.Message);
            transaction.Rollback();
            Console.WriteLine("Transaction rolled back");
            statusCode = 400;
            content = "400 : Bad request";
        }

        base.PutRequest(response, id, request, content, statusCode);
    }
}

