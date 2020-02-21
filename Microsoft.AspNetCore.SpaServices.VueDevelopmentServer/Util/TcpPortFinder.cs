using System.Net;
using System.Net.Sockets;

namespace Microsoft.AspNetCore.SpaServices.VueDevelopmentServer.Util
{
    internal static class TcpPortFinder
    {
        public static int FindAvailablePort()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            try
            {
                return ((IPEndPoint) tcpListener.LocalEndpoint).Port;
            }
            finally
            {
                tcpListener.Stop();
            }
        }
    }
}