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
        // todo: replace with proper DB connection
        private readonly IStationRepository _stationRepository;


        public BestandController(IStationRepository stationRepository)
        {
            this._stationRepository = stationRepository;
        }


        // return information about the requested station
        // returns 404 if that station doesnt exist
        [HttpGet("{stationID}", Name = "GetStation")]
        public IActionResult GetStation(string stationName)
        {
            var searchedStation = _stationRepository.Find(stationName);

            if (searchedStation == null)
            {
                return NotFound();
            }
            return new ObjectResult(searchedStation);
        }


        // push new found station to the db or dictionary for now
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

        [HttpGet]
        public IEnumerable<Station> GetAll()
        {
            return _stationRepository.GetAll();
        }
    }
}