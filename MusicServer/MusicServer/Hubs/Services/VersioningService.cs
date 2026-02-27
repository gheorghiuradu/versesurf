using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicServer.Hubs.Services
{
    public class VersioningService
    {
        private readonly IEnumerable<string> supportedVersions;

        public VersioningService(IConfiguration configuration)
        {
            try
            {
                this.supportedVersions = configuration["SupportedVersions"].Split(";");
            }
            catch (Exception ex)
            {
                // TODO: log
                this.supportedVersions = new List<string>();
            }
        }

        public bool IsVersionSupported(string version)
        {
            // Check only for major version support
            return supportedVersions.Contains(version.Split(".").FirstOrDefault());
        }
    }
}