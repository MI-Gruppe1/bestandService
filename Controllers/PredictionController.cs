﻿using System;
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
        // in development mode the service is being run with mock data
        private const bool Development = true;
        private const string dummyPrediction = "http://localhost:5000/prediction";
        private const string prediction = "http://localhost:3000/prediction";

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
                receivedInfos = radInfoDownloader.DownloadStadtRadInformation();
                if (receivedInfos == null)
                {
                    throw new HttpRequestException("Stadtrad API not reachable");
                }
            }

            var stadtradParser = new StadtradParser();

            var reqBody = new StreamReader(Request.Body).ReadToEnd();
            var stations = JArray.Parse(reqBody);
            var collectedInformation = "";

            foreach (var station in stations)
            {
                var stationName = (string) station.SelectToken("name");

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
                            response = client.GetAsync(prediction).Result;
                        }

                        if (response.IsSuccessStatusCode)
                        {
                            stationInfo.Add("prediction",response.Content.ReadAsStringAsync().Result);
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