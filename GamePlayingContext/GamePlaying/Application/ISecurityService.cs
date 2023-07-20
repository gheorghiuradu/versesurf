using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Application
{
    public interface ISecurityService
    {
        bool AuthenticatesHost(string connectionId);
        bool AuthenticatesGuest(string connectionId);
    }
}
