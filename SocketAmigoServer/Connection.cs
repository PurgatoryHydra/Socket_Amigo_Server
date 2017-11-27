using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketAmigoServer
{
    class Connection
    {
        protected byte[] bufferDataRead = new byte[1024];
        public EventHandler DataReceived;
        protected int counterConnection = 0;
        public Connection()
        {
        }
    }
}
