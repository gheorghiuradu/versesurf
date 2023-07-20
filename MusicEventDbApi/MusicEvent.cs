using Google.Cloud.Firestore;
using System;

namespace MusicEventDbApi
{
    [FirestoreData]
    public class MusicEvent
    {
        [FirestoreProperty]
        public string Sender { get; set; }

        [FirestoreProperty]
        public DateTime TimeStamp { get; set; }

        [FirestoreProperty]
        public string EventType { get; set; }

        [FirestoreProperty]
        public string PayloadJson { get; set; }

        [FirestoreProperty]
        public string PayloadName { get; set; }

        [FirestoreProperty]
        public string PayloadType { get; set; }
    }
}