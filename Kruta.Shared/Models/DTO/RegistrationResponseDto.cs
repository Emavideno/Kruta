using Kruta.Protocol.Serilizations;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kruta.Shared.Models.DTO
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegistrationResponseDto
    {
        [EAField(1)]
        public int PlayerId; // ID, который выдал сервер
    }
}
