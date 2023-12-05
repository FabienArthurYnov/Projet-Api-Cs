using System;
using System.Net;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;

class ApiServer
{
    static async Task Main(string[] args)
    {
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
        
        // we're not gonna handle it if it's not /api. If we want a front app, maybe later
        if (pathList[0] != "api") {
            SendResponse(response, "Api is accessible at http://localhost:8080/api", 200);
            return;
        }
        

        // get the correct Controller
        
        string responseString = "Not yet implemented";
        int statusCode = 500;
        switch (request.HttpMethod) {
            case "GET":
                Console.WriteLine("Get");
                statusCode = 200;
                break;
            case "POST":
                statusCode = 501;
                break;
            case "DELETE":
                statusCode = 501;
                break;
            case "PUT":
                statusCode = 501;
                break;
            default:
                Console.WriteLine("Unknown method : ", request.HttpMethod);   
                statusCode = 501;
                break;
        }

        SendResponse(response, responseString, statusCode);
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