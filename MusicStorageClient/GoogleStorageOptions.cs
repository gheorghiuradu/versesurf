using System.IO;

namespace MusicStorageClient
{
    public class GoogleStorageOptions
    {
        public string BasePath { get; set; }
        public string SongPreviewsPrefix { get; set; }
        public string PlaylistImgPrefix { get; set; }

        public GoogleStorageOptions()
        {
        }

        public void EnsureDirectoriesExist()
        {
            if (!string.IsNullOrWhiteSpace(BasePath))
            {
                var songPreviewsPath = Path.Combine(BasePath, SongPreviewsPrefix ?? "");
                var playlistImgPath = Path.Combine(BasePath, PlaylistImgPrefix ?? "");

                Directory.CreateDirectory(songPreviewsPath);
                Directory.CreateDirectory(playlistImgPath);
            }
        }
    }
}