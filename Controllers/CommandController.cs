using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using Api.Models;
using MySql.Data.MySqlClient;

namespace Controllers;
class CommandController : Controller
{
    public override void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
        // status ok if not overriden by unexpected events
        statusCode = 200;

        // will hold the commands
        List<Command> commands = new List<Command>();

        // open a new connection
        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        // opening the transaction, and doing it
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // create a new command and the reader for its output
            MySqlCommand command = new MySqlCommand("SELECT * FROM commands;", connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) { // in case it's empty, there is nothing to read
                while (reader.Read()) {
                    // read the new Command
                    Command Command = new Command
                    {
                        CommandId = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        Price = reader.GetFloat(2),
                        StatusProduct = reader.GetInt32(3)
                    };
                    // add the new Command to the list
                    commands.Add(Command);
                }
                // to json and change content
                string jsoncommands = JsonSerializer.Serialize(commands);
                content = jsoncommands;
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
        Command Command = new Command();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // the command as a string with correct id
            string commandString = "SELECT * FROM commands WHERE CommandId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                Command = new Command
                {
                    CommandId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Price = reader.GetFloat(2),
                    StatusProduct = reader.GetInt32(3)
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in Command  (id:"+id.ToString()+")");
                }
                string jsoncommands = JsonSerializer.Serialize(Command);
                content = jsoncommands;
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
            // the command as a string with correct id
            string commandString = "DELETE FROM commands WHERE CommandId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            int nbOfAffectedRows = command.ExecuteNonQuery();
            if (nbOfAffectedRows == 0) {
                content = "No corresponding Command of id " + id.ToString();
            } else if (nbOfAffectedRows == 1) {
                content = "Success : deleted Command of id " + id.ToString();
            } else {
                content = "Warning : multiple deleted Command of id " + id.ToString();
                Console.WriteLine("Warning : multiple deleted Command with id " + id.ToString());
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

        // Deserialize JSON content to Command object
        StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
        string body = reader.ReadToEnd();
        Console.WriteLine(body);

        

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();

        MySqlTransaction transaction = connection.BeginTransaction();
        try
        {
            Command? JsonInfo  = JsonSerializer.Deserialize<Command>(body);
            // if it is null ; body couldn't be deserialized into Command. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in Command ; \n   " + body);
                content = "Error : Wrong body.\n" + body;
                statusCode = 400;
                throw new Exception();
            }

            string commandString = "INSERT INTO commands (UserId, Price, StatusProduct) VALUES (@UserId, @Price, @StatusProduct);SELECT LAST_INSERT_ID();";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            command.Parameters.AddWithValue("@UserId", JsonInfo.UserId);
            command.Parameters.AddWithValue("@Price", JsonInfo.Price);
            command.Parameters.AddWithValue("@StatusProduct", JsonInfo.StatusProduct);

            // get the id
            int CommandId = Convert.ToInt32(command.ExecuteScalar());
            content = "Success : new Command can be found at /api/Command/" + CommandId.ToString();

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
        Command Command = new Command();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // GET by id

            // the command as a string with correct id
            string commandString = "SELECT * FROM commands WHERE CommandId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                Command = new Command
                {
                    CommandId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Price = reader.GetFloat(2),
                    StatusProduct = reader.GetInt32(3)
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in Command  (id:"+id.ToString()+")");
                }
                string jsoncommands = JsonSerializer.Serialize(Command);
                content = jsoncommands;
            } else {
                content = "";
            }
            reader.Close(); 
            if (content == "") {throw new Exception("No Command corresponding to Id");}

            // PUT
            Command? JsonInfo  = JsonSerializer.Deserialize<Command>(new StreamReader(request.InputStream, Encoding.UTF8).ReadToEnd());
            // if it is null ; body couldn't be deserialized into Command. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in Command \n");
                content = "Error : Wrong body.\n";
                statusCode = 400;
                throw new Exception("Wrong body in PUT Command");
            }
            if (JsonInfo.UserId is not 0) { Command.UserId = JsonInfo.UserId;}
            if (JsonInfo.Price is not 0) { Command.Price = JsonInfo.Price;}
            if (JsonInfo.StatusProduct is not 0) { Command.StatusProduct = JsonInfo.StatusProduct;}

            // UPDATE
            string commandStringInsert = "UPDATE commands SET UserId = @UserId, Price = @Price, StatusProduct = @StatusProduct WHERE CommandId = @CommandId;";
            MySqlCommand commandInsert = new MySqlCommand(commandStringInsert, connection, transaction);
            commandInsert.Parameters.AddWithValue("@CommandId", Command.CommandId);
            commandInsert.Parameters.AddWithValue("@UserId", JsonInfo.UserId);
            commandInsert.Parameters.AddWithValue("@Price", JsonInfo.Price);
            commandInsert.Parameters.AddWithValue("@StatusProduct", JsonInfo.StatusProduct);
            commandInsert.ExecuteNonQuery();
            content = "Success : new Command updated " + id.ToString();

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

