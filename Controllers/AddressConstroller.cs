using System.Net;
using System.Text.Json;
using Api.Models;
using MySql.Data.MySqlClient;

namespace Controllers;
class AddressController : Controller
{
    public override void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
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
        }


        // to json
        string jsonAddresses = JsonSerializer.Serialize(addresses);
        Console.WriteLine(jsonAddresses);

        // change content and statusCode here
        statusCode = 200;
        content = jsonAddresses;

        base.GetRequest(response, content, statusCode); // base keyword : like super() in java, call the parent class
    }


}