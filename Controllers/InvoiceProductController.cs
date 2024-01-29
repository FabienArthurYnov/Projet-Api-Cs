using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using Api.Models;
using MySql.Data.MySqlClient;

namespace Controllers;
class InvoiceProductController : Controller
{
    public override void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
        // status ok if not overriden by unexpected events
        statusCode = 200;

        // will hold the invoices_products
        List<InvoiceProduct> invoices_products = new List<InvoiceProduct>();

        // open a new connection
        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        // opening the transaction, and doing it
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // create a new InvoiceProduct and the reader for its output
            MySqlCommand command = new MySqlCommand("SELECT * FROM invoices_products;", connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) { // in case it's empty, there is nothing to read
                while (reader.Read()) {
                    // read the new InvoiceProduct
                    InvoiceProduct InvoiceProduct = new InvoiceProduct
                    {
                        InvoiceProductId = reader.GetInt32(0),
                        InvoiceId = reader.GetInt32(1),
                        ProductId = reader.GetInt32(2),
                    };
                    // add the new InvoiceProduct to the list
                    invoices_products.Add(InvoiceProduct);
                }
                // to json and change content
                string jsoninvoices_products = JsonSerializer.Serialize(invoices_products);
                content = jsoninvoices_products;
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
        connection.Close();
        base.GetRequest(response, content, statusCode); // base keyword : like super() in java, call the parent class
    }


    public override void GetByIdRequest(HttpListenerResponse response, int id, string content = "", int statusCode = 501) {
        statusCode = 200;
        InvoiceProduct InvoiceProduct = new InvoiceProduct();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // the InvoiceProduct as a string with correct id
            string invoices_productstring = "SELECT * FROM invoices_products WHERE InvoiceProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(invoices_productstring, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                InvoiceProduct = new InvoiceProduct
                {
                    InvoiceProductId = reader.GetInt32(0),
                    InvoiceId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in InvoiceProduct  (id:"+id.ToString()+")");
                }
                string jsoninvoices_products = JsonSerializer.Serialize(InvoiceProduct);
                content = jsoninvoices_products;
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
        connection.Close();
        base.GetRequest(response, content, statusCode);
    }


    public override void DeleteRequest(HttpListenerResponse response, int id, string content = "", int statusCode = 501) {
        statusCode = 200;

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // the InvoiceProduct as a string with correct id
            string commandString = "DELETE FROM invoices_products WHERE InvoiceProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            int nbOfAffectedRows = command.ExecuteNonQuery();
            if (nbOfAffectedRows == 0) {
                content = "No corresponding InvoiceProduct of id " + id.ToString();
            } else if (nbOfAffectedRows == 1) {
                content = "Success : deleted InvoiceProduct of id " + id.ToString();
            } else {
                content = "Warning : multiple deleted InvoiceProduct of id " + id.ToString();
                Console.WriteLine("Warning : multiple deleted InvoiceProduct with id " + id.ToString());
            }
            
            transaction.Commit();
        } catch (Exception e) {
            Console.WriteLine("Error : " + e.Message);
            transaction.Rollback();
            Console.WriteLine("Transaction rolled back");
            statusCode = 500;
        }
        connection.Close();
        base.GetRequest(response, content, statusCode);
    }

    public override void PostRequest(HttpListenerResponse response, HttpListenerRequest request, string content = "Unimplemented", int statusCode = 501) {
        statusCode = 201;  // Created status code for successful POST

        // Deserialize JSON content to InvoiceProduct object
        StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
        string body = reader.ReadToEnd();
        Console.WriteLine(body);

        

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();

        MySqlTransaction transaction = connection.BeginTransaction();
        try
        {
            InvoiceProduct? JsonInfo  = JsonSerializer.Deserialize<InvoiceProduct>(body);
            // if it is null ; body couldn't be deserialized into InvoiceProduct. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in InvoiceProduct ; \n   " + body);
                content = "Error : Wrong body.\n" + body;
                statusCode = 400;
                throw new Exception();
            }

            string commandString = "INSERT INTO invoices_products (InvoiceId, ProductId) VALUES (@InvoiceId, @ProductId);SELECT LAST_INSERT_ID();";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            command.Parameters.AddWithValue("@InvoiceId", JsonInfo.InvoiceId);
            command.Parameters.AddWithValue("@ProductId", JsonInfo.ProductId);

            // get the id
            int InvoiceProductId = Convert.ToInt32(command.ExecuteScalar());
            content = "Success : new InvoiceProduct can be found at /api/InvoiceProduct/" + InvoiceProductId.ToString();

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
        connection.Close();
        base.PostRequest(response, request, content, statusCode);
    }


    public override void PutRequest(HttpListenerResponse response, int id, HttpListenerRequest request, string content = "", int statusCode = 501) {
        statusCode = 200;
        InvoiceProduct InvoiceProduct = new InvoiceProduct();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // GET by id

            // the InvoiceProduct as a string with correct id
            string invoices_productstring = "SELECT * FROM invoices_products WHERE InvoiceProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(invoices_productstring, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                InvoiceProduct = new InvoiceProduct
                {
                    InvoiceProductId = reader.GetInt32(0),
                    InvoiceId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in InvoiceProduct  (id:"+id.ToString()+")");
                }
                string jsoninvoices_products = JsonSerializer.Serialize(InvoiceProduct);
                content = jsoninvoices_products;
            } else {
                content = "";
            }
            reader.Close(); 
            if (content == "") {throw new Exception("No InvoiceProduct corresponding to Id");}

            // PUT
            InvoiceProduct? JsonInfo  = JsonSerializer.Deserialize<InvoiceProduct>(new StreamReader(request.InputStream, Encoding.UTF8).ReadToEnd());
            // if it is null ; body couldn't be deserialized into InvoiceProduct. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in InvoiceProduct \n");
                content = "Error : Wrong body.\n";
                statusCode = 400;
                throw new Exception("Wrong body in PUT InvoiceProduct");
            }
            if (JsonInfo.InvoiceId is not 0) { InvoiceProduct.InvoiceId = JsonInfo.InvoiceId;}
            if (JsonInfo.ProductId is not 0) { InvoiceProduct.ProductId = JsonInfo.ProductId;}

            // UPDATE
            string commandStringInsert = "UPDATE invoices_products SET Invoiceid = @InvoiceId, ProductId = @ProductId WHERE InvoiceProductId = @InvoiceProductId;";
            MySqlCommand commandInsert = new MySqlCommand(commandStringInsert, connection, transaction);
            commandInsert.Parameters.AddWithValue("@InvoiceProductId", InvoiceProduct.InvoiceProductId);
            commandInsert.Parameters.AddWithValue("@InvoiceId", JsonInfo.InvoiceId);
            commandInsert.Parameters.AddWithValue("@ProductId", JsonInfo.ProductId);
            commandInsert.ExecuteNonQuery();
            content = "Success : new InvoiceProduct updated " + id.ToString();

            transaction.Commit();
        } catch (Exception e) {
            Console.WriteLine("Error : " + e.Message);
            transaction.Rollback();
            Console.WriteLine("Transaction rolled back");
            statusCode = 400;
            content = "400 : Bad request";
        }
        connection.Close();
        base.PutRequest(response, id, request, content, statusCode);
    }
}

