using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Threading.Tasks;

namespace ArtManager
{
    public class SpotifyBuilder
    {
        private const string ClientId = "INSERT CLIENT ID";
        private const string ClientSecret = "INSERT CLIENT SECRET";

        public async Task<SpotifyWebAPI> BuildDefaultAsync()
        {
            var spotifyAuth = new CredentialsAuth(ClientId, ClientSecret);
            var spotifyToken = await spotifyAuth.GetToken();
            return new SpotifyWebAPI { TokenType = spotifyToken.TokenType, AccessToken = spotifyToken.AccessToken };
        }
    }
}