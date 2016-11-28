using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BestandService.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BestandService.Controllers
{
    [Route("[controller]")]
    public class BestandController : Controller
    {
        // safes all station for now
        private readonly IStationRepository _stationRepository;


        public BestandController(IStationRepository stationRepository)
        {
            this._stationRepository = stationRepository;
        }


        [HttpGet]
        public void GetAll()
        {
            bool development = true;

            JToken stadtRadInformation = new JArray();
            JToken radDBInformation = new JArray();

            if (development)
            {
                // if development read information from file
                var fileContent =
                    System.IO.File.ReadAllText("/Users/julius/Development/bestandService/stadtRadSample.json");
                stadtRadInformation = JObject.Parse(fileContent);

                var andi = System.IO.File.ReadAllText("/Users/julius/Development/bestandService/andi.json");
                radDBInformation = JArray.Parse(andi);
            }
            else
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("mapstadt_id", "75"),
                    new KeyValuePair<string, string>("ajxmod", "hal2map"),
                    new KeyValuePair<string, string>("callee", "getMarker"),
                });

                const string stadtRadUrl = "http://stadtrad.hamburg.de/kundenbuchung/hal2ajax_process.php";
                using (var client = new HttpClient())
                {
                    var response = client.PostAsync(stadtRadUrl, formContent).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var stringContent = response.Content.ReadAsStringAsync().Result;
                        stadtRadInformation = JObject.Parse(stringContent);

                        //TODO ersetzen durch echten call
                        var andi = System.IO.File.ReadAllText("/Users/julius/Development/bestandService/andi.json");
                        radDBInformation = JArray.Parse(andi);
                    }
                    else
                    {
                        //return null;
                    }
                }
            }


            var markers = (JArray) stadtRadInformation["marker"];
            foreach (var item in markers.Children())
            {
                // get properties
                var itemProperties = item.Children<JProperty>();

                var latidudeProp = itemProperties.FirstOrDefault(x => x.Name == "lat");
                var latitude = latidudeProp.Value;

                var longitudeProp = itemProperties.FirstOrDefault(x => x.Name == "lng");
                var longitude = longitudeProp.Value;

                var hal2OptionProp = itemProperties.FirstOrDefault(x => x.Name == "hal2option");
                var hal2Option = hal2OptionProp.Value;

                var bikeCount = hal2Option["bikelist"].Count();

                var tooltipValue = (string) hal2Option["tooltip"];
                var name = tooltipValue.Substring(1, 4);

                foreach (var inf in radDBInformation)
                {
                    var fullName = (string) inf["name"];
                    if (fullName.StartsWith(name))
                    {
                        //Console.WriteLine("found a match");
                        inf["bikes"] = bikeCount;
                    }
                }
            }

            if (development)
            {
                string json = JsonConvert.SerializeObject(radDBInformation.ToArray());
                System.IO.File.WriteAllText("/Users/julius/Development/bestandService/output.json", json);
            }
        }


        [HttpGet("{stationName}", Name = "GetStation")]
        public string GetStation(string stationName)
        {
            Console.WriteLine("station name: " + stationName);

            return stationName;
        }


        [HttpPost]
        public IActionResult AddStation([FromBody] Station station)
        {
            if (station == null)
            {
                return BadRequest();
            }
            _stationRepository.Add(station);

            // the quoted "GetStation" refers to the GET Method with exactly that name
            // this return doesnt work if the name doesnt match that!!!
            // Result of this: the ID of the station is returned so that on can
            // retrieve further Information about the station
            return CreatedAtRoute("GetStation", new {stationID = station.Key}, station);
        }
    }
}