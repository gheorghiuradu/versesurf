using System.Threading;

namespace LicensingService
{
    public class TryGetLicenseIdCommand
    {
        public string[] Artists { get; set; }
        public string Title { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}