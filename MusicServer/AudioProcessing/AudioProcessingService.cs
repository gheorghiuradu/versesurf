using NAudio.Wave;
using NLayer.NAudioSupport;
using System;
using System.IO;

namespace AudioProcessing
{
    public class AudioProcessingService
    {
        public Stream TrimClip(Stream mp3Stream, double startSecond, double endSecond)
        {
            if (endSecond <= startSecond)
            {
                throw new ArgumentOutOfRangeException("end", "end should be greater than begin");
            }

            var outputStream = new MemoryStream();
            mp3Stream.Position = 0;
            var builder = new Mp3FileReader.FrameDecompressorBuilder(wf => new Mp3FrameDecompressor(wf));

            using (var reader = new Mp3FileReader(mp3Stream, builder))
            {
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    if (reader.CurrentTime.TotalSeconds >= startSecond)
                    {
                        if (reader.CurrentTime.TotalSeconds <= endSecond)
                            outputStream.Write(frame.RawData, 0, frame.RawData.Length);
                        else break;
                    }
                }
            }

            outputStream.Position = 0;

            return outputStream;
        }
    }
}