using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using Api.Models;
using MySql.Data.MySqlClient;

namespace Controllers;
class CartProductController : Controller
{
    public override void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
        // status ok if not overriden by unexpected events
        statusCode = 200;

        // will hold the carts_products
        List<CartProduct> carts_products = new List<CartProduct>();

        // open a new connection
        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        // opening the transaction, and doing it
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // create a new CartProduct and the reader for its output
            MySqlCommand command = new MySqlCommand("SELECT * FROM carts_products;", connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) { // in case it's empty, there is nothing to read
                while (reader.Read()) {
                    // read the new CartProduct
                    CartProduct CartProduct = new CartProduct
                    {
                        CartProductId = reader.GetInt32(0),
                        CartId = reader.GetInt32(1),
                        ProductId = reader.GetInt32(2),
                    };
                    // add the new CartProduct to the list
                    carts_products.Add(CartProduct);
                }
                // to json and change content
                string jsoncarts_products = JsonSerializer.Serialize(carts_products);
                content = jsoncarts_products;
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
        CartProduct CartProduct = new CartProduct();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // the CartProduct as a string with correct id
            string carts_productstring = "SELECT * FROM carts_products WHERE CartProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(carts_productstring, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                CartProduct = new CartProduct
                {
                    CartProductId = reader.GetInt32(0),
                    CartId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in CartProduct  (id:"+id.ToString()+")");
                }
                string jsoncarts_products = JsonSerializer.Serialize(CartProduct);
                content = jsoncarts_products;
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
            // the CartProduct as a string with correct id
            string carts_productstring = "DELETE FROM carts_products WHERE CartProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(carts_productstring, connection, transaction);
            int nbOfAffectedRows = command.ExecuteNonQuery();
            if (nbOfAffectedRows == 0) {
                content = "No corresponding CartProduct of id " + id.ToString();
            } else if (nbOfAffectedRows == 1) {
                content = "Success : deleted CartProduct of id " + id.ToString();
            } else {
                content = "Warning : multiple deleted CartProduct of id " + id.ToString();
                Console.WriteLine("Warning : multiple deleted CartProduct with id " + id.ToString());
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

        // Deserialize JSON content to CartProduct object
        StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
        string body = reader.ReadToEnd();
        Console.WriteLine(body);

        

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();

        MySqlTransaction transaction = connection.BeginTransaction();
        try
        {
            CartProduct? JsonInfo  = JsonSerializer.Deserialize<CartProduct>(body);
            // if it is null ; body couldn't be deserialized into CartProduct. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in CartProduct ; \n   " + body);
                content = "Error : Wrong body.\n" + body;
                statusCode = 400;
                throw new Exception();
            }

            string commandString = "INSERT INTO carts_products (CartId, ProductId) VALUES (@CartId, @ProductId);SELECT LAST_INSERT_ID();";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            command.Parameters.AddWithValue("@CartId", JsonInfo.CartId);
            command.Parameters.AddWithValue("@ProductId", JsonInfo.ProductId);

            // get the id
            int CartProductId = Convert.ToInt32(command.ExecuteScalar());
            content = "Success : new CartProduct can be found at /api/CartProduct/" + CartProductId.ToString();

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
        CartProduct CartProduct = new CartProduct();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // GET by id

            // the CartProduct as a string with correct id
            string carts_productstring = "SELECT * FROM carts_products WHERE CartProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(carts_productstring, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                CartProduct = new CartProduct
                {
                    CartProductId = reader.GetInt32(0),
                    CartId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in CartProduct  (id:"+id.ToString()+")");
                }
                string jsoncarts_products = JsonSerializer.Serialize(CartProduct);
                content = jsoncarts_products;
            } else {
                content = "";
            }
            reader.Close(); 
            if (content == "") {throw new Exception("No CartProduct corresponding to Id");}

            // PUT
            CartProduct? JsonInfo  = JsonSerializer.Deserialize<CartProduct>(new StreamReader(request.InputStream, Encoding.UTF8).ReadToEnd());
            // if it is null ; body couldn't be deserialized into CartProduct. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in CartProduct \n");
                content = "Error : Wrong body.\n";
                statusCode = 400;
                throw new Exception("Wrong body in PUT CartProduct");
            }
            if (JsonInfo.CartId is not 0) { CartProduct.CartId = JsonInfo.CartId;}
            if (JsonInfo.ProductId is not 0) { CartProduct.ProductId = JsonInfo.ProductId;}

            // UPDATE
            string commandStringInsert = "UPDATE carts_products SET CartId = @CartId, ProductId = @ProductId WHERE CartProductId = @CartProductId;";
            MySqlCommand commandInsert = new MySqlCommand(commandStringInsert, connection, transaction);
            commandInsert.Parameters.AddWithValue("@CartProductId", CartProduct.CartProductId);
            commandInsert.Parameters.AddWithValue("@CartId", JsonInfo.CartId);
            commandInsert.Parameters.AddWithValue("@ProductId", JsonInfo.ProductId);
            commandInsert.ExecuteNonQuery();
            content = "Success : new CartProduct updated " + id.ToString();

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

