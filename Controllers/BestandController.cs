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

            if (Development)
            {
                Console.WriteLine("Bestand Controller: starting in development mode");
                var stationsFromFile = radInfoDownloader.ReadStationsFromFile();
                _knownStations = JArray.Parse(stationsFromFile);
            }
            else
            {
                Console.WriteLine("Bestand Controller: starting in production mode");
                // try to reach the radDB Service and download a list of all stations
                var downloadResponse = radInfoDownloader.DownloadAllStations();
                Console.WriteLine("Bestand Controller: all stations downloaded from stadtraddbservice");
                while (downloadResponse == null)
                {
                    Console.WriteLine("sleeping");
                    System.Threading.Thread.Sleep(1000);
                    downloadResponse = radInfoDownloader.DownloadAllStations();
                }
                _knownStations = JArray.Parse(downloadResponse);
            }
        }

        /// <summary>
        /// Get all stations with the current bike stock
        /// </summary>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        [HttpGet]
        public string GetAll()
        {
            Console.WriteLine("Bestand Controller: bestand requested");
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
                Console.WriteLine("Bestand Controller: current stock from stadtrad downloaded");

                radDbInfo = radInfoDownloader.DownloadAllStations();
                if (radDbInfo == null)
                {
                    Console.WriteLine("Bestand Controller: couldn't download stationlist from stadtraddb service, working with copy from file now");
                    radDbInfo = radInfoDownloader.ReadStationsFromFile();
                }
                else
                {
                    Console.WriteLine("Bestand Controller: all stations downloaded from stadtraddbservice 2");
                }
            }

            var stadtradParser = new StadtradParser();
            var allStations = stadtradParser.GetAllStations(stadtRadInfo, radDbInfo);
            Console.WriteLine("Bestand Controller: all stations formatted and returned");
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
            Console.WriteLine("Bestand Controller: bestand/specific station requested");
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
                    Console.WriteLine("Bestand Controller: couldn't download current stock from stadtrad");
                    throw new HttpRequestException("Stadtrad API not reachable");
                }
            }

            var stadtradParser = new StadtradParser();
            var stationInfo = stadtradParser.GetInfoForOneStation(receivedInfos, stationName);
            Console.WriteLine("Bestand Controller: stock for station collected: " + stationInfo.ToString());
            if (stationInfo != null)
                return JsonConvert.SerializeObject(stationInfo);
            else
                return null;
        }
    }
}