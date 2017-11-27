using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketAmigoServer
{
    interface ITab
    {
        void OnDataRecieved(Object sender, EventArgs e);
    }
}
