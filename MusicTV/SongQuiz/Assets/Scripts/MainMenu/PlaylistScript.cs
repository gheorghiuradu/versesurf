using Assets.Scripts.Services;
using SharedDomain.Domain;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaylistScript : MonoBehaviour
{
    private const string PlaylistPrefabPath = "Prefabs/Playlist";

    public TextMeshProUGUI AlbumName;

    public string Id { get; private set; }

    public static async Task<PlaylistScript> InstantiateAsync(IPlaylistViewModel playlist, Transform parent)
    {
        var prefab = Resources.Load<GameObject>(PlaylistPrefabPath);
        var script = GameObject.Instantiate(prefab, parent).GetComponent<PlaylistScript>();
        await script.InitializeAsync(playlist);

        return script;
    }

    public static PlaylistScript InstantiateLocal(string name, Transform parent)
    {
        var prefab = Resources.Load<GameObject>(PlaylistPrefabPath);
        var script = GameObject.Instantiate(prefab, parent).GetComponent<PlaylistScript>();
        script.InitializeLocal(name);

        return script;
    }

    public async Task InitializeAsync(IPlaylistViewModel playlist)
    {
        var coverImage = this.transform.Find("CoverImage").gameObject;
        coverImage.GetComponent<Image>().sprite =
        await ServiceProvider.Get<CacheService>().HandlePlaylistImageAsync(playlist.PictureUrl, playlist.PictureHash);

        this.Id = playlist.Id;
        this.AlbumName.text = playlist.Name;
        //var image = coverImage.GetComponent<Image>();
        //image.type = Image.Type.Simple;
        //image.preserveAspect = true;
    }

    public async Task InitializeAsync(IPlaylistViewModel playlist, SongViewModel song)
    {
        var coverImage = this.transform.Find("CoverImage").gameObject;
        coverImage.GetComponent<Image>().sprite =
        await ServiceProvider.Get<CacheService>().HandlePlaylistImageAsync(playlist.PictureUrl, playlist.PictureHash);

        this.Id = playlist.Id;
        this.AlbumName.text = $"{song.Artist} - {song.Title}\n{playlist.Name}";
        //var image = coverImage.GetComponent<Image>();
        //image.type = Image.Type.Simple;
        //image.preserveAspect = true;
    }

    public void InitializeLocal(string name, Sprite sprite = null)
    {
        if (!(sprite is null))
        {
            var coverImage = this.transform.Find("CoverImage").gameObject;
            coverImage.GetComponent<Image>().sprite = sprite;
        }
        this.AlbumName.text = name;
    }
}