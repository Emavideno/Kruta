using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Server
{
    public class ServerSettings
    {
        public string Host { get; set; } = "127.0.0.1"; //базово если не прочитано из файла конфигурации
        public int Port { get; set; } = 5000; //базово если не прочитано из файла конфигурации
    }
}
