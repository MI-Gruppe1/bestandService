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
        //TODO durch port 6000 ersetzen
        private const string AllStations = "http://localhost:6000/allStations";
        private const string StadtRadUrl = "http://stadtrad.hamburg.de/kundenbuchung/hal2ajax_process.php";

        // in development mode the service is being run with mock data
        private const bool Development = true;

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

            string stadtRadInfo = "";
            var radDbInfo = "";

            if (Development)
            {
                // if Development read information from file
                stadtRadInfo = ReadStadtRadResponseFromFile();

                radDbInfo = ReadStationsFromFile();
            }
            else
            {
                stadtRadInfo = DownloadStadtRadInformation();
                if (stadtRadInfo == null)
                {
                    throw new HttpRequestException("Stadtrad API not reachable");
                }

                radDbInfo = DownloadAllStations();
                if (radDbInfo == null)
                {
                    radDbInfo = ReadStationsFromFile();
                }
            }

            var stadtradParser = new StadtradParser();
            var allStations = stadtradParser.GetAllStations(stadtRadInfo, radDbInfo);
            return JsonConvert.SerializeObject(allStations);

        }


        [HttpGet("{*stationName}", Name = "GetStation")]
        public string GetStation(string stationName)
        {
            var receivedInfos = "";

            if (Development)
            {
                // if Development read information from file
                receivedInfos = ReadStadtRadResponseFromFile();
            }
            else
            {
                receivedInfos = DownloadStadtRadInformation();
                if (receivedInfos == null)
                {
                    throw new HttpRequestException("Stadtrad API not reachable");
                }
            }

            var stadtradParser = new StadtradParser();
            var stationInfo = stadtradParser.GetInfoForOneStation(receivedInfos, stationName);
            if (stationInfo != null)
                return JsonConvert.SerializeObject(stationInfo);
            else
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