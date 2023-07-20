using BackOffice.Web.Data;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.Threading.Tasks;

namespace BackOffice.Web.Services
{
    public class MusicServerApiClient
    {
        private readonly IRestClient restClient;

        public MusicServerApiClient(MusicServerApiClientOptions options)
        {
            this.restClient = new RestClient(options.GameControllerUrl);
            this.restClient.AddDefaultHeader("accept", "application/json");
            this.restClient.UseSerializer<JsonNetSerializer>();
        }

        public async ValueTask<bool> SendNotificationAsync(NotificationRequest notification)
        {
            var request = new RestRequest("/SendNotification");
            request.AddJsonBody(notification);

            var response = await this.restClient.ExecutePostAsync(request);

            return response.IsSuccessful;
        }
    }
}