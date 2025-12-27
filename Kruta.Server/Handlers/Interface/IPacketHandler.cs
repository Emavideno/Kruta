using Kruta.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Server.Handlers.Interface
{
    public interface IPacketHandler
    {
        public void Handle(ClientObject client, EAPacket packet);
    }
}
