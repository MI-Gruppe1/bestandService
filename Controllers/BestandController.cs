using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BestandService.Models;
using Microsoft.AspNetCore.Mvc;

namespace BestandService.Controllers
{
    [Route("[controller]")]
    public class BestandController : Controller
    {

        // safes all station for now
        // TODO: replace with proper DB connection
        private readonly IStationRepository _stationRepository;


        /// <summary>
        /// Constructor for starting the api
        /// </summary>
        /// <param name="stationRepository">List with stations</param>
        public BestandController(IStationRepository stationRepository)
        {
            this._stationRepository = stationRepository;
        }


        /// <summary>
        /// Get a list of all known stations including
        /// the position, name and stock of the stations
        /// </summary>
        /// <returns>List of all bike stations</returns>
        [HttpGet]
        public IEnumerable<Station> GetAll()
        {
            return _stationRepository.GetAll();
        }


        /// <summary>
        /// Get information to a specific station
        /// </summary>
        /// <param name="stationName">Name of the
        ///     requested station</param>
        /// <returns></returns>
        [HttpGet("{stationName}", Name = "GetStation")]
        public IActionResult GetStation(string stationName)
        {
            Console.WriteLine("station name: " + stationName);
            var searchedStation = _stationRepository.Find(stationName);

            if (searchedStation == null)
            {
                return NotFound();
            }
            return new ObjectResult(searchedStation);
        }


        /// <summary>
        /// Add a new station
        /// </summary>
        /// <param name="station"></param>
        /// <returns>key to find the station later</returns>
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