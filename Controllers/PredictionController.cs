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
                Console.WriteLine("Prediction Controller working in production mode");
                receivedInfos = radInfoDownloader.DownloadStadtRadInformation();
                if (receivedInfos == null)
                {
                    Console.WriteLine("Prediction Controller: Stadtrad API not reachable");
                    throw new HttpRequestException("Prediction Controller: Stadtrad API not reachable");
                }
            }

            var stadtradParser = new StadtradParser();

            var reqBody = new StreamReader(Request.Body).ReadToEnd();
            //Console.WriteLine("Prediction Controller: Request Body read: " + reqBody.ToString());
            var stations = JArray.Parse(reqBody);
            var collectedInformation = "";

            foreach (var station in stations)
            {
                var stationName = (string) station.SelectToken("name");
                Console.WriteLine("\n\nPrediction Controller: requested Station " +stationName);

                var stationInfo = stadtradParser.GetInfoForOneStation(receivedInfos, stationName);
                if (stationInfo != null)
                {
                    Console.WriteLine("Prediction Controller: using fake prediction to simulate prediction service");
                    const bool dummyPredictionTest = false;
                    if (dummyPredictionTest)
                    {
                        var rand = new Random();
                        var pred = rand.Next(-3, 3);
                        var current = (int) stationInfo.SelectToken("bikes");

                        var hist = new JArray
                        {
                            rand.Next(0, current),
                            rand.Next(3, 15),
                            rand.Next(4, 16),
                            rand.Next(5, 17)
                        };

                        stationInfo.Add(new JProperty("history", hist));
                        Console.WriteLine("Prediction Controller: fake history added to stationInfo: " + hist.ToString());

                        var realBikeCount = current + pred;
                        if (realBikeCount < 0)
                            realBikeCount = 0;
                        stationInfo.Add("prediction", realBikeCount);
                        Console.WriteLine("Prediction Controller: fake prediction added to stationInfo: " + realBikeCount);
                    }
                    else
                    {
                        Console.WriteLine("Prediction Controller: trying to get information from real prediction service");
                        var response = new HttpResponseMessage();
                        using (var client = new HttpClient())
                        {
                                var encoding = System.Net.WebUtility.UrlEncode(stationName);
                                var requestedStation = prediction + "?name=" + encoding;
                                Console.WriteLine("Prediction Controller: Requested Prediction ULR: " + requestedStation);

                                //response = new HttpResponseMessage(HttpStatusCode.Accepted);
                                response = client.GetAsync(requestedStation).Result;

                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Prediction Controller: received IsSuccessfullStatusCode from prediction service");
                                //var predictionResponseArray = JArray.Parse("[-10.22,11.0,13.0,7.0,12.0,4.0]");
                                var predictionResponseArray = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                                Console.WriteLine("Prediction Controller: Empfangener Array: " + predictionResponseArray);

                                var pred = (int) predictionResponseArray[0];
                                var current = (int) stationInfo.SelectToken("bikes");
                                Console.WriteLine("Prediction Controller: received prediction: " + pred);

                                var hist = new JArray
                                {
                                    (int) predictionResponseArray[2],
                                    (int) predictionResponseArray[3],
                                    (int) predictionResponseArray[4],
                                    (int) predictionResponseArray[5]
                                };

                                stationInfo.Add(new JProperty("history", hist));
                                Console.WriteLine("Prediction Controller: history added to stationInfo: " + hist.ToString());

                                int realBikeCount = current + pred;
                                if (realBikeCount < 0)
                                    realBikeCount = 0;
                                stationInfo.Add("prediction", realBikeCount);
                                Console.WriteLine("Prediction Controller: prediction added to stationInfo: " + realBikeCount);
                            }
                        }
                    }

                    if (collectedInformation != "")
                        collectedInformation += "," + stationInfo;
                    else
                        collectedInformation += stationInfo;
                    Console.WriteLine("Prediction Controller: collected Information: " + collectedInformation.ToString());
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