namespace SharedDomain.Domain
{
    public interface IPlaylistViewModel
    {
        string Id { get; set; }
        string Name { get; set; }
        bool Featured { get; set; }
        string PictureUrl { get; set; }
        string PictureHash { get; set; }
    }
}