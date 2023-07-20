using Google.Cloud.SecretManager.V1;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GcloudWebApiExtensions
{
    public class GCloudSecretProvider
    {
        private const string Latest = "latest";
        private readonly SecretManagerServiceClient client = SecretManagerServiceClient.Create();
        private readonly GCloudSecretProviderOptions options;

        public GCloudSecretProvider(GCloudSecretProviderOptions options = null)
        {
            this.options = options;
        }

        private string DecryptSecret(AccessSecretVersionResponse secret)
        {
            var base64 = secret.Payload.ToString()
                .Replace("{ \"data\": \"", string.Empty)
                .Replace("\" }", string.Empty);
            var data = Convert.FromBase64String(base64);

            return Encoding.UTF8.GetString(data);
        }

        public async ValueTask<string> GetSecretAsync(string secretName, CancellationToken token = default)
        {
            var secret = string.IsNullOrWhiteSpace(options?.SecretManagerProjectId) ?
                await this.client.AccessSecretVersionAsync(secretName, token) :
                await this.client
                    .AccessSecretVersionAsync(new SecretVersionName(options.SecretManagerProjectId, secretName, Latest), token);

            return this.DecryptSecret(secret);
        }

        public string GetSecret(string secretName)
        {
            var secret = string.IsNullOrWhiteSpace(options?.SecretManagerProjectId) ?
                this.client.AccessSecretVersion(secretName) :
                this.client.AccessSecretVersion(new SecretVersionName(options.SecretManagerProjectId, secretName, Latest));

            return this.DecryptSecret(secret);
        }
    }
}