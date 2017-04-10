using System;
using System.Net;
using System.Net.Sockets;

namespace Warsim.API.Authentication.External
{
    public abstract class ExternalAuthenticator
    {
        public static readonly string Domain = "http://e3fb190c.ngrok.io";

        public static readonly string RedirectUri = $"{Domain}/api/account/register/external";

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("Local IP Address Not Found!");
        }
    }
}