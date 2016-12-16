using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BestandService.Controllers
{
    [Route("/bestandUndVorhersage")]
    public class PredictionController : Controller
    {
        private const string AllStations = "http://localhost:3000";
        private const string StadtRadUrl = "http://stadtrad.hamburg.de/kundenbuchung/hal2ajax_process.php";

        // in development mode the service is being run with mock data
        private const bool Development = true;

        [HttpPost]
        public string GetAll()
        {
            var reqBody = new StreamReader(Request.Body).ReadToEnd();
            var stations = JArray.Parse(reqBody);
            foreach (var station in stations)
            {
                var stationName = (string) station.SelectToken("name");
                Console.WriteLine(stationName);
            }
            return (string)stations.SelectToken("[0].name");
        }
    }
}