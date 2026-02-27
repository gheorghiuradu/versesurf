using GamePlaying.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MusicServer.CustomAuth
{
    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

    public class GameAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private const string HostKeyName = "X-HostConnectionId";
        private const string PlayerKeyName = "X-PlayerConnectionId";
        private readonly ISecurityService securityService;

        public GameAuthenticationHandler(
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder urlEncoder,
            ISystemClock systemClock,
            ISecurityService securityService) :

            base(options,
                loggerFactory,
                urlEncoder,
                systemClock)
        {
            this.securityService = securityService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            ///TODO:
            /// - implement authentication for gamehub
            /// - add id as username, maybe connectionid
            /// - use groups for hub
            /// https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-3.1
            if (this.Context.Request.Path.Value.EndsWith("/ws/gamehub"))
            {
            }
            try
            {
                if (this.Request.Headers.ContainsKey(HostKeyName))
                {
                    this.Request.Headers.TryGetValue(HostKeyName, out var hostConnectionId);
                    return this.ValidateHost(hostConnectionId);
                }
                if (this.Request.Headers.ContainsKey(PlayerKeyName))
                {
                    this.Request.Headers.TryGetValue(PlayerKeyName, out var playerConnectionId);
                    return this.ValidatePlayer(playerConnectionId);
                }

                return AuthenticateResult.Fail("Unauthorized");
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }

        private AuthenticateResult ValidateHost(string connectionId)
        {
            if (!this.securityService.AuthenticatesHost(connectionId))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var identity = new GenericIdentity(connectionId);
            var principal = new GenericPrincipal(identity, new[] { Roles.Host });
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private AuthenticateResult ValidatePlayer(string connectionId)
        {
            if (!this.securityService.AuthenticatesGuest(connectionId))
            {
                return AuthenticateResult.Fail("Unauthorized");
            }

            var identity = new GenericIdentity(connectionId);
            var principal = new GenericPrincipal(identity, new[] { Roles.Player });
            var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}