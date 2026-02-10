using System;
using System.ComponentModel.DataAnnotations;

namespace MusicEventDbApi
{
    public class MusicEvent
    {
        [Key]
        public int Id { get; set; }

        public string Sender { get; set; }

        public DateTime TimeStamp { get; set; }

        public string EventType { get; set; }

        public string PayloadJson { get; set; }

        public string PayloadName { get; set; }

        public string PayloadType { get; set; }
    }
}