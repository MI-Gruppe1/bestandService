using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BestandService.Controllers
{
    [Route("/prediction")]
    public class DummyPredictionService : Controller
    {
        Random rand = new Random();

        [HttpGet("{*stationName}", Name = "getPrediction")]
        public string getPrediction(string stationName)
        {
            var gimmeAnumber = rand.Next(0, 10);
            return gimmeAnumber.ToString();
        }
    }
}