using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;
using Api.Models;
using MySql.Data.MySqlClient;

namespace Controllers;
class UserController : Controller
{
    public override void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
        // status ok if not overriden by unexpected events
        statusCode = 200;

        // will hold the users
        List<User> users = new List<User>();

        // open a new connection
        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        // opening the transaction, and doing it
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // create a new command and the reader for its output
            MySqlCommand command = new MySqlCommand("SELECT * FROM users;", connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) { // in case it's empty, there is nothing to read
                while (reader.Read()) {
                    // read the new user
                    User user = new User
                    {
                        UserId = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        Password = reader.GetString(3),
                        Email = reader.GetString(4)
                    };
                    // add the new user to the list
                    users.Add(user);
                }
                // to json and change content
                string jsonusers = JsonSerializer.Serialize(users);
                content = jsonusers;
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
        User user = new User();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // the command as a string with correct id
            string commandString = "SELECT * FROM users WHERE UserId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                user = new User
                {
                    UserId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Password = reader.GetString(3),
                    Email = reader.GetString(4)
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in user  (id:"+id.ToString()+")");
                }
                string jsonusers = JsonSerializer.Serialize(user);
                content = jsonusers;
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
            string commandString = "DELETE FROM users WHERE UserId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            int nbOfAffectedRows = command.ExecuteNonQuery();
            if (nbOfAffectedRows == 0) {
                content = "No corresponding user of id " + id.ToString();
            } else if (nbOfAffectedRows == 1) {
                content = "Success : deleted user of id " + id.ToString();
            } else {
                content = "Warning : multiple deleted user of id " + id.ToString();
                Console.WriteLine("Warning : multiple deleted user with id " + id.ToString());
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

        // Deserialize JSON content to user object
        StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
        string body = reader.ReadToEnd();
        Console.WriteLine(body);

        

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();

        MySqlTransaction transaction = connection.BeginTransaction();
        try
        {
            User? JsonInfo  = JsonSerializer.Deserialize<User>(body);
            // if it is null ; body couldn't be deserialized into user. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in user ; \n   " + body);
                content = "Error : Wrong body.\n" + body;
                statusCode = 400;
                throw new Exception();
            }

            string commandString = "INSERT INTO users (FirstName, LastName, Password, Email) VALUES (@FirstName, @LastName, @Password, @Email);SELECT LAST_INSERT_ID();";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            command.Parameters.AddWithValue("@FirstName", JsonInfo.FirstName);
            command.Parameters.AddWithValue("@LastName", JsonInfo.LastName);
            command.Parameters.AddWithValue("@Password", JsonInfo.Password);
            command.Parameters.AddWithValue("@Email", JsonInfo.Email);

            // get the id
            int userId = Convert.ToInt32(command.ExecuteScalar());
            content = "Success : new user can be found at /api/user/" + userId.ToString();

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
        User user = new User();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // GET by id

            // the command as a string with correct id
            string commandString = "SELECT * FROM users WHERE UserId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                user = new User
                {
                    UserId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Password = reader.GetString(3),
                    Email = reader.GetString(4)
                };
                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in user  (id:"+id.ToString()+")");
                }
                string jsonusers = JsonSerializer.Serialize(user);
                content = jsonusers;
            } else {
                content = "";
            }
            reader.Close(); 
            if (content == "") {throw new Exception("No user corresponding to Id");}

            // PUT
            User? JsonInfo  = JsonSerializer.Deserialize<User>(new StreamReader(request.InputStream, Encoding.UTF8).ReadToEnd());
            // if it is null ; body couldn't be deserialized into User. Most likely ; wrong body
            if (JsonInfo is null) {
                Console.WriteLine("Error : Wrong body in user \n");
                content = "Error : Wrong body.\n";
                statusCode = 400;
                throw new Exception("Wrong body in PUT user");
            }
            if (JsonInfo.FirstName is not null) { user.FirstName = JsonInfo.FirstName;}
            if (JsonInfo.LastName is not null) { user.LastName = JsonInfo.LastName;}
            if (JsonInfo.Password is not null) { user.Password = JsonInfo.Password;}
            if (JsonInfo.Email is not null) { user.Email = JsonInfo.Email;}

            // UPDATE
            string commandStringInsert = "UPDATE users SET FirstName = @FirstName, LastName = @LastName, Password = @Password, Email = @Email WHERE UserId = @UserId;";
            MySqlCommand commandInsert = new MySqlCommand(commandStringInsert, connection, transaction);
            commandInsert.Parameters.AddWithValue("@UserId", user.UserId);
            commandInsert.Parameters.AddWithValue("@FirstName", JsonInfo.FirstName);
            commandInsert.Parameters.AddWithValue("@LastName", JsonInfo.LastName);
            commandInsert.Parameters.AddWithValue("@Password", JsonInfo.Password);
            commandInsert.Parameters.AddWithValue("@Email", JsonInfo.Email);
            commandInsert.ExecuteNonQuery();
            content = "Success : new user updated " + id.ToString();

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

