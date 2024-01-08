using System;
using System.Net;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Controllers;
using Microsoft.AspNetCore.Http.HttpResults;

class ApiServer
{
    public static string ConnectionString = "Server=localhost;Port=3306;Database=ecommerce;User Id=root;Password=azerty;";

    static async Task Main(string[] args)
    {
        if (File.Exists("./connectionString.txt")) {
            ConnectionString = File.ReadAllText("./connectionString.txt");
        }
        
        // Define the base URL and port for the server
        string url = "http://localhost:8080/";

        // Create an HttpListener and add the prefixes
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(url);

        // Start the listener
        listener.Start();
        Console.WriteLine($"Listening on {url}...");


        // Handle incoming requests
        while (true)
        {
            // Wait for a request to be processed
            HttpListenerContext context = await listener.GetContextAsync();

            // Make multiple thread for the requests
            ThreadPool.QueueUserWorkItem((_) => HandleRequest(context));
        }
    }

    static void HandleRequest(HttpListenerContext context)
    {
        // Extract the request information
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        // Log the request details
        Console.WriteLine($"Request received: {request.HttpMethod} {request.Url}");

        // check if url is null, which would be weird
        if (request.Url is null) {
            Console.WriteLine("Error : request.Url is NULL");
            SendResponse(response, "500 : Internal Error", 500);
            return;
        }
        string path = request.Url.ToString();
        string[] pathList = path.Split('/').Skip(3).ToArray();  // split the url in /, and remove the three first item (the http://[website] part), so we have the usefull parts
        
        // we're not gonna handle it if it's not /api. If we want a front app, maybe later.
        // we are also checking the format of the url to prevent further errors
        if (pathList[0] != "api") {
            SendResponse(response, "Api is accessible at http://localhost:8080/api", 200);
            return;
        } else if (pathList.Length < 2) {
            SendResponse(response, "Bad request (400) : the collection of data needs to be specified", 400);
            return;
        }
        

        // get the correct Controller
        Controller controller;
        switch (pathList[1]) {
            case "address" :
                controller = new AddressController();
                break;
            case "user" :
                controller = new UserController();
                break;
            case "poduct" :
                controller = new ProductControllers();
                break;
            case "cart" :
                controller = new CartController();
                break;
            default:
                SendResponse(response, "400 : Bad Request", 400);
                Console.WriteLine(DateTime.Now.ToString(),  " : Bad Request " + 400 + "(" + pathList[1] + " : no such controller)");
                return;
        }
        
        // handle the correct method to the controller
        int statusCode = 500;
        string content = "Not yet Implemented";
        switch (request.HttpMethod) {
            case "GET":
                if (pathList.Length == 2) {  // api/[table]   ; it's a get of a whole table
                    controller.GetRequest(response);
                } else {  // api/[table]/[id]  ; get by id
                    controller.GetByIdRequest(response, int.Parse(pathList[2]));
                }
                return;
            case "POST":
                controller.PostRequest(response, request);
                return;
            case "DELETE":
                if (pathList.Length == 2) {  // api/[table]   ;
                    SendResponse(response, "Need to specify Id", 400);
                } else {  // api/[table]/[id]  ; get by id
                    controller.DeleteRequest(response, int.Parse(pathList[2]));
                }
                return;
            case "PUT":
                controller.PutRequest(response, int.Parse(pathList[2]), request);
                return;
            default:
                Console.WriteLine("Unknown method : ", request.HttpMethod);   
                statusCode = 400;
                content = "Bad request : Unknown method";
                break;
        }

        // If the request was found, nothing gets here. Only happens if request is unhandled.
        SendResponse(response, content, statusCode);
    }

    public static void SendResponse(HttpListenerResponse response, string content, int statusCode = 200)
    {
        // make the response and send it to outputStream
        response.StatusCode = statusCode;
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);

        // Close the response stream
        response.Close();
    }
}