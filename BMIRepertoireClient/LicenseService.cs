using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("LicensingService.Tests")]

namespace LicensingService
{
    public class LicenseService
    {
        private readonly BMIClient bMIClient = new BMIClient();
        private readonly AscapClient ascapClient = new AscapClient();
        private readonly SesacClient sesacClient = new SesacClient();

        public Task<string> TryGetBmiLicenseAsync(TryGetLicenseIdCommand command)
        {
            return
                this.bMIClient.TryGetBMIWorkNumberAsync(command.Artists, command.Title, command.CancellationToken);
        }

        public Task<string> TryGetAscapLicenseAsync(TryGetLicenseIdCommand command)
        {
            return this.ascapClient.TryGetAscapLicenseAsync(command.Artists, command.Title, command.CancellationToken);
        }

        public Task<string> TryGetSesacLicenseAsync(TryGetLicenseIdCommand command)
        {
            return this.sesacClient.TryGetSesacLicenseAsync(command.Artists, command.Title, command.CancellationToken);
        }
    }
}