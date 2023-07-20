using System;
using System.Collections.Generic;

namespace Assets.Scripts.Serialization
{
    [System.Serializable]
    public class AppSettings
    {
        public string ServerUrl { get; set; }
        public string WebClientUrl { get; set; }

        public string HubUrl => $"{this.ServerUrl}/ws/gamehub";
        public string ApiUrl => $"{this.ServerUrl}/api";
        public string GenericAlbumImageUrl => $"{this.ServerUrl}/img/generic-album.png";
        public Uri WebClientUri => new Uri(this.WebClientUrl);
        public List<string> AvailableColorCodes;
        public List<string> AvailableCharacters;
    }
}