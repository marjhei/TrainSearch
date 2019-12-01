using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using RataDigiTraffic.Model;

namespace Trains
{
    static class CommandLineUI
    {
        public static void RunFromCommandLine(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<BetweenOptions, RouteOptions, DistanceOptions, CurrentStationInfoOptions, NextStationInfoOptions>(args)
                .MapResult(
                    (BetweenOptions opts) => RunBetweenStations(opts),
                    (RouteOptions opts) => RunTrainRoute(opts),
                    (DistanceOptions opts) => RunTrainDistance(opts),
                    (CurrentStationInfoOptions opts) => RunCurrentStationInfo(opts),
                    (NextStationInfoOptions opts) => RunNextStationInfo(opts),
                    errs => 1);
        }

        static int RunBetweenStations(BetweenOptions opts)
        {
            try
            {
                var fromStation = SearchLogic.ConvertUserInputStringToStation(opts.FromStation);
                var toStation = SearchLogic.ConvertUserInputStringToStation(opts.ToStation);
                var limit = opts.Limit;
                if (limit != 0)
                {
                    SearchLogic.SearchBetweenStations(fromStation, toStation, limit);
                }
                else
                {
                    SearchLogic.SearchBetweenStations(fromStation, toStation);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return 1;
        }

        static int RunCurrentStationInfo(CurrentStationInfoOptions opts)
        {
            try
            {
                var station = SearchLogic.ConvertUserInputStringToStation(opts.Station);
                var limit = opts.Limit;
                var trains = new List<Train>();
                if (limit != 0)
                {
                    trains = SearchLogic.CurrentStationInfoWithLimit(station, limit);
                }
                else
                {
                    trains = SearchLogic.CurrentStationInfoWithLimit(station);
                }
                ShowStationData(station, trains, opts.showPast);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return 1;
        }

        static int RunNextStationInfo(NextStationInfoOptions opts)
        {
            try
            {
                var station = SearchLogic.ConvertUserInputStringToStation(opts.Station);
                var minutes = opts.Minutes;
                var trains = new List<Train>();
                if (minutes != 0)
                {
                    trains = SearchLogic.CurrentStationInfoWithTime(station, minutes, minutes, minutes, minutes);
                }
                else
                {
                    trains = SearchLogic.CurrentStationInfoWithTime(station);
                }
                ShowStationData(station, trains, opts.showPast);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return 1;
        }

        static int RunTrainRoute(RouteOptions opts)
        {
            try
            {
                var trainNum = SearchLogic.GetTrainNumber(opts.TrainNumber);
                SearchLogic.GetTrainRoute(trainNum);
                return 1;
            }
            catch (FormatException)
            {
                return 1;
            }
        }

        static int RunTrainDistance(DistanceOptions opts)
        {
            try
            {
                var trainNum = SearchLogic.GetTrainNumber(opts.TrainNumber);
                var station = SearchLogic.ConvertUserInputStringToStation(opts.Station);
                SearchLogic.GetTrainDistanceFromStation(station, trainNum);
            }
            catch(ArgumentException)
            {
                Console.WriteLine("Station is not valid. Please try again.");
            }
            catch (FormatException)
            {
                Console.WriteLine("Train number is not valid. Please try again.");
            }
            return 1;

        }
        static void ShowStationData(Station station, List<Train> trains, bool showPast)
        {
            SearchLogic.ShowUpcomingDepartures(station, trains);
            if (showPast)
            {
                SearchLogic.ShowPastDepartures(station, trains);
            }
            SearchLogic.ShowUpcomingArrivals(station, trains);
            if (showPast)
            {
                SearchLogic.ShowPastArrivals(station, trains);
            }
        }
    }

    [Verb("between", HelpText = "Search for train connections between two stations. Syntax: \"between <FROM STATION> <TO STATION>\"")]
    class BetweenOptions
    {
        [Value(0, MetaName = "From", HelpText = "The name or shortcode of station.", Required = true)]
        //[Option('f', "from", Required = true, HelpText = "The station from which trains are searched.")]
        public string FromStation { get; set; }

        [Value(1, MetaName = "To", HelpText = "The name or shortcode of station.", Required = true)]
        //[Option('t', "to", Required = true, HelpText = "The station to which trains are searched.")]
        public string ToStation { get; set; }

        [Option('l', "limit", Required = false, HelpText = "Parameter to limit the number of results. Default is 5.")]
        public int Limit { get; set; }
    }

    [Verb("station", HelpText = "Show current info at a specific station. Syntax: \"station <STATION NAME>\"")]
    class CurrentStationInfoOptions
    {
        // [Option('s', "station", Required = true, HelpText = "Name of the station whose information is shown.")]
        [Value(0, MetaName = "Station name", HelpText = "The name or shortcode of station whose information is to be displayed.", Required = true)]
        public string Station { get; set; }

        [Option('l', "limit", Required = false, HelpText = "How many results are shown. Default 5.")]
        public int Limit { get; set; }

        [Option('p', "past", Required = false, HelpText = "Show also past departures and arrivals. ")]
        public bool showPast { get; set; }
    }

    [Verb("route", HelpText = "Get the route for a specific train number. Syntax: \"route <TRAIN NUMBER>\"")] 
    class RouteOptions
    {
        [Value(0, Required = true, MetaName = "Train number", HelpText = "The number of the train. May be in the form 'IC47' or '47'.")]
        //[Option('n', "train-number", Required = true, HelpText = "The number of the train. May be in the form 'IC47' or '47'.")]
        public string TrainNumber { get; set; }

    }

    [Verb("distance", HelpText = "Get the distance of a train from your station if the station is on the train's route and the train has not yet passed the station. Syntax: \"distance <STATION NAME> <TRAIN NUMBER>\"")]
    class DistanceOptions
    {
        [Value(0, MetaName = "Station", Required = true, HelpText = "Station where you are at.")]
        //[Option('s', "station", Required = true, HelpText = "Station.")]
        public string Station { get; set; }

        [Value(1, MetaName = "Train number", Required = true, HelpText = "The number of the train. May be in the form 'IC47' or '47'.")]
        //[Option('n', "train-number", Required = true, HelpText = "The number of the train. May be in the form 'IC47' or '47'.")]
        public string TrainNumber { get; set; }

    }

    [Verb("next", HelpText = "Show the next train information at a specific station. Syntax: \"next <STATION NAME>\"")]
    class NextStationInfoOptions
    {
        [Value(0, MetaName = "Station", Required = true, HelpText = "Name of the station whose information is to be displayed.")]
        //[Option('s', "station", Required = true, HelpText = "Name of the station whose information is shown.")]
        public string Station { get; set; }

        [Option('m', "minutes", Required = false, HelpText = "Trains shown for the next X minutes. Default 15 minutes.")]
        public int Minutes { get; set; }

        [Option('p', "past", Required = false, HelpText = "Show also past departures and arrivals.")]
        public bool showPast { get; set; }
    }
}
