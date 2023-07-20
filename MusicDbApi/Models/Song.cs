using Google.Cloud.Firestore;
using System;

namespace MusicDbApi.Models
{
    [FirestoreData]
    public class Song
    {
        [FirestoreDocumentId]
        public string PlaylistId { get; set; }

        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string SpotifyId { get; set; }

        [FirestoreProperty]
        public string ISRC { get; set; }

        [FirestoreProperty]
        public string Artist { get; set; }

        [FirestoreProperty]
        public string Title { get; set; }

        [FirestoreProperty]
        public bool IsExplicit { get; set; }

        [FirestoreProperty]
        public string Snippet { get; set; }

        [FirestoreProperty]
        public int Plays { get; set; }

        [FirestoreProperty]
        public bool Enabled { get; set; }

        [FirestoreProperty]
        public string FullAudioUrl { get; set; }

        [FirestoreProperty]
        public string PreviewUrl { get; set; }

        [FirestoreProperty]
        public string BmiLicenseId { get; set; }

        [FirestoreProperty]
        public string ASCAPLicenseId { get; set; }

        [FirestoreProperty]
        public string SesacLicenseId { get; set; }

        [FirestoreProperty]
        public float? StartSecond { get; set; }

        [FirestoreProperty]
        public float? EndSecond { get; set; }

        public DateTime AddedAt { get; set; }

        public DateTime ModifiedAt { get; set; }
    }
}