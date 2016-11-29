using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BestandService.Controllers
{
    [Route("[controller]")]
    public class BestandController : Controller
    {
        //TODO durch port 4567 ersetzen
        private const string AllStations = "http://localhost:4567/allStations";
        private const string StadtRadUrl = "http://stadtrad.hamburg.de/kundenbuchung/hal2ajax_process.php";

        // in development mode the service is being run with mock data
        private const bool Development = false;

        // list of the known stations
        private JArray _knownStations;

        public BestandController()
        {
            if (Development)
            {
                var stationsFromFile = ReadStationsFromFile();
                _knownStations = JArray.Parse(stationsFromFile);
            }
            else
            {
                var downloadResponse = DownloadAllStations();
                while (downloadResponse == null)
                {
                    Console.WriteLine("sleeping");
                    System.Threading.Thread.Sleep(10000);
                    downloadResponse = DownloadAllStations();
                }
                _knownStations = JArray.Parse(downloadResponse);
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
                var responseFromFile = ReadStadtRadResponseFromFile();
                stadtRadInformation = JObject.Parse(responseFromFile);

                var stationsFromFile = ReadStationsFromFile();
                radDBInformation = JArray.Parse(stationsFromFile);
            }
            else
            {
                var downloadedStadtRadInfos = DownloadStadtRadInformation();
                if (downloadedStadtRadInfos == null)
                {
                    throw new HttpRequestException("Stadtrad API not reachable");
                }
                stadtRadInformation = JObject.Parse(downloadedStadtRadInfos);


                var downloadedStations = DownloadAllStations();
                if (downloadedStations == null)
                {
                    downloadedStations = ReadStationsFromFile();
                }
                radDBInformation = JArray.Parse(downloadedStations);
            }


            var markers = (JArray) stadtRadInformation["marker"];
            foreach (var item in markers.Children())
            {
                // get properties
                var itemProperties = item.Children<JProperty>();

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


        [HttpGet("{*stationName}", Name = "GetStation")]
        public string GetStation(string stationName)
        {
            JToken stadtRadInformation = new JArray();
            JToken radDBInformation = new JArray();

            if (Development)
            {
                // if Development read information from file
                var responseFromFile = ReadStadtRadResponseFromFile();
                stadtRadInformation = JObject.Parse(responseFromFile);
            }
            else
            {
                var downloadedStadtRadInfos = DownloadStadtRadInformation();
                if (downloadedStadtRadInfos == null)
                {
                    throw new HttpRequestException("Stadtrad API not reachable");
                }
                stadtRadInformation = JObject.Parse(downloadedStadtRadInfos);
            }


            var markers = (JArray) stadtRadInformation["marker"];
            JObject resp = new JObject();
            foreach (var item in markers.Children())
            {
                // get properties
                var itemProperties = item.Children<JProperty>();

                var hal2OptionProp = itemProperties.FirstOrDefault(x => x.Name == "hal2option");
                var hal2Option = hal2OptionProp.Value;

                var tooltipValue = (string) hal2Option["tooltip"];
                var name = tooltipValue.Substring(1, 4);

                var requestedName = stationName.Substring(0, 4);

                if (name == requestedName)
                {
                    var bikeCount = hal2Option["bikelist"].Count();


                    resp["name"] = stationName;
                    var latitudeProp = itemProperties.FirstOrDefault(x => x.Name == "lat");
                    resp["latitude"] = latitudeProp.Value;
                    var longitudeProp = itemProperties.FirstOrDefault(x => x.Name == "lng");
                    resp["longitude"] = latitudeProp.Value;
                    resp["bikes"] = bikeCount;

                    return JsonConvert.SerializeObject(resp);
                }
            }
            return null;
        }

        #region "private methods"

        private static string DownloadStadtRadInformation()
        {
            // needed information for the stadtRad rest api
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
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    return null;
                }
            }
        }

        private static string DownloadAllStations()
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(AllStations).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    return responseContent.ReadAsStringAsync().Result;
                }
            }
            return null;
        }

        private static string ReadStationsFromFile()
        {
            return System.IO.File.ReadAllText("andi.json");
        }

        private static string ReadStadtRadResponseFromFile()
        {
            return System.IO.File.ReadAllText("stadtRadSample.json");
        }

        #endregion
    }
}