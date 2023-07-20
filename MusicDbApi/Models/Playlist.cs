using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace MusicDbApi.Models
{
    [FirestoreData]
    public class Playlist
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string SpotifyId { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public bool Enabled { get; set; } = true;

        [FirestoreProperty]
        public bool Featured { get; set; }

        [FirestoreProperty]
        public string PictureUrl { get; set; }

        [FirestoreProperty]
        public List<Song> Songs { get; set; }

        [FirestoreProperty]
        public int Votes { get; set; }

        [FirestoreProperty]
        public string Language { get; set; }

        [FirestoreProperty]
        public int Plays { get; set; }

        [FirestoreDocumentCreateTimestamp]
        public DateTime AddedAt { get; set; }

        [FirestoreDocumentUpdateTimestamp]
        public DateTime UpdatedAt { get; set; }
    }
}