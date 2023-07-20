namespace SharedDomain.Domain
{
    public class SongViewModel
    {
        public string Id { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string PreviewUrl { get; set; }
        public string Snippet { get; set; }
        public bool IsExplicit { get; set; }
        public string PreviewHash { get; set; }
        public float? StartSecond { get; set; }
        public float? EndSecond { get; set; }
    }
}