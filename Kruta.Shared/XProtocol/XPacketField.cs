using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;

namespace Kruta.Shared.XProtocol
{
    public class XPacketField
    {
        public byte FieldID { get; set; }
        public byte FieldSize { get; set; }
        public byte[] Contents { get; set; }
    }
}