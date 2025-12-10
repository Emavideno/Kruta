using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.XProtocol
{
    [AttributeUsage(AttributeTargets.Field)]
    public class XFieldAttribute : Attribute
    {
        public byte FieldID { get; }

        public XFieldAttribute(byte fieldId)
        {
            FieldID = fieldId;
        }
    }
}
