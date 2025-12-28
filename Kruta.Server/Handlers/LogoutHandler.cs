using Kruta.Protocol;
using Kruta.Server.Handlers.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Server.Handlers
{
    public class LogoutHandler : IPacketHandler
    {
        public void Handle(ClientObject client, EAPacket packet)
        {
            //Вся логика тут 
            client.Close();
        }
    }
}
