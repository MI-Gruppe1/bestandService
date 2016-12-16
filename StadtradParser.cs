using System;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace BestandService
{
    public class StadtradParser
    {
        public JObject GetInfoForOneStation(string input, string stationName)
        {
            JToken stadtRadInformation = new JArray();

            stadtRadInformation = JObject.Parse(input);

            var markers = (JArray) stadtRadInformation["marker"];
            var resp = new JObject();
            foreach (var item in markers.Children())
            {
                // get properties
                var itemProperties = item.Children<JProperty>();

                var hal2OptionProp = itemProperties.FirstOrDefault(x => x.Name == "hal2option");
                var hal2Option = hal2OptionProp.Value;

                var tooltipValue = (string) hal2Option["tooltip"];
                var name = tooltipValue.Substring(1, 4);

                var requestedName = stationName.Substring(0, 4);

                if (name == requestedName)
                {
                    var bikeCount = hal2Option["bikelist"].Count();


                    resp["name"] = stationName;
                    var latitudeProp = itemProperties.FirstOrDefault(x => x.Name == "lat");
                    resp["latitude"] = latitudeProp.Value;
                    var longitudeProp = itemProperties.FirstOrDefault(x => x.Name == "lng");
                    resp["longitude"] = latitudeProp.Value;
                    resp["bikes"] = bikeCount;

                    return resp;
                }
            }
            return null;
        }

        public JToken GetAllStations(string stadtRadStations, string radDbStations)
        {
            JToken stadtRadInformation = new JArray();
            JToken radDbInformation = new JArray();

            stadtRadInformation = JObject.Parse(stadtRadStations);
            radDbInformation = JArray.Parse(radDbStations);

            var markers = (JArray) stadtRadInformation["marker"];
            foreach (var item in markers.Children())
            {
                // get properties
                var itemProperties = item.Children<JProperty>();

                var hal2OptionProp = itemProperties.FirstOrDefault(x => x.Name == "hal2option");
                var hal2Option = hal2OptionProp.Value;

                var bikeCount = hal2Option["bikelist"].Count();

                var tooltipValue = (string) hal2Option["tooltip"];
                var name = tooltipValue.Substring(1, 4);

                foreach (var inf in radDbInformation)
                {
                    var fullName = (string) inf["name"];
                    if (fullName.StartsWith(name))
                    {
                        inf["bikes"] = bikeCount;
                    }
                }
            }
            return radDbInformation;
        }
    }
}