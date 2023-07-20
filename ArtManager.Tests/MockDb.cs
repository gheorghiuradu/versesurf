using MusicDbApi.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ArtManager.Tests
{
    public class MockDb
    {
        public MockDb()
        {
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var jsonPath = Path.Combine(assemblyDirectory, "playlists.json");
            var json = File.ReadAllText(jsonPath);
            this.Playlists = JsonConvert.DeserializeObject<List<Playlist>>(json);
        }

        public List<Playlist> Playlists { get; }
    }
}