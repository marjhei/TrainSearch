using RataDigiTraffic.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Trains
{
    class ConsoleUI
    {

        public static void StartMenu()
        {

            bool repeat = true;

            while (repeat)
            {


                Console.Title = "UI";
                Console.WriteLine("");
                Console.WriteLine("");
                string title = @"
                

++                                                    |
|        ▀▄▀▄▀▄▀▄▀▄▀▄ TrainSearch ▄▀▄▀▄▀▄▀▄▀▄▀        |
|                                                     |
+-----------------------------------------------------+
|                                                     |
|                                                     |
|                                                     |
|    1. Search trains between start & end stations    |
|                                                     |
|    2. Information from specific train route         |
|                                                     |
|    3. Get train distance from station               |
|                                                     |
|    4. Station's next arrivals / departures          |
|                                                     |
|    5. Station's arrivals / departures               |
|       in +- 15min.                                  |
|                                                     |
|   Press Esc. to close console application           |
|                                                     |
+-----------------------------------------------------+
";


                Console.WriteLine(title);
                ConsoleKeyInfo switchKey = Console.ReadKey();
                Console.Clear();



                switch (switchKey.Key)
                {
                    case ConsoleKey.D1:
                        UserInput(); // trains between stations
                        break;
                    case ConsoleKey.NumPad1:
                        UserInput();  // trains between stations
                        break;
                    case ConsoleKey.D2:
                        var trainNumber = SearchLogic.GetTrainNumber(); // train route via train number
                        SearchLogic.GetTrainRoute(trainNumber);
                        break;
                    case ConsoleKey.NumPad2:
                        var trainNumber02 = SearchLogic.GetTrainNumber(); // train route via train number
                        SearchLogic.GetTrainRoute(trainNumber02);
                        break;
                    case ConsoleKey.D3:
                        UserInputStation(); // distance from station to train
                        break;
                    case ConsoleKey.NumPad3:
                        UserInputStation(); // distance from station to train
                        break;
                    case ConsoleKey.D4:
                        UserInputStationInfoWithLimit(); // station info with 5 trains
                        break;
                    case ConsoleKey.NumPad4:
                        UserInputStationInfoWithLimit(); // station info with 5 trains
                        break;
                    case ConsoleKey.D5:
                        UserInputStationInfoWithTime(); // station info with +- 15min time window
                        break;
                    case ConsoleKey.NumPad5:
                        UserInputStationInfoWithTime(); // station info with +- 15min time window
                        break;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("Not a valid option.");
                        break;
                        
                }
                Console.WriteLine("Press ESC to go back to main menu.");
                Console.ReadKey();
                Console.Clear();


            }

        }


        public static void UserInput()
        {

            Console.WriteLine("Welcome to TrainSearch!");
            Console.WriteLine("Search trains between start & end stations.");

            Console.WriteLine("");

            Console.Write("from:");

            string from = Console.ReadLine().ToUpper().Trim();

            Console.Write("to:");

            string to = Console.ReadLine().ToUpper().Trim();

            Console.WriteLine("");

            try
            {
                var fromStation = SearchLogic.ConvertUserInputStringToStation(from);
                var toStation = SearchLogic.ConvertUserInputStringToStation(to);
                SearchLogic.SearchBetweenStations(fromStation, toStation);
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }


            Console.WriteLine("");





        }

        public static void UserInputStation()
        {

            Console.WriteLine("Welcome to TrainSearch!");
            Console.WriteLine("Get train distance from station");
            Console.WriteLine("");
            
            Console.Write("Select station:");

            string station = Console.ReadLine().ToUpper().Trim();

            try
            {
                var distfromStation = SearchLogic.ConvertUserInputStringToStation(station);
                var trainNumber = SearchLogic.GetTrainNumber();
                SearchLogic.GetTrainDistanceFromStation(distfromStation, trainNumber);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            Console.WriteLine("");


        }
        public static void UserInputStationInfoWithLimit()
        {

            Console.WriteLine("Welcome to TrainSearch!");
            Console.WriteLine("Station's next arrivals / departures");
            Console.WriteLine("");

            
            Console.Write("Select station:");

            string station = Console.ReadLine().ToUpper().Trim();

            try
            {
                var stationLocation = SearchLogic.ConvertUserInputStringToStation(station);
                var list = SearchLogic.CurrentStationInfoWithLimit(stationLocation);
                SearchLogic.ShowUpcomingArrivals(stationLocation, list);
                SearchLogic.ShowPastArrivals(stationLocation, list);
                SearchLogic.ShowUpcomingDepartures(stationLocation, list);
                SearchLogic.ShowPastDepartures(stationLocation, list);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("");




        }
        public static void UserInputStationInfoWithTime()
        {

            Console.WriteLine("Welcome to TrainSearch!");
            Console.WriteLine("Station's next arrivals / departures");
            Console.WriteLine("in +- 15min.");

            Console.WriteLine("");

            
            Console.Write("Select station:");

            string station = Console.ReadLine().ToUpper().Trim();

            try
            {
                var stationLocation = SearchLogic.ConvertUserInputStringToStation(station);
                var list = SearchLogic.CurrentStationInfoWithTime(stationLocation);
                SearchLogic.ShowUpcomingArrivals(stationLocation, list);
                SearchLogic.ShowPastArrivals(stationLocation, list);
                SearchLogic.ShowUpcomingDepartures(stationLocation, list);
                SearchLogic.ShowPastDepartures(stationLocation, list);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("");


        }
    }
}