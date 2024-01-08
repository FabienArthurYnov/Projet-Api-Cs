using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using Api.Models;
using MySql.Data.MySqlClient;

namespace Controllers;
class CommandProductController : Controller
{
    public override void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
        // status ok if not overriden by unexpected events
        statusCode = 200;

        // will hold the commands_products
        List<CommandProduct> commands_products = new List<CommandProduct>();

        // open a new connection
        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        // opening the transaction, and doing it
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // create a new CommandProduct and the reader for its output
            MySqlCommand command = new MySqlCommand("SELECT * FROM commands_products;", connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) { // in case it's empty, there is nothing to read
                while (reader.Read()) {
                    // read the new CommandProduct
                    CommandProduct CommandProduct = new CommandProduct
                    {
                        CommandProductId = reader.GetInt32(0),
                        CommandId = reader.GetInt32(1),
                        ProductId = reader.GetInt32(2),
                    };
                    // add the new CommandProduct to the list
                    commands_products.Add(CommandProduct);
                }
                // to json and change content
                string jsoncommands_products = JsonSerializer.Serialize(commands_products);
                content = jsoncommands_products;
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
        CommandProduct CommandProduct = new CommandProduct();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // the CommandProduct as a string with correct id
            string commands_productstring = "SELECT * FROM commands_products WHERE CommandProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commands_productstring, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                CommandProduct = new CommandProduct
                {
                    CommandProductId = reader.GetInt32(0),
                    CommandId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in CommandProduct  (id:"+id.ToString()+")");
                }
                string jsoncommands_products = JsonSerializer.Serialize(CommandProduct);
                content = jsoncommands_products;
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
            // the CommandProduct as a string with correct id
            string commands_productstring = "DELETE FROM commands_products WHERE CommandProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commands_productstring, connection, transaction);
            int nbOfAffectedRows = command.ExecuteNonQuery();
            if (nbOfAffectedRows == 0) {
                content = "No corresponding CommandProduct of id " + id.ToString();
            } else if (nbOfAffectedRows == 1) {
                content = "Success : deleted CommandProduct of id " + id.ToString();
            } else {
                content = "Warning : multiple deleted CommandProduct of id " + id.ToString();
                Console.WriteLine("Warning : multiple deleted CommandProduct with id " + id.ToString());
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

        // Deserialize JSON content to CommandProduct object
        StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
        string body = reader.ReadToEnd();
        Console.WriteLine(body);

        

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();

        MySqlTransaction transaction = connection.BeginTransaction();
        try
        {
            CommandProduct? JsonInfo  = JsonSerializer.Deserialize<CommandProduct>(body);
            // if it is null ; body couldn't be deserialized into CommandProduct. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in CommandProduct ; \n   " + body);
                content = "Error : Wrong body.\n" + body;
                statusCode = 400;
                throw new Exception();
            }

            string commandString = "INSERT INTO commands_products (CommandId, ProductId) VALUES (@CommandId, @ProductId);SELECT LAST_INSERT_ID();";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            command.Parameters.AddWithValue("@CommandId", JsonInfo.CommandId);
            command.Parameters.AddWithValue("@ProductId", JsonInfo.ProductId);

            // get the id
            int CommandProductId = Convert.ToInt32(command.ExecuteScalar());
            content = "Success : new CommandProduct can be found at /api/CommandProduct/" + CommandProductId.ToString();

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
        CommandProduct CommandProduct = new CommandProduct();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // GET by id

            // the CommandProduct as a string with correct id
            string commandString = "SELECT * FROM commands_products WHERE CommandProductId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                CommandProduct = new CommandProduct
                {
                    CommandProductId = reader.GetInt32(0),
                    CommandId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in CommandProduct  (id:"+id.ToString()+")");
                }
                string jsoncommands_products = JsonSerializer.Serialize(CommandProduct);
                content = jsoncommands_products;
            } else {
                content = "";
            }
            reader.Close(); 
            if (content == "") {throw new Exception("No CommandProduct corresponding to Id");}

            // PUT
            CommandProduct? JsonInfo  = JsonSerializer.Deserialize<CommandProduct>(new StreamReader(request.InputStream, Encoding.UTF8).ReadToEnd());
            // if it is null ; body couldn't be deserialized into CommandProduct. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in CommandProduct \n");
                content = "Error : Wrong body.\n";
                statusCode = 400;
                throw new Exception("Wrong body in PUT CommandProduct");
            }
            if (JsonInfo.CommandId is not 0) { CommandProduct.CommandId = JsonInfo.CommandId;}
            if (JsonInfo.ProductId is not 0) { CommandProduct.ProductId = JsonInfo.ProductId;}

            // UPDATE
            string commandStringInsert = "UPDATE commands_products SET CommandId = @CommandId, ProductId = @ProductId WHERE CommandProductId = @CommandProductId;";
            MySqlCommand commandInsert = new MySqlCommand(commandStringInsert, connection, transaction);
            commandInsert.Parameters.AddWithValue("@CommandProductId", CommandProduct.CommandProductId);
            commandInsert.Parameters.AddWithValue("@CommandId", JsonInfo.CommandId);
            commandInsert.Parameters.AddWithValue("@ProductId", JsonInfo.ProductId);
            commandInsert.ExecuteNonQuery();
            content = "Success : new CommandProduct updated " + id.ToString();

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

