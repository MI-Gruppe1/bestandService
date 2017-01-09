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

        // in development mode the service is being run with mock data
        private const bool Development = false;

        // list of the known stations
        private JArray _knownStations;

        public BestandController()
        {
            RadInfoDownloader radInfoDownloader = new RadInfoDownloader();

    ///////////////
    //Quickfix
    ////////////////
    
            var stationsFromFile = radInfoDownloader.ReadStationsFromFile();
            _knownStations = JArray.Parse(stationsFromFile);

//            if (Development)
//            {
//                var stationsFromFile = radInfoDownloader.ReadStationsFromFile();
//                _knownStations = JArray.Parse(stationsFromFile);
//            }
//            else
//            {
//                // try to reach the radDB Service and download a list of all stations
//                var downloadResponse = radInfoDownloader.DownloadAllStations();
//                while (downloadResponse == null)
//                {
//                    Console.WriteLine("sleeping");
//                    System.Threading.Thread.Sleep(1000);
//                    downloadResponse = radInfoDownloader.DownloadAllStations();
//                }
//                _knownStations = JArray.Parse(downloadResponse);
//            }
        }

        /// <summary>
        /// Get all stations with the current bike stock
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        [HttpGet]
        public string GetAll()
        {
            var stadtRadInfo = "";
            var radDbInfo = "";
            RadInfoDownloader radInfoDownloader = new RadInfoDownloader();

            if (Development)
            {
                // if Development read information from file
                stadtRadInfo = radInfoDownloader.ReadStadtRadResponseFromFile();

                radDbInfo = radInfoDownloader.ReadStationsFromFile();
            }
            else
            {
                stadtRadInfo = radInfoDownloader.DownloadStadtRadInformation();
                if (stadtRadInfo == null)
                {
                    throw new HttpRequestException("Stadtrad API not reachable");
                }

        ///////////////
        //Quickfix
        ////////////////

                radDbInfo = radInfoDownloader.ReadStationsFromFile();
                //radDbInfo = radInfoDownloader.DownloadAllStations();
                if (radDbInfo == null)
                {
                    radDbInfo = radInfoDownloader.ReadStationsFromFile();
                }
            }

            var stadtradParser = new StadtradParser();
            var allStations = stadtradParser.GetAllStations(stadtRadInfo, radDbInfo);
            return JsonConvert.SerializeObject(allStations);
        }

        /// <summary>
        /// Get bike stock for one specific station
        /// </summary>
        /// <param name="stationName"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        [HttpGet("{*stationName}", Name = "GetStation")]
        public string GetStation(string stationName)
        {
            var receivedInfos = "";
            RadInfoDownloader radInfoDownloader = new RadInfoDownloader();

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
            var stationInfo = stadtradParser.GetInfoForOneStation(receivedInfos, stationName);
            if (stationInfo != null)
                return JsonConvert.SerializeObject(stationInfo);
            else
                return null;
        }
    }
}