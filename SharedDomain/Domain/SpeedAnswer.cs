using System;

namespace SharedDomain
{
    public class SpeedAnswer
    {
        public string Id { get; set; }
        public Player Player { get; set; }
        public string Name { get; set; }
        public DateTime ReceivedAtUTC { get; set; }
    }
}