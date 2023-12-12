using System.Net;
using System.Text;
using System.Text.Json;
using Api.Models;
using MySql.Data.MySqlClient;

namespace Controllers;
class AddressController : Controller
{
    public override void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
        // status ok if not overriden by unexpected events
        statusCode = 200;

        // will hold the addresses
        List<Address> addresses = new List<Address>();

        // open a new connection
        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        // opening the transaction, and doing it
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // create a new command and the reader for its output
            MySqlCommand command = new MySqlCommand("SELECT * FROM Addresses;", connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) { // in case it's empty, there is nothing to read
                while (reader.Read()) {
                    // read the new address
                    Address address = new Address
                    {
                        AddressId = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        AddressString = reader.GetString(2)
                    };
                    // add the new address to the list
                    addresses.Add(address);
                }
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


        // to json
        string jsonAddresses = JsonSerializer.Serialize(addresses);

        // change content
        content = jsonAddresses;

        base.GetRequest(response, content, statusCode); // base keyword : like super() in java, call the parent class
    }


    public override void GetByIdRequest(HttpListenerResponse response, int id, string content = "", int statusCode = 501) {
        statusCode = 200;
        Address address = new Address();

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();
        
        MySqlTransaction transaction = connection.BeginTransaction();
        try {
            // the command as a string with correct id
            string commandString = "SELECT * FROM Addresses WHERE AddressId = " + id.ToString() + ";";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                address = new Address
                {
                    AddressId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    AddressString = reader.GetString(2)
                };

                //check if there is another row
                if (reader.Read()) {
                    Console.WriteLine("Warning ; multiple result on a GET by Id in Address  (id:"+id.ToString()+")");
                }
            }
            reader.Close(); 
            transaction.Commit();
        } catch (Exception e) {
            Console.WriteLine("Error : " + e.Message);
            transaction.Rollback();
            Console.WriteLine("Transaction rolled back");
            statusCode = 500;
        }

        string jsonAddresses = JsonSerializer.Serialize(address);

        content = jsonAddresses;

        base.GetRequest(response, content, statusCode);
    }
    public override void PostRequest(HttpListenerResponse response, HttpListenerRequest request, string content = "Unimplemented", int statusCode = 501) {
    {
        statusCode = 201;  // Created status code for successful POST

        // Deserialize JSON content to Address object
        StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
        string body = reader.ReadToEnd();
        Console.WriteLine(body);

        

        MySqlConnection connection = new MySqlConnection(ApiServer.ConnectionString);
        connection.Open();

        MySqlTransaction transaction = connection.BeginTransaction();
        try
        {
            // Insert new address into the database
            string commandString = "INSERT INTO Addresses (UserId, AddressString) VALUES (@UserId, @AddressString);";
            MySqlCommand command = new MySqlCommand(commandString, connection, transaction);
            // command.Parameters.AddWithValue("@UserId", newAddress.UserId);
            // command.Parameters.AddWithValue("@AddressString", newAddress.AddressString);

            command.ExecuteNonQuery();

            // Commit the transaction
            transaction.Commit();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            transaction.Rollback();
            Console.WriteLine("Transaction rolled back");
            statusCode = 500;  // Internal Server Error status code
        }
        finally
        {
            connection.Close();
        }

        base.PostRequest(response, request, content, statusCode);
    }
}
}
