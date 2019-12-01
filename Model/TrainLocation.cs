using System;
using System.Collections.Generic;
using System.Text;

namespace RataDigiTraffic.Model
{
    public class TrainLocation
    {
        public int trainNumber;
        public DateTime departureDate;
        public DateTime timestamp;
        public Coords location;
        public double speed;
    }

    public class Coords
    {
        public string type;
        public decimal[] coordinates; // [0]: longitude [1]: latitude
    }
}
