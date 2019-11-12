﻿using System;
using System.Net;
using System.Runtime.CompilerServices;
using StickyNet.Service;

namespace StickyNet.Server
{
    public interface IStickyServer
    {
        public EndPoint EndPoint { get; }
        public int Port { get; }
        public StickyServerConfig Config { get; }

        public event Action<IPAddress, ConnectionAttempt> CatchedIpAdress;

        public bool Start();
        public bool Stop();

        public void Dispose();
    }
}
