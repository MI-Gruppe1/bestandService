using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc;

namespace BestandService.Models
{
    public class StationRepository : IStationRepository
    {
        private static ConcurrentDictionary<string, Station> stations;

        public StationRepository()
        {
            stations = new ConcurrentDictionary<string, Station>();
        }

        public void Add(Station _station)
        {
            _station.Key = Guid.NewGuid().ToString();
            stations[_station.Key] = _station;
        }

        public void Remove(Station _station)
        {
            throw new System.NotImplementedException();
        }

        public void Update(Station _station)
        {
            throw new System.NotImplementedException();
        }

        public Station Find(string _name)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<Station> GetAll()
        {
            return stations.Values;
        }
    }
}