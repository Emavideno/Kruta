using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.XProtocol;

namespace Kruta.Shared.XMessages
{
    public class XPacketHandshake
    {
        // Поле 1: Магическое число для рукопожатия
        [XField(1)]
        public int MagicHandshakeNumber;
    }
}
