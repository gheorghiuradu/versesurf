namespace SharedDomain.Domain
{
    public interface IPlaylistViewModel
    {
        string Id { get; set; }
        string Name { get; set; }
        string PictureUrl { get; set; }
        bool Featured { get; set; }
    }
}