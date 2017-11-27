using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAmigoServer
{
    class DataRecievedEventArgs: EventArgs
    {
        public int ID;
        public int counterRecieved;
        public byte[] buffer = new byte[1024];
        public DataRecievedEventArgs(int ID, int counterRecieved, byte[] data)
        {
            this.ID = ID;
            this.counterRecieved = counterRecieved;
            data.CopyTo(buffer, 0);
        }
    }
}
