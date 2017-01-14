using System.Collections.Generic;
using System.Net.Http;

namespace BestandService
{
    public class RadInfoDownloader
    {
        private const string AllStations = "http://stadtraddbservice:6000/allStations";
        private const string StadtRadUrl = "http://stadtrad.hamburg.de/kundenbuchung/hal2ajax_process.php";

        public string DownloadStadtRadInformation()
        {
            // needed information for the stadtRad rest api
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("mapstadt_id", "75"),
                new KeyValuePair<string, string>("ajxmod", "hal2map"),
                new KeyValuePair<string, string>("callee", "getMarker"),
            });

            using (var client = new HttpClient())
            {
                var response = client.PostAsync(StadtRadUrl, formContent).Result;

                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    return null;
                }
            }
        }

        public string DownloadAllStations()
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(AllStations).Result;

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    return responseContent.ReadAsStringAsync().Result;
                }
            }
            return null;
        }

        public string ReadStationsFromFile()
        {
            return System.IO.File.ReadAllText("andi.json");
        }

        public string ReadStadtRadResponseFromFile()
        {
            return System.IO.File.ReadAllText("stadtRadSample.json");
        }
    }
}