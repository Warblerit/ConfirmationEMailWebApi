
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using ConfirmationEMailWebApi.Models;
using System.Web.Http;
//using System.Web.Script.Serialization;

namespace ConfirmationEMailWebApi.Controllers
{
    [RoutePrefix("API/UrlShortner")]
    public class URLShortnerController : ApiController
    {
        //[HttpPost]
        //[Route("urlshort")]
        //public string urlshort(URLShort URL)
        //{

        //    string FinalURL = "";
        //    WebClient client = new WebClient();
        //    client.Headers.Add("Content-Type", "application/json");
        //    string RR = "https://hbhub.in?link=" + URL.longUrl.ToString();
        //    string body = "{\"longDynamicLink\":\"" + RR + "\",\"suffix\":{\"option\":\"SHORT\"}}";

        //    try
        //    {
        //        string response = client.UploadString("https://firebasedynamiclinks.googleapis.com/v1/shortLinks?key= AIzaSyD9CW6NlkMj5tmpDjQSfHBDiTPRUqbmc3o", "POST", body);
        //        var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(response);
        //        string[] result = dict.Select(kv => kv.Value.ToString()).ToArray();
        //        FinalURL = result[0].Replace("\"", "").ToString();

        //        return FinalURL;
        //    }
        //    catch (Exception ex)
        //    {
                
               
        //        return FinalURL;
        //    }

        //}
        //[HttpGet]
        //[Route("Test")]
        //public string Test()
        //{
        //    string RR1 = "";
        //    WebClient client = new WebClient();
        //    client.Headers.Add("Content-Type", "application/json");
        //    string RR = "https://hbnest.in?link=https://gmail.com";
        //    string body = "{\"dynamicurl\":\"" + RR + "\"}";

        //    try
        //    {
        //        string response = client.UploadString("http://localhost:4199/API/UrlShortner/urlshort", "POST", body);

        //    }
        //    catch (Exception ex)
        //    {
        //        //request failed
        //    }
        //    return RR1;
        //}
    }
}
