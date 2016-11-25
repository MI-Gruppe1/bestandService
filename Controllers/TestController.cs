using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BestandService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

namespace BestandService.Controllers
{
    [Route("[controller]")]
    public class TestController : Controller
    {


        /// <summary>
        /// Get a list of all known stations including
        /// the position, name and stock of the stations
        /// </summary>
        /// <returns>List of all bike stations</returns>
        [HttpGet]
        public string GetAll()
        {
            const string bestand = "http://localhost:5000/bestand";
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(bestand).Result;

                if (response.IsSuccessStatusCode)
                {
                    // by calling .Result you are performing a synchronous call
                    var responseContent = response.Content;

                    // by calling .Result you are synchronously reading the result
                    var responseString = responseContent.ReadAsStringAsync().Result;

                    Console.WriteLine(responseString);

                    return responseString;
                }
                else
                {
                    return null;
                }
            }
        }

    }
}