using System.Net;

namespace Controllers;
class Controller
{
    // virtual vs abstract ; virtual has a default implementation, abstract doesn't.
    // GET
    public virtual void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
        Console.WriteLine(DateTime.Now.ToString(),  " : Get Request " + statusCode);
        ApiServer.SendResponse(response, content, statusCode);
    }

    // GET by Id
    public virtual void GetByIdRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {
        Console.WriteLine(DateTime.Now.ToString(),  " : Get by Id Request " + statusCode);
        ApiServer.SendResponse(response, content, statusCode);
    }

    // POST
    public virtual void PostRequest(HttpListenerResponse response, HttpListenerRequest request, string content = "Unimplemented", int statusCode = 501) {
        Console.WriteLine(DateTime.Now.ToString(),  " : Post Request " + statusCode);
        ApiServer.SendResponse(response, content, statusCode);
    }

    // DELETE
    public virtual void DeleteRequest(HttpListenerResponse response, string content = "Unimplemented", int statusCode = 501) {
        Console.WriteLine(DateTime.Now.ToString(),  " : Delete Request " + statusCode);
        ApiServer.SendResponse(response, content, statusCode);
    }

    // PUT
    public virtual void PutRequest(HttpListenerResponse response, HttpListenerRequest request, string content = "Unimplemented", int statusCode = 501) {
        Console.WriteLine(DateTime.Now.ToString(),  " : Put Request " + statusCode);
        ApiServer.SendResponse(response, content, statusCode);
    }

}