using System.Net;

namespace Controllers;
class AddressController : Controller
{
    public override void GetRequest(HttpListenerResponse response, string content = "", int statusCode = 501) {

        // change content and statusCode here
        
        base.GetRequest(response, content, statusCode); // base keyword : like super() in java, call the parent class
    }


}