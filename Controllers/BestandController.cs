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
    [Route("[controller]")]
    public class BestandController : Controller
    {
        private const bool Development = true;

        private JArray _knownStations = new JArray();

        //TODO durch port 4567 ersetzen
        private const string AllStations = "http://localhost:5000/allStations";
        private const string StadtRadUrl = "http://stadtrad.hamburg.de/kundenbuchung/hal2ajax_process.php";

        public BestandController()
        {
            if (Development)
            {
                var stationsFromFile = System.IO.File.ReadAllText("/Users/julius/Development/bestandService/andi.json");
                _knownStations = JArray.Parse(stationsFromFile);
            }
            else
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(AllStations).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        // by calling .Result you are performing a synchronous call
                        var responseContent = response.Content;

                        // by calling .Result you are synchronously reading the result
                        var responseString = responseContent.ReadAsStringAsync().Result;

                        _knownStations = JArray.Parse(responseString);
                    }
                }
            }
        }


        [HttpGet]
        public string GetAll()
        {
            JToken stadtRadInformation = new JArray();
            JToken radDBInformation = new JArray();

            if (Development)
            {
                // if Development read information from file
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

                using (var client = new HttpClient())
                {
                    var response = client.PostAsync(StadtRadUrl, formContent).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var stringContent = response.Content.ReadAsStringAsync().Result;
                        stadtRadInformation = JObject.Parse(stringContent);
                    }
                    else
                    {
                        //return null;
                    }
                }

                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(AllStations).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        // by calling .Result you are performing a synchronous call
                        var responseContent = response.Content;

                        // by calling .Result you are synchronously reading the result
                        var responseString = responseContent.ReadAsStringAsync().Result;

                        radDBInformation = JArray.Parse(responseString);
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
                        inf["bikes"] = bikeCount;
                    }
                }
            }
            return JsonConvert.SerializeObject(radDBInformation);
        }


        [HttpGet("{stationName}", Name = "GetStation")]
        public string GetStation(string stationName)
        {
            Console.WriteLine("station name: " + stationName);

            return stationName;
        }
    }
}