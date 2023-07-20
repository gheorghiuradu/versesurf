using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SignalrCoreWrapper
{
    public class MusicHubClient
    {
        private readonly HubConnection hubConnection;

        public MusicHubClient(string hubUrl)
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .AddJsonProtocol(opt =>
                {
                    opt.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    opt.PayloadSerializerOptions.IgnoreNullValues = true;
                })
                .Build();

            this.hubConnection.Closed += error =>
            {
                this.Disconnected?.Invoke(error);
                return Task.CompletedTask;
            };
            this.hubConnection.Reconnecting += error =>
            {
                this.Reconnecting?.Invoke(error);
                return Task.CompletedTask;
            };
            this.hubConnection.Reconnected += newId =>
            {
                this.Reconnected?.Invoke(newId);
                return Task.CompletedTask;
            };
        }

        public event Action<Exception> Disconnected;

        public event Action<Exception> Reconnecting;

        public event Action<string> Reconnected;

        public string ConnectionId => this.hubConnection.ConnectionId;

        public ConnectionState State => (ConnectionState)Enum.Parse
            (typeof(ConnectionState), this.hubConnection.State.ToString());

        public Task ConnectAsync()
        {
            return this.hubConnection.StartAsync();
        }

        public Task DisconnectAsync()
        {
            return this.hubConnection.StopAsync();
        }

        public IDisposable On<T>(string method, Action<T> handler)
        {
            return hubConnection.On(method, handler);
        }

        public IDisposable On(string method, Action handler)
        {
            return this.hubConnection.On(method, handler);
        }

        public Task<T> InvokeAsync<T>(string method)
        {
            return hubConnection.InvokeAsync<T>(method);
        }

        public Task<T> InvokeAsync<T>(string method, object arg1)
        {
            return hubConnection.InvokeAsync<T>(method, arg1);
        }

        public Task<T> InvokeAsync<T>(string method, object arg1, object arg2)
        {
            return hubConnection.InvokeAsync<T>(method, arg1, arg2);
        }

        public Task InvokeAsync(string method)
        {
            return hubConnection.InvokeAsync(method);
        }

        public Task InvokeAsync(string method, object arg1)
        {
            return hubConnection.InvokeAsync(method, arg1);
        }

        public Task InvokeAsync(string method, object arg1, object arg2)
        {
            return hubConnection.InvokeAsync(method, arg1, arg2);
        }

        public Task SendAsync(string method)
        {
            return hubConnection.SendAsync(method);
        }

        public Task SendAsync(string method, object arg1)
        {
            return hubConnection.SendAsync(method, arg1);
        }

        public enum ConnectionState
        {
            //
            // Summary:
            //     The hub connection is disconnected.
            Disconnected = 0,

            //
            // Summary:
            //     The hub connection is connected.
            Connected = 1,

            //
            // Summary:
            //     The hub connection is connecting.
            Connecting = 2,

            //
            // Summary:
            //     The hub connection is reconnecting.
            Reconnecting = 3
        }
    }
}