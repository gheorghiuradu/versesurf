using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;

namespace YoutubeDL
{
    public class YoutubeDLService
    {
        private readonly YoutubeClient youtube = new YoutubeClient();

        public YoutubeDLService()
        {
            var ffmpeg = "ffmpeg";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ffmpeg += "_linux";
                File.Move(Path.Combine(this.GetAssemblyDirectory(), ffmpeg), Path.Combine(this.GetAssemblyDirectory(), "ffmpeg"));
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ffmpeg += "_mac";
                File.Move(Path.Combine(this.GetAssemblyDirectory(), ffmpeg), Path.Combine(this.GetAssemblyDirectory(), "ffmpeg"));
            }
        }

        public async ValueTask<Stream> GetAudioStreamAsync(string videoUrl, CancellationToken token = default)
        {
            var id = new VideoId(videoUrl);
            var filePath = Path.Combine(this.GetAssemblyDirectory(), $"{id.Value}.mp3");
            await this.youtube.Videos.DownloadAsync(id, filePath, cancellationToken: token);

            var outputStream = new MemoryStream();
            using (var file = File.OpenRead(filePath))
            {
                await file.CopyToAsync(outputStream);
            }
            outputStream.Seek(0, SeekOrigin.Begin);

            File.Delete(filePath);

            return outputStream;
        }

        private string GetAssemblyDirectory()
            => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    }
}