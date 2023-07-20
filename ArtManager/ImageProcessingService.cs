using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Threading.Tasks;

namespace ArtManager
{
    public static class ImageProcessingService
    {
        public static int DefaultWidth { get; set; } = 300;
        public static int DefaultHeight { get; set; } = 300;

        public static void ResizeImage(string imagePath)
        {
            using (var image = Image.Load(imagePath))
            {
                image.Mutate(i => i.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(DefaultWidth, DefaultWidth)
                }));
                image.Save(imagePath);
            }
        }

        public static async Task<Stream> ResizeImageAsync(Stream imageStream)
        {
            var imageResult = await Image.LoadWithFormatAsync(imageStream);
            imageResult.Image.Mutate(i => i.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(DefaultWidth, DefaultHeight)
            }));
            var resizedStream = new MemoryStream();
            imageResult.Image.Save(resizedStream, imageResult.Format);
            imageResult.Image.Dispose();

            return resizedStream;
        }
    }
}