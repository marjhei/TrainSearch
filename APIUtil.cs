using Newtonsoft.Json;
using RataDigiTraffic.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RataDigiTraffic
{
    public class APIUtil

    {
        public List<Station> Stations()
        {
            string json = "";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync($"https://rata.digitraffic.fi/api/v1/metadata/stations").Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                json = responseString;
            }
            List<Station> res;
            res = JsonConvert.DeserializeObject<List<Station>>(json);
            return res;
        }

        public List<Train> TrainsBetween(string from, string to, int limit = 5)
        {
            string json = "";
            //string url = $"https://rata.digitraffic.fi/api/v1/schedules?departure_station={from}&arrival_station={to}";
            // Newer link to the API
            string url = $"https://rata.digitraffic.fi/api/v1/live-trains/station/{from}/{to}?include_nonstopping=false&limit={limit}";
            // The following link for testing purposes, set to show trains after the end of daylight saving.
            //string url = $"https://rata.digitraffic.fi/api/v1/live-trains/station/{from}/{to}?departure_date=2019-11-01&include_nonstopping=false&limit=1";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync(url).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                json = responseString;
            }
            try
            {
                List<Train> res;
                res = JsonConvert.DeserializeObject<List<Train>>(json);
                return res;
            }
            catch (Exception e)
            {
                if (json.Contains("TRAIN_NOT_FOUND"))
                {
                    throw new ArgumentException("No direct trains found between the two stations.");
                }
                else
                {
                    throw e;
                }
            }
        }

        public List<Train> CurrentStationInfoWithLimit(string stationShortCode, int arrivedTrains, int arrivingTrains, int departedTrains, int departingTrains)
        {
            string json = "";
            string url = $"https://rata.digitraffic.fi/api/v1/live-trains/station/{stationShortCode}?arrived_trains={arrivedTrains}&arriving_trains={arrivingTrains}&departed_trains={departedTrains}&departing_trains={departingTrains}&train_categories=Commuter,Long-distance";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync(url).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                json = responseString;
            }
            List<Train> res;
            if (!string.IsNullOrEmpty(json))
            {
                res = JsonConvert.DeserializeObject<List<Train>>(json);
            }
            else
            {
                throw new ArgumentException("No trains found in the nearby future.");
            }
            return res;
        }
        public List<Train> CurrentStationInfoWithTime(string stationShortCode, int minutesBeforeDeparture, int minutesAfterDeparture, int minutesBeforeArrival, int minutesAfterArrival)
        {
            string json = "";
            string url = $"https://rata.digitraffic.fi/api/v1/live-trains/station/{stationShortCode}?minutes_before_departure={minutesBeforeDeparture}&minutes_after_departure={minutesAfterDeparture}&minutes_before_arrival={minutesBeforeArrival}&minutes_after_arrival={minutesAfterArrival}&train_categories=Commuter,Long-distance";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync(url).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                json = responseString;
            }
            List<Train> res;
            if (!string.IsNullOrEmpty(json))
            {
                res = JsonConvert.DeserializeObject<List<Train>>(json);
            }
            else
            {
                throw new ArgumentException("No trains found in the nearby future.");
            }
            return res;
        }

        //public List<Train> NextStationInfo(string stationShortCode)
        //{
        //    string json = "";
        //    string url = $"https://rata.digitraffic.fi/api/v1/";
        //}

        public List<TrackingMessage> StationTrains(string paikka)
        {
            string json = "";
            string url = $"https://rata.digitraffic.fi/api/v1/train-tracking?station={paikka}&departure_date={DateTime.Today.ToString("yyyy-MM-dd")}";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync(url).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                json = responseString;
            }
            List<TrackingMessage> res;
            res = JsonConvert.DeserializeObject<List<TrackingMessage>>(json);
            return res;
        }

        //this client is for getting the train routes (of the current date) from the api
        public List<Train> TrainRoute(int trainNumber)
        {
            string json = "";
            string url = $"https://rata.digitraffic.fi/api/v1/trains/{DateTime.Today.ToString("yyyy-MM-dd")}/{trainNumber}";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync(url).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                json = responseString;
            }
            List<Train> res;
            res = JsonConvert.DeserializeObject<List<Train>>(json);
            return res;
        }

        public List<TrainLocation> TrainLocationLatest(int trainNumber)
        {
            string json = "";
            string url = $"https://rata.digitraffic.fi/api/v1/train-locations/latest/{trainNumber}";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync(url).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                json = responseString;
            }
            List<TrainLocation> res;
            res = JsonConvert.DeserializeObject<List<TrainLocation>>(json);
            return res;
        }

        public List<TrainLocation> TrainLocationPast(int trainNumber)
        {
            string json = "";
            string url = $"https://rata.digitraffic.fi/api/v1//train-locations/{DateTime.Today.ToString("yyyy-MM-dd")}/{trainNumber}";

            using (var client = new HttpClient())
            {

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = client.GetAsync(url).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                json = responseString;
            }
            List<TrainLocation> res;
            res = JsonConvert.DeserializeObject<List<TrainLocation>>(json);
            return res;
        }
    }





}
