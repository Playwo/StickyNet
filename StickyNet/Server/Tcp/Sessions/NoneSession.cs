using System;
using NetCoreServer;

namespace StickyNet.Server.Tcp
{
    public class NoneSession : TcpSession, IProtocol
    {
        public string Name => "None";

        public NoneSession(TcpServer server) 
            : base(server)
        {
        }

        protected override void OnConnected() => Disconnect();
    }
}
