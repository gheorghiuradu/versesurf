using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using JWT;
using JWT.Builder;
using Microsoft.IdentityModel.Tokens;
using RestSharp;
using RestSharp.Authenticators;
using SharedDomain.Extensions;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace MusicApi.Serverless.Client
{
    public class GcloudAuthenticator : IAuthenticator
    {
        private const string MetadataServerTokenUrl = "http://metadata/computeMetadata/v1/instance/service-accounts/default/identity?audience=";
        private const string GoogleAppCredentialsVarName = "GOOGLE_APPLICATION_CREDENTIALS";
        private readonly bool isProduction;

        public GcloudAuthenticator(bool isProduction)
        {
            this.isProduction = isProduction;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            var jwt = client.DefaultParameters
                .FirstOrDefault(p => p.Type == ParameterType.HttpHeader && string.Equals(p.Name, "Authorization"))
                ?.Value?.ToString()
                ?.Replace("Bearer ", string.Empty);

            if (this.IsTokenExpired(jwt))
            {
                jwt = this.RefreshTokenAsync($"{client.BaseUrl.Scheme}://{client.BaseUrl.Host}").Result;
            }

            client.RemoveDefaultParameter("Authorization");
            client.AddDefaultHeader("Authorization", $"Bearer {jwt}");
        }

        private bool IsTokenExpired(string jwt)
        {
            if (string.IsNullOrEmpty(jwt)) return true;

            try
            {
                new JwtBuilder()
                    .DoNotVerifySignature()
                    .Decode(jwt);
                return false;
            }
            catch (TokenExpiredException)
            {
                return true;
            }
        }

        private ValueTask<string> RefreshTokenAsync(string baseAddress)
        {
            return this.isProduction ?
               this.GetAccessTokenFromMetadata(baseAddress) : this.GetAccessTokenFromServiceAccount(baseAddress);
        }

        private async ValueTask<string> GetAccessTokenFromMetadata(string baseAddress)
        {
            string token;
            using (var authClient = new HttpClient())
            {
                authClient.DefaultRequestHeaders.Add("Metadata-Flavor", "Google");
                var result = await authClient.GetAsync(MetadataServerTokenUrl + baseAddress);
                if (!result.IsSuccessStatusCode)
                {
                    var ex = new Exception("Could not get token for cloud run request");
                    throw ex;
                }
                token = await result.Content.ReadAsStringAsync();
            }

            return token;
        }

        private async ValueTask<string> GetAccessTokenFromServiceAccount(string baseAddress)
        {
            ServiceAccountCredential credential;
            using (var stream = File.OpenRead(Environment.GetEnvironmentVariable(GoogleAppCredentialsVarName)))
            {
                credential = ServiceAccountCredential.FromServiceAccountData(stream);
            }
            var initialToken = this.CreateAccessToken(credential, baseAddress);
            var tokenRequest = new GoogleAssertionTokenRequest()
            {
                Assertion = initialToken
            };
            var result = await tokenRequest.ExecuteAsync(credential.HttpClient,
                credential.TokenServerUrl,
                CancellationToken.None,
                credential.Clock);

            return result.IdToken;
        }

        /// <summary>
        /// Generate a JWT signed with the service account's internal key
        /// containing a special "target_audience" claim.
        /// </summary>
        /// <param name="internalKey">The internal key string pulled from
        /// a credentials .json file.</param>
        /// <param name="iapClientId">The client id observed on
        /// https://console.cloud.google.com/apis/credentials.</param>
        /// <param name="email">The e-mail address associated with the
        /// internalKey.</param>
        /// <returns>An access token.</returns>
        private string CreateAccessToken(ServiceAccountCredential saCredential,
            string iapClientId)
        {
            var now = saCredential.Clock.UtcNow;
            var currentTime = now.ToUnixEpochDate();
            var expTime = now.AddHours(1).ToUnixEpochDate();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Aud,
                    GoogleAuthConsts.OidcTokenUrl),
                new Claim(JwtRegisteredClaimNames.Sub, saCredential.Id),
                new Claim(JwtRegisteredClaimNames.Iat, currentTime.ToString()),
                new Claim(JwtRegisteredClaimNames.Exp, expTime.ToString()),
                new Claim(JwtRegisteredClaimNames.Iss, saCredential.Id),

                // We need to generate a JWT signed with the service account's
                // internal key containing a special "target_audience" claim.
                // That claim should contain the clientId of IAP we eventually
                // want to access.
                new Claim("target_audience", iapClientId)
            };

            // Encryption algorithm must be RSA SHA-256, according to
            // https://developers.google.com/identity/protocols/OAuth2ServiceAccount
            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(saCredential.Key),
                SecurityAlgorithms.RsaSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signingCredentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}