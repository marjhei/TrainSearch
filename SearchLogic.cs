using RataDigiTraffic;
using RataDigiTraffic.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using GeoCoordinatePortable;

namespace Trains
{
    public enum TimetableRowType { Arrival, Departure };

    static class SearchLogic
    {
        public static Dictionary<string, Station> stationDictionary = new Dictionary<string, Station>();

        //populates the stationDictionary with stationNames and stationShortCodes as keys and their respective station objects as values
        //needs to be run at the start-up of the app! (route method relies on this)
        public static void PopulateStationDictionary()
        {
            APIUtil utils = new APIUtil();

            List<Station> result = utils.Stations();

            foreach (var station in result)
            {
                stationDictionary.Add(station.stationName.ToUpper(), station);
                //handles the case where Eno station has the short code eno
                //note: still contains "stations" with type "TURNOUT_IN_THE_OPEN_LANE"
                if (!stationDictionary.ContainsKey(station.stationShortCode))
                {
                    stationDictionary.Add(station.stationShortCode, station);
                }
            }
        }

        public static Station ConvertUserInputStringToStation(string input)
        {


            if (stationDictionary.ContainsKey(input.Trim().ToUpper()))
            {
                Station userStation = stationDictionary[input.Trim().ToUpper()];
                return userStation;
            }

            else
            {
                throw new ArgumentException($"{input} is not a valid station. Please try again.");
            }

        }

        public static string ConvertUserInputStationToShortCode(string input)
        {
            Station userStation = stationDictionary[input.ToUpper().Trim()];
            string shortcode = userStation.stationShortCode;
            return shortcode;
        }




        //gets trains between two stations specified by the user
        public static List<Train> SearchTrainsBetweenStations(Station from, Station to)
        {
            var api = new APIUtil();
            var fromShortCode = from.stationShortCode;
            var toShortCode = to.stationShortCode;

            var trains = api.TrainsBetween(fromShortCode, toShortCode);
            return trains;
        }

        //passes an int input (route number) to the api client and then prints the stations with arrival times
        public static void GetTrainRoute(int trainNum)
        {
            //int trainNum = GetTrainNumber(); //now supplied as an argument from the menu

            APIUtil api = new APIUtil();
            List<Train> TrainRoute = api.TrainRoute(trainNum);
            if (TrainRoute.Count == 0)
            {
                Console.WriteLine("There are no trains operating with the given train number.");
                return;
            }
            //end of train number parsing

            Console.WriteLine($"The timetable for train {trainNum} is: ");
            Console.WriteLine();
            Console.WriteLine($"{"Station",-15}{"Time",10}{"Stop (minutes)",20}");

            //initialisation of variables used in loop below
            DateTime stationArrivalTime = default;
            string shortStationArrivalTime = "";

            double stationStopDuration = 0;
            bool firstStation = true;
            string stationName = "";
            List<TimetableRow> station = TrainRoute[0].timeTableRows;

            for (int i = 0; i < station.Count; i++)//index zero because there will only be one item in the list so no need to iterate through the "list"
            {
                if (station[i].commercialStop) //if it's a station where the train stops
                {
                    if (station[i].type == "ARRIVAL")
                    {
                        stationArrivalTime = station[i].scheduledTime;
                        shortStationArrivalTime = station[i].scheduledTime.ToLocalTime().ToString("HH:mm");
                        stationName = stationDictionary[station[i].stationShortCode].stationName;

                        if (i < station.Count - 1 && station[i + 1].type == "DEPARTURE")
                        {
                            stationStopDuration = (station[i + 1].scheduledTime - stationArrivalTime).TotalMinutes;
                        }
                        Console.WriteLine($"{stationName,15}{shortStationArrivalTime,12}{(i == station.Count - 1 ? "" : (stationStopDuration > 0 ? stationStopDuration.ToString() : "")),12}");
                    }

                    if (firstStation) //first station (when it doesn't have an arrival-pair)
                    {
                        stationName = stationDictionary[station[i].stationShortCode].stationName;
                        shortStationArrivalTime = station[i].scheduledTime.ToLocalTime().ToString("HH:mm");
                        firstStation = false;
                        Console.WriteLine($"{stationName,15}{shortStationArrivalTime,12}{(stationStopDuration > 0 ? stationStopDuration.ToString() : ""),12}");
                    }
                }
            }
        }


        //returns the train number that user provided, removes possible non-numbers
        public static int GetTrainNumber()
        {
            //parsing the input train number
            int trainNum = 0;
            bool format = false;
            while (!format)
            {
                Console.WriteLine("Enter a train number:");
                try
                {
                    string tempTrainNum = Console.ReadLine().Trim();
                    if(tempTrainNum == "exit") { return trainNum; }
                    string numberOnly = Regex.Replace(tempTrainNum, "[^0-9.]", "");
                    trainNum = int.Parse(numberOnly);

                    format = true;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Please enter a valid train number or type 'exit' to go back to the menu.");
                }
            }
            return trainNum;
        }

        //overload for commandlineparser
        public static int GetTrainNumber(string tempTrainNum)
        {
            //parsing the input train number
            int trainNum = 0;
            bool format = false;
            while (!format)
            {
                try
                {
                    string numberOnly = Regex.Replace(tempTrainNum, "[^0-9.]", "");
                    trainNum = int.Parse(numberOnly);

                }
                catch (FormatException)
                {
                    Console.WriteLine("Please enter a valid train number!");
                    throw new FormatException();
                }
                format = true;
            }
            return trainNum;
        }

        public static decimal GetTrainDistanceFromStation(Station station, int trainNum) //remember to add GetTrainNumber() to menu logic to pass the 2nd arg!
        {
            //int trainNum = GetTrainNumber();
            APIUtil api = new APIUtil();
            List<TrainLocation> trainLocation = api.TrainLocationLatest(trainNum);
            List<TrainLocation> trainLocationPast = api.TrainLocationPast(trainNum);


            try
            {
                //calculating distance: current location
                decimal longT = trainLocation[0].location.coordinates[0];
                decimal latT = trainLocation[0].location.coordinates[1];
                decimal longS = station.longitude;
                decimal latS = station.latitude;

                //past location: 15s before the current location
                decimal longP = trainLocationPast[10].location.coordinates[0];
                decimal latP = trainLocationPast[10].location.coordinates[1];

                var coordT = new GeoCoordinate((double)latT, (double)longT);
                var coordS = new GeoCoordinate((double)latS, (double)longS);
                var coordP = new GeoCoordinate((double)latP, (double)longP);


                decimal distInMeters = Convert.ToDecimal(coordT.GetDistanceTo(coordS));
                decimal distInKm = Math.Round((distInMeters / 1000), 1);

                decimal pastDistInMeters = Convert.ToDecimal(coordP.GetDistanceTo(coordS));
                decimal pastDistInKm = Math.Round((pastDistInMeters / 1000), 1);

                //Console.WriteLine("past long " + longP + " past lat " + latP);
                //Console.WriteLine("past dist: " + pastDistInKm);


                //this loop ensures the user's station is on the train's route
                List<Train> trainRoute = api.TrainRoute(trainNum);
                bool stationIsOnRoute = false;
                int userStationIndex = 0;
                foreach (var s in trainRoute[0].timeTableRows)
                {
                    if (s.stationShortCode == station.stationShortCode)
                    {
                        if (s.trainStopping)
                        {
                            stationIsOnRoute = true;
                            userStationIndex = trainRoute[0].timeTableRows.IndexOf(s);
                        }
                        break;
                    }
                }

                bool stoppedAlready = (trainRoute[0].timeTableRows[userStationIndex].scheduledTime > DateTime.Today) && (trainRoute[0].timeTableRows[userStationIndex].actualTime != null ? trainRoute[0].timeTableRows[userStationIndex].actualTime > DateTime.Today : true);
                if (stationIsOnRoute && !stoppedAlready)
                {
                    Console.WriteLine($"Distance from {station.stationName} station: " + distInKm + "km");//junan etäisyys pasilan asemalta

                    if (distInMeters < pastDistInMeters)
                    {
                        Console.WriteLine("The train is approaching your station.");
                    }
                    if (distInMeters > pastDistInMeters)
                    {
                        Console.WriteLine("The train is going further from your station.");
                    }
                }

                if (!stationIsOnRoute)
                {
                    Console.WriteLine("The train is not stopping at your station.");
                }

                if (stationIsOnRoute && stoppedAlready)
                {
                    Console.WriteLine("The train has already passed your station.");
                }


                return distInKm;
            }
            catch (Exception)
            {
                Console.WriteLine("Train is currently not operational. Try Again with another train number.");
                return -1;
                ////move below to consoleUI logic!
                //Console.WriteLine("Train is currently not operational. Press 'Esc' to exit, otherwise press any key to try again.");
                //ConsoleKeyInfo key = Console.ReadKey();
                //if(key.Key == ConsoleKey.Escape)
                //{
                //    return 0; 
                //}
                //else { return GetTrainDistanceFromStation(station, trainNum); }
            }
        }

        public static void SearchBetweenStations(Station from, Station to, int numberToPrint = 5)
        {
            var api = new APIUtil();
            var fromShortCode = from.stationShortCode;
            var toShortCode = to.stationShortCode;

            var trains = api.TrainsBetween(fromShortCode, toShortCode, numberToPrint);

            Console.WriteLine();
            Console.WriteLine($"Next {trains.Count} " + (trains.Count > 0 ? "trains" : "train") + $" between {from.stationName} and {to.stationName}:");

            foreach (var t in trains)
            {
                var sb = new StringBuilder();
                sb.AppendLine(TrainName(t));
                var departure = SearchForTimetableRow(fromShortCode, t.timeTableRows, TimetableRowType.Departure)[0];
                sb.AppendLine($"\tScheduled departure time from {from.stationName}: {departure.scheduledTime.ToLocalTime()}");
                if (departure.liveEstimateTime != DateTime.MinValue)
                {
                    sb.AppendLine($"\tEstimated departure time: {departure.liveEstimateTime.ToLocalTime()} ({departure.differenceInMinutes} minutes late");
                }
                var arrival = SearchForTimetableRow(toShortCode, t.timeTableRows, TimetableRowType.Arrival)[0];
                sb.AppendLine($"\tScheduled arrival time to {to.stationName}: {arrival.scheduledTime.ToLocalTime()}");
                if (arrival.liveEstimateTime != DateTime.MinValue)
                {
                    sb.AppendLine($"\tEstimated departure time: {arrival.liveEstimateTime.ToLocalTime()} ({arrival.differenceInMinutes} minutes late");
                }
                Console.WriteLine(sb.ToString());
            }
        }

        public static List<Train> CurrentStationInfoWithLimit(Station station, int limit = 5)
        {
            var api = new APIUtil();
            var stationShortCode = station.stationShortCode;
            List<Train> trains = new List<Train>();

            try
            {
                trains = api.CurrentStationInfoWithLimit(stationShortCode, limit, limit, limit, limit);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return trains;
        }

        public static List<Train> CurrentStationInfoWithTime(Station station, int minutesBeforeDeparture = 15, int minutesAfterDeparture = 15, int minutesBeforeArrival = 15, int minutesAfterArrival = 15)
        {
            var api = new APIUtil();
            var stationShortCode = station.stationShortCode;
            List<Train> trains = new List<Train>();

            try
            {
                trains = api.CurrentStationInfoWithTime(stationShortCode, minutesBeforeDeparture, minutesAfterDeparture, minutesBeforeArrival, minutesAfterArrival);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return trains;
        }
        public static void ShowUpcomingArrivals(Station station, List<Train> trains)
        {
            var upcomingArrivalTimes = new List<(Train, TimetableRow)>();
            var stationShortCode = station.stationShortCode;

            foreach (var t in trains)
            {
                var arrival = SearchForTimetableRow(stationShortCode, t.timeTableRows, TimetableRowType.Arrival);
                if (arrival.Length != 0)
                {
                    if (arrival[0].scheduledTime > DateTime.Now.ToUniversalTime().AddMinutes(-1) || arrival[0].liveEstimateTime > DateTime.Now.ToUniversalTime().AddMinutes(-1))
                    {
                        upcomingArrivalTimes.Add((t, arrival[0]));
                    }
                }
            }

            var sortUpcomingArrivals = from tuple in upcomingArrivalTimes
                                       orderby tuple.Item2.scheduledTime
                                       select tuple;

            upcomingArrivalTimes = sortUpcomingArrivals.ToList();

            Console.WriteLine();
            var header = $"Upcoming arrivals at {station.stationName}";
            Console.WriteLine(header);
            Console.WriteLine("".PadLeft(header.Length).Replace(" ", "="));
            Console.WriteLine("Train:    From:           Time:");
            foreach (var tuple in upcomingArrivalTimes)
            {
                var train = tuple.Item1;
                var sb = new StringBuilder();
                var arrival = SearchForTimetableRow(stationShortCode, train.timeTableRows, TimetableRowType.Arrival);
                if (arrival.Length != 0)
                {
                    sb.Append(TrainName(train).PadRight(10));
                    var fromStation = stationDictionary[train.timeTableRows[0].stationShortCode].stationName;
                    sb.Append(fromStation.PadRight(16));
                    sb.Append(arrival[0].scheduledTime.ToLocalTime().ToLongTimeString().PadRight(7));
                    if (arrival[0].scheduledTime.Date != DateTime.Today)
                    {
                        sb.Append(" (");
                        sb.Append(arrival[0].scheduledTime.ToShortDateString());
                        sb.Append(")");
                    }
                    if (arrival[0].liveEstimateTime != DateTime.MinValue && arrival[0].liveEstimateTime != arrival[0].scheduledTime && arrival[0].differenceInMinutes > 0)
                    {
                        sb.Append("  =>  ");
                        sb.Append(arrival[0].liveEstimateTime.ToLocalTime().ToLongTimeString().PadRight(7));
                        if (arrival[0].liveEstimateTime.Date != DateTime.Today)
                        {
                            sb.Append(" (");
                            sb.Append(arrival[0].liveEstimateTime.ToShortDateString());
                            sb.Append(")");
                        }
                        sb.Append(" ");
                        var difference = arrival[0].differenceInMinutes;
                        sb.Append(difference < 1 && difference > -1 ? "< 1" : "~ " + Math.Abs(difference).ToString());
                        sb.Append(Math.Abs(difference) > 1 ? " minutes " : " minute ");
                        sb.Append(difference >= 0 ? "late" : "early");
                    }

                    Console.WriteLine(sb.ToString());
                }
            }
        }

        public static void ShowPastArrivals(Station station, List<Train> trains)
        {
            var pastArrivalTimes = new List<(Train, TimetableRow)>();
            var stationShortCode = station.stationShortCode;

            foreach (var t in trains)
            {
                var arrival = SearchForTimetableRow(stationShortCode, t.timeTableRows, TimetableRowType.Arrival);
                if (arrival.Length != 0)
                {
                    // Last() instead of [0] ?????
                    if (arrival.Last().scheduledTime < DateTime.Now.ToUniversalTime() && arrival.Last().liveEstimateTime < DateTime.Now.ToUniversalTime())
                    {
                        pastArrivalTimes.Add((t, arrival.Last()));
                    }
                }
            }

            var sortPastArrivals = from tuple in pastArrivalTimes
                                   orderby tuple.Item2.scheduledTime descending
                                   select tuple;

            pastArrivalTimes = sortPastArrivals.ToList();

            Console.WriteLine();
            var header = $"Past arrivals at {station.stationName}";
            Console.WriteLine(header);
            Console.WriteLine("".PadLeft(header.Length).Replace(" ", "="));
            Console.WriteLine("Train:    From:           Time:");
            foreach (var tuple in pastArrivalTimes)
            {
                var train = tuple.Item1;
                var sb = new StringBuilder();
                var arrival = SearchForTimetableRow(stationShortCode, train.timeTableRows, TimetableRowType.Arrival);
                if (arrival.Length != 0)
                {
                    sb.Append(TrainName(train).PadRight(10));
                    var fromStation = stationDictionary[train.timeTableRows[0].stationShortCode].stationName;
                    sb.Append(fromStation.PadRight(16));
                    sb.Append(arrival.Last().scheduledTime.ToLocalTime().ToLongTimeString().PadRight(7));
                    if (arrival.Last().scheduledTime.Date != DateTime.Today)
                    {
                        sb.Append(" (");
                        sb.Append(arrival.Last().scheduledTime.ToShortDateString());
                        sb.Append(")");
                    }
                    if (arrival.Last().actualTime != DateTime.MinValue && arrival.Last().actualTime != arrival.Last().scheduledTime && arrival.Last().differenceInMinutes > 0)
                    {
                        sb.Append("  =>  ");
                        sb.Append(arrival.Last().actualTime.ToLocalTime().ToLongTimeString().PadRight(7));
                        if (arrival.Last().actualTime.Date != DateTime.Today)
                        {
                            sb.Append(" (");
                            sb.Append(arrival.Last().actualTime.ToShortDateString());
                            sb.Append(")");
                        }
                        sb.Append(" ");
                        var difference = arrival.Last().differenceInMinutes;
                        sb.Append(difference < 1 && difference > -1 ? "< 1" : "~ " + Math.Abs(difference).ToString());
                        sb.Append(Math.Abs(difference) > 1 ? " minutes " : " minute ");
                        sb.Append(difference >= 0 ? "late" : "early");
                    }
                    Console.WriteLine(sb.ToString());
                }
            }
        }

        public static void ShowUpcomingDepartures(Station station, List<Train> trains)
        {
            var upcomingDepartureTimes = new List<(Train, TimetableRow)>();
            var stationShortCode = station.stationShortCode;

            foreach (var t in trains)
            {
                var departure = SearchForTimetableRow(stationShortCode, t.timeTableRows, TimetableRowType.Departure);
                if (departure.Length != 0)
                {
                    if (departure[0].scheduledTime > DateTime.Now.ToUniversalTime() || departure[0].liveEstimateTime > DateTime.Now)
                    {
                        upcomingDepartureTimes.Add((t, departure[0]));
                    }
                }
            }

            var sortUpcomingDepartures = from tuple in upcomingDepartureTimes
                                         orderby tuple.Item2.scheduledTime
                                         select tuple;

            upcomingDepartureTimes = sortUpcomingDepartures.ToList();

            Console.WriteLine();
            var header = $"Upcoming departures from {station.stationName}";
            Console.WriteLine(header);
            Console.WriteLine("".PadLeft(header.Length).Replace(" ", "="));
            Console.WriteLine("Train:    To:             Time:");
            foreach (var tuple in upcomingDepartureTimes)
            {
                var train = tuple.Item1;
                var sb = new StringBuilder();
                var departure = SearchForTimetableRow(stationShortCode, train.timeTableRows, TimetableRowType.Departure);
                if (departure.Length != 0)
                {
                    sb.Append(TrainName(train).PadRight(10));
                    var toStation = stationDictionary[train.timeTableRows[train.timeTableRows.Count - 1].stationShortCode].stationName;
                    sb.Append(toStation.PadRight(16));
                    sb.Append(departure[0].scheduledTime.ToLocalTime().ToLongTimeString().PadRight(7));
                    if (departure[0].scheduledTime.Date != DateTime.Today)
                    {
                        sb.Append(" (");
                        sb.Append(departure[0].scheduledTime.ToShortDateString());
                        sb.Append(")");
                    }
                    if (departure[0].liveEstimateTime != DateTime.MinValue && departure[0].liveEstimateTime != departure[0].scheduledTime && departure[0].differenceInMinutes > 0)
                    {
                        sb.Append("  =>  ");
                        sb.Append(departure[0].liveEstimateTime.ToLocalTime().ToLongTimeString().PadRight(7));
                        if (departure[0].liveEstimateTime.Date != DateTime.Today)
                        {
                            sb.Append(" (");
                            sb.Append(departure[0].liveEstimateTime.ToShortDateString());
                            sb.Append(")");
                        }
                        sb.Append(" ");
                        var difference = departure[0].differenceInMinutes;
                        sb.Append(difference < 1 && difference > -1 ? "< 1" : "~ " + Math.Abs(difference).ToString());
                        sb.Append(Math.Abs(difference) > 1 ? " minutes " : " minute ");
                        sb.Append(difference >= 0 ? "late" : "early");
                    }
                    Console.WriteLine(sb.ToString());
                }
            }
        }

        public static void ShowPastDepartures(Station station, List<Train> trains)
        {
            var pastDepartureTimes = new List<(Train, TimetableRow)>();
            var stationShortCode = station.stationShortCode;

            foreach (var t in trains)
            {
                var departure = SearchForTimetableRow(stationShortCode, t.timeTableRows, TimetableRowType.Departure);
                if (departure.Length != 0)
                {
                    if (departure.Last().scheduledTime < DateTime.Now.ToUniversalTime() && departure.Last().liveEstimateTime < DateTime.Now)
                    {
                        pastDepartureTimes.Add((t, departure.Last()));
                    }
                }
            }

            var sortPastDepartures = from tuple in pastDepartureTimes
                                     orderby tuple.Item2.scheduledTime descending
                                     select tuple;

            pastDepartureTimes = sortPastDepartures.ToList();

            Console.WriteLine();
            var header = $"Past departures from {station.stationName}";
            Console.WriteLine(header);
            Console.WriteLine("".PadLeft(header.Length).Replace(" ", "="));
            Console.WriteLine("Train:    To:             Time:");
            foreach (var tuple in pastDepartureTimes)
            {
                var train = tuple.Item1;
                var sb = new StringBuilder();
                var departure = SearchForTimetableRow(stationShortCode, train.timeTableRows, TimetableRowType.Departure);
                if (departure.Length != 0)
                {
                    sb.Append(TrainName(train).PadRight(10));
                    var fromStation = stationDictionary[train.timeTableRows[train.timeTableRows.Count - 1].stationShortCode].stationName;
                    sb.Append(fromStation.PadRight(16));
                    sb.Append(departure.Last().scheduledTime.ToLocalTime().ToLongTimeString().PadRight(7));
                    if (departure.Last().scheduledTime.Date != DateTime.Today)
                    {
                        sb.Append(" (");
                        sb.Append(departure.Last().scheduledTime.ToShortDateString());
                        sb.Append(")");
                    }
                    if (departure.Last().actualTime != DateTime.MinValue && departure.Last().actualTime != departure.Last().scheduledTime && departure.Last().differenceInMinutes > 0)
                    {
                        sb.Append("  =>  ");
                        sb.Append(departure.Last().actualTime.ToLocalTime().ToLongTimeString().PadRight(7));
                        if (departure.Last().actualTime.Date != DateTime.Today)
                        {
                            sb.Append(" (");
                            sb.Append(departure.Last().actualTime.ToShortDateString());
                            sb.Append(")");
                        }
                        sb.Append(" ");
                        var difference = departure.Last().differenceInMinutes;
                        sb.Append(difference < 1 && difference > -1 ? "< 1" : "~ " + Math.Abs(difference).ToString());
                        sb.Append(Math.Abs(difference) > 1 ? " minutes " : " minute ");
                        sb.Append(difference >= 0 ? "late" : "early");
                    }
                    Console.WriteLine(sb.ToString());
                }
            }
        }
        static TimetableRow[] SearchForTimetableRow(string shortCode, List<TimetableRow> rows, TimetableRowType rowType)
        {
            var query = from row in rows
                        where row.stationShortCode == shortCode && row.type == (rowType == TimetableRowType.Arrival ? "ARRIVAL" : "DEPARTURE")
                        select row;
            return query.ToArray();
        }

        static string TrainName(Train train)
        {
            return train.trainCategory == "Commuter" ? train.commuterLineID : train.trainType + " " + train.trainNumber;
        }
    }
}
