using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace SocketAmigoServer
{
    class SocketThreadParameter
    {
        public int id;
        public SocketThreadParameter(int processID)
        {
            id = processID;
        }
    }
}
