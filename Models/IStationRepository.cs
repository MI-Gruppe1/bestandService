using System.Collections.Generic;

namespace BestandService.Models
{
    public interface IStationRepository
    {
        void Add(Station _station);
        void Remove(Station _station);
        void Update(Station _station);
        Station Find(string _name);
        IEnumerable<Station> GetAll();
    }
}