using System;
using System.Net;
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

        // HERE MAKE THE RESPONSE and put it in responseString
        string responseString = "Hello, World!";
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

    static void SendResponse(HttpListenerResponse response, string content, int statusCode = 200)
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