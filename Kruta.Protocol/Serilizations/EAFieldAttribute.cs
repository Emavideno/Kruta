using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Protocol.Serilizations
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EAFieldAttribute : Attribute
    {
        public byte FieldID { get; }

        public EAFieldAttribute(byte fieldId)
        {
            FieldID = fieldId;
        }
    }
}
