using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class EmptyTcpProtocol : ITcpProtocol
    {
        public string Name => "None";

        public Task<bool> PerformHandshakeAsync(TcpServer server, TcpSession session)
        {
            session.SendAsync("You've been catched in a StickyNet!");
            session.Disconnect();

            return Task.FromResult(true);
        }
    }
}
