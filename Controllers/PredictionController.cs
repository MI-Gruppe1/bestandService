using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        // in development mode the service is being run with mock data
        private const bool Development = false;
        private const string dummyPrediction = "http://localhost:5000/prediction";
        private string prediction = "http://localhost:3000/predictionService";

        [HttpPost]
        public string GetAll()
        {
            var radInfoDownloader = new RadInfoDownloader();
            var receivedInfos = "";

            if (Development)
            {
                // if Development read information from file
                receivedInfos = radInfoDownloader.ReadStadtRadResponseFromFile();
            }
            else
            {
        ///////////////
        //Quickfix
        ////////////////
                receivedInfos = radInfoDownloader.ReadStadtRadResponseFromFile();
//                receivedInfos = radInfoDownloader.DownloadStadtRadInformation();
//                if (receivedInfos == null)
//                {
//                    throw new HttpRequestException("Stadtrad API not reachable");
//                }
            }

            var stadtradParser = new StadtradParser();

            var reqBody = new StreamReader(Request.Body).ReadToEnd();
            var stations = JArray.Parse(reqBody);
            var collectedInformation = "";

            foreach (var station in stations)
            {
                var stationName = (string) station.SelectToken("name");
                Console.WriteLine(stationName);

                var stationInfo = stadtradParser.GetInfoForOneStation(receivedInfos, stationName);
                if (stationInfo != null)
                {
                    var response = new HttpResponseMessage();
                    using (var client = new HttpClient())
                    {
                        if (Development)
                        {
                            response = client.GetAsync(dummyPrediction).Result;
                        }
                        else
                        {
                            var requestedStation = prediction + "?name=" + stationName;
                            Console.WriteLine("Requested Prediction ULR: " + requestedStation);
                            response = client.GetAsync(prediction).Result;
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            //var predictionResponseArray = JArray.Parse(System.IO.File.ReadAllText("prediction.json"));
                            var predictionResponseArray = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                            Console.WriteLine("Empfangener Array: " + predictionResponseArray);

                            int pred = (int)predictionResponseArray[0];
                            var current = (int) stationInfo.SelectToken("bikes");

                            JArray hist = new JArray();
                            hist.Add((int)predictionResponseArray[2]);
                            hist.Add((int)predictionResponseArray[3]);
                            hist.Add((int)predictionResponseArray[4]);
                            hist.Add((int)predictionResponseArray[5]);

                            stationInfo.Add(new JProperty("history",hist));

                            int realBikeCount = current + pred;
                            if (realBikeCount < 0)
                                realBikeCount = 0;
                            stationInfo.Add("prediction",realBikeCount);
                        }
                    }

                    if (collectedInformation != "")
                        collectedInformation += "," + stationInfo;
                    else
                        collectedInformation += stationInfo;
                }
                else
                {
                    return null;
                }
            }
            return collectedInformation;
        }
    }
}