using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BestandService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace BestandService.Controllers
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        public string GetAll()
        {
            Console.WriteLine("get auf test controller");
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mapstadt_id","75"),
                new KeyValuePair<string, string>("ajxmod","hal2map"),
                new KeyValuePair<string, string>("callee","getMarker"),
            });

            const string stadtRadUrl = "http://stadtrad.hamburg.de/kundenbuchung/hal2ajax_process.php";
            using (var client = new HttpClient())
            {
                var response =  client.PostAsync(stadtRadUrl, formContent).Result;

                if (response.IsSuccessStatusCode)
                {
                    var stringContent = response.Content.ReadAsStringAsync().Result;

                    //Console.WriteLine(stringContent);
                    JToken token = JObject.Parse(stringContent);

                    JArray markers = (JArray) token["marker"];

                    //var myTest = (string)token.SelectToken("marker");

                    return markers.Count.ToString();
                }
                else
                {
                    return null;
                }
            }
        }



//        [HttpGet]
//        public string GetAll()
//        {
//            const string bestand = "http://localhost:5000/bestand";
//            using (var client = new HttpClient())
//            {
//                var response = client.GetAsync(bestand).Result;
//
//                if (response.IsSuccessStatusCode)
//                {
//                    // by calling .Result you are performing a synchronous call
//                    var responseContent = response.Content;
//
//                    // by calling .Result you are synchronously reading the result
//                    var responseString = responseContent.ReadAsStringAsync().Result;
//
//                    Console.WriteLine(responseString);
//
//                    return responseString;
//                }
//                else
//                {
//                    return null;
//                }
//            }
//        }

    }
}