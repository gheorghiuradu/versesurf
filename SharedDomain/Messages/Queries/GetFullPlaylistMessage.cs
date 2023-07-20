namespace SharedDomain.Messages.Queries
{
    public class GetFullPlaylistMessage : BaseMessage
    {
        public string PlaylistId { get; set; }
        public PlaylistOptions PlaylistOptions { get; set; }
    }
}