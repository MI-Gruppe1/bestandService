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
    [Route("/allStations")]
    public class DummyDbService : Controller
    {
        [HttpGet]
        public string GetAll()
        {
            var stations = System.IO.File.ReadAllText("/Users/julius/Development/bestandService/andi.json");
            return stations;
        }
    }
}