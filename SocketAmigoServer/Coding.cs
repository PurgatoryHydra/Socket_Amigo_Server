using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAmigoServer
{
    class Coding
    {
        private int id;
        private int type;
        private int length;
        private byte[] data = new byte[1024];

        public Coding(byte[] rawData)
        {
            if (rawData[0] == 0xAA & rawData[1] == 0xAA)
            {
                type = rawData[2];
                length = rawData[3];
                id = rawData[4];
                for (int i = 0; i < length; i++)
                    data[i] = rawData[5 + i];
            }
            else
            {
                length = 0;
            }
        }

        public int ID
        {
            get { return id; }
        }

        public int Type
        {
            get { return type; }
        }

        public int Length
        {
            get { return length; }
        }

        public byte[] Data
        {
            get { return data; }
        }
    }
}
