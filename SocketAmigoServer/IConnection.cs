using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAmigoServer
{
    interface IConnection
    {
        void SendBuffer(byte[] buffer, int length);
        void SendString(string str);
        void Close();
    }
}
