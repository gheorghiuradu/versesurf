namespace LicensingService.Models
{
    public class SESACArtistSearchRequest
    {
        public string Term { get; set; }
        public string Method { get; set; } = "get_songs_by_artist_name";
    }
}