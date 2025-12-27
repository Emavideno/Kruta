using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Protocol
{
    public class EAPacketField
    {
        public byte FieldID { get; set; } //id поля
        public byte FieldSize { get; set; } //размер поля (знаем заранее)
        public byte[] Contents { get; set; } //контент
    }
}
