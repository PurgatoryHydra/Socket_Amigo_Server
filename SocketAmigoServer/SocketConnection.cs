using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SocketAmigoServer
{
    class SocketConnection: Connection, IConnection
    {
        private Socket socket;
        Socket[] connections = new Socket[16];

        public SocketConnection(){}

        public SocketConnection(IPAddress IP, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipEndPoint = new IPEndPoint(IP, port);
            socket.Bind(ipEndPoint);
            socket.Listen(10);
            ThreadStart startListen = new ThreadStart(procListening);
            Thread threadListen = new Thread(startListen);
            threadListen.Start();
        }

        public void Close()
        {

        }

        private void procListening()
        {
            while(true)
            {
                SocketThreadParameter parameter = new SocketThreadParameter(counterConnection);
                connections[counterConnection++] = socket.Accept();
                ParameterizedThreadStart startRecieve = new ParameterizedThreadStart(procRecieving);
                Thread threadRecieve = new Thread(startRecieve);
                threadRecieve.Start(parameter);
            }
        }

        private void procRecieving(Object obj)
        {
            int id = ((SocketThreadParameter)obj).id;
            int counterRecieved;
            while(true)
            {
                try
                {
                    counterRecieved = connections[id].Receive(bufferDataRead);
                    DataRecievedEventArgs args = new DataRecievedEventArgs(id, counterRecieved, bufferDataRead);
                    DataReceived(this, args);
                }
                catch (Exception ex)
                {
                }
            }
        }

        public void SendBuffer(byte[] buffer, int length)
        {
        }

        public void SendString(String str)
        {
        }

        public List<IPAddress> getIPList()
        {
            IPAddress[] IPList = Dns.GetHostAddresses(Dns.GetHostName());
            List<IPAddress> listIP = new List<IPAddress>();
            foreach (IPAddress IP in IPList)
            {
                if (Regex.IsMatch(IP.ToString(), @"\d{1,3}(\.\d{1,3}){1,3}"))
                    listIP.Add(IP);
            }

            return listIP;
        }
    }
}
