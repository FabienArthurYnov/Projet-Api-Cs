using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using Api.Models;
using MySql.Data.MySqlClient;

namespace Controllers;
class InvoiceController : Controller
{
    public override void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
        // status ok if not overriden by unexpected events
        statusCode = 200;

        // will hold the invoices
        List<Invoice> invoices = new List<Invoice>();

        // open a new connection
        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        // opening the transaction, and doing it
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // create a new Invoice and the reader for its output
            MySqlCommand command = new MySqlCommand("SELECT * FROM invoices;", connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) { // in case it's empty, there is nothing to read
                while (reader.Read()) {
                    // read the new Invoice
                    Invoice Invoice = new Invoice
                    {
                        InvoiceId = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        NameProduct = reader.GetString(2),
                        TypeProduct = reader.GetString(3),
                        Price = reader.GetFloat(4),
                        StatusProduct = reader.GetInt32(5)
                    };
                    // add the new Invoice to the list
                    invoices.Add(Invoice);
                }
                // to json and change content
                string jsoninvoices = JsonSerializer.Serialize(invoices);
                content = jsoninvoices;
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
        Invoice Invoice = new Invoice();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // the Invoice as a string with correct id
            string invoicestring = "SELECT * FROM invoices WHERE InvoiceId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(invoicestring, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                Invoice = new Invoice
                {
                    InvoiceId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    NameProduct = reader.GetString(2),
                    TypeProduct = reader.GetString(3),
                    Price = reader.GetFloat(4),
                    StatusProduct = reader.GetInt32(5)
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in Invoice  (id:"+id.ToString()+")");
                }
                string jsoninvoices = JsonSerializer.Serialize(Invoice);
                content = jsoninvoices;
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
            // the Invoice as a string with correct id
            string invoicestring = "DELETE FROM invoices WHERE InvoiceId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(invoicestring, connection, transaction);
            int nbOfAffectedRows = command.ExecuteNonQuery();
            if (nbOfAffectedRows == 0) {
                content = "No corresponding Invoice of id " + id.ToString();
            } else if (nbOfAffectedRows == 1) {
                content = "Success : deleted Invoice of id " + id.ToString();
            } else {
                content = "Warning : multiple deleted Invoice of id " + id.ToString();
                Console.WriteLine("Warning : multiple deleted Invoice with id " + id.ToString());
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

        // Deserialize JSON content to Invoice object
        StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
        string body = reader.ReadToEnd();
        Console.WriteLine(body);

        

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();

        MySqlTransaction transaction = connection.BeginTransaction();
        try
        {
            Invoice? JsonInfo  = JsonSerializer.Deserialize<Invoice>(body);
            // if it is null ; body couldn't be deserialized into Invoice. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in Invoice ; \n   " + body);
                content = "Error : Wrong body.\n" + body;
                statusCode = 400;
                throw new Exception();
            }

            string invoicestring = "INSERT INTO invoices (UserId, NameProduct, TypeProduct, Price, StatusProduct) VALUES (@UserId, @NameProduct, @TypeProduct, @Price, @StatusProduct);SELECT LAST_INSERT_ID();";
            MySqlCommand Invoice = new MySqlCommand(invoicestring, connection, transaction);
            Invoice.Parameters.AddWithValue("@UserId", JsonInfo.UserId);
            Invoice.Parameters.AddWithValue("@NameProduct", JsonInfo.NameProduct);
            Invoice.Parameters.AddWithValue("@TypeProduct", JsonInfo.TypeProduct);
            Invoice.Parameters.AddWithValue("@Price", JsonInfo.Price);
            Invoice.Parameters.AddWithValue("@StatusProduct", JsonInfo.StatusProduct);

            // get the id
            int InvoiceId = Convert.ToInt32(Invoice.ExecuteScalar());
            content = "Success : new Invoice can be found at /api/Invoice/" + InvoiceId.ToString();

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
        Invoice Invoice = new Invoice();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // GET by id

            // the Invoice as a string with correct id
            string invoicestring = "SELECT * FROM invoices WHERE InvoiceId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(invoicestring, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                Invoice = new Invoice
                {
                    InvoiceId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    NameProduct = reader.GetString(2),
                    TypeProduct = reader.GetString(3),
                    Price = reader.GetFloat(4),
                    StatusProduct = reader.GetInt32(5)
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in Invoice  (id:"+id.ToString()+")");
                }
                string jsoninvoices = JsonSerializer.Serialize(Invoice);
                content = jsoninvoices;
            } else {
                content = "";
            }
            reader.Close(); 
            if (content == "") {throw new Exception("No Invoice corresponding to Id");}

            // PUT
            Invoice? JsonInfo  = JsonSerializer.Deserialize<Invoice>(new StreamReader(request.InputStream, Encoding.UTF8).ReadToEnd());
            // if it is null ; body couldn't be deserialized into Invoice. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in Invoice \n");
                content = "Error : Wrong body.\n";
                statusCode = 400;
                throw new Exception("Wrong body in PUT Invoice");
            }
            if (JsonInfo.UserId is not 0) { Invoice.UserId = JsonInfo.UserId;}
            if (JsonInfo.NameProduct is not null) { Invoice.NameProduct = JsonInfo.NameProduct;}
            if (JsonInfo.TypeProduct is not null) { Invoice.TypeProduct = JsonInfo.TypeProduct;}
            if (JsonInfo.Price is not 0) { Invoice.Price = JsonInfo.Price;}
            if (JsonInfo.StatusProduct is not 0) { Invoice.StatusProduct = JsonInfo.StatusProduct;}

            // UPDATE
            string commandStringInsert = "UPDATE invoices SET UserId = @UserId, NameProduct = @NameProduct, TypeProduct = @TypeProduct, Price = @Price, StatusProduct = @StatusProduct WHERE InvoiceId = @InvoiceId;";
            MySqlCommand commandInsert = new MySqlCommand(commandStringInsert, connection, transaction);
            commandInsert.Parameters.AddWithValue("@InvoiceId", Invoice.InvoiceId);
            commandInsert.Parameters.AddWithValue("@UserId", JsonInfo.UserId);
            commandInsert.Parameters.AddWithValue("@NameProduct", JsonInfo.NameProduct);
            commandInsert.Parameters.AddWithValue("@TypeProduct", JsonInfo.TypeProduct);
            commandInsert.Parameters.AddWithValue("@Price", JsonInfo.Price);
            commandInsert.Parameters.AddWithValue("@StatusProduct", JsonInfo.StatusProduct);
            commandInsert.ExecuteNonQuery();
            content = "Success : new Invoice updated " + id.ToString();

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

