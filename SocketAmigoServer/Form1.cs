using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;
using SocketAmigoServer.Secondary_Tabs;
using SocketAmigoServer.Secondary_Tabs.ChartTab;

namespace SocketAmigoServer
{
    public partial class FormMain : Form
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);

        Socket socket;
        IPAddress serverIP;
        Thread threadListening;
        Thread receivingThread;
        bool isOpen = false;
        private delegate void FreshControls(int id);
        private delegate void FreshTCP();
        private delegate void AddTabPage(TabPage tabPage);
        public int countTCP = 0;
        int countTab = 0;
        List<ChannelTab> listChannels = new List<ChannelTab>();
        ToolStripLabel toolLabelTCPNumber;
        ToolStripLabel toolLabelIP;
        SerialPort port = new SerialPort();
        ChannelTab serialChannel;

        private string strFilePath = Application.StartupPath + "\\config.ini";
        private string strSec = ""; 

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            toolLabelIP = new ToolStripLabel();
            toolLabelIP.Text = "Host IP          .";
            toolLabelTCPNumber = new ToolStripLabel();
            toolLabelTCPNumber.Text = "TCP Number:0";
            statusStripMain.Items.AddRange(new ToolStripItem[]{toolLabelIP, toolLabelTCPNumber});

            IPAddress[] IPList = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress IP in IPList)
            {
                if (Regex.IsMatch(IP.ToString(), @"192.168.137.\d{1,3}"))
                {
                    toolLabelIP.Text = "HostIP:" + IP.ToString() + "   ";
                    serverIP = IP;
                }
            }
            ChannelTab channelTab = new ChannelTab(0, "Sample", socket, tabControlMain.Size);
            tabControlMain.TabPages.Add(channelTab.tabPage);

            strSec = Path.GetFileNameWithoutExtension(strFilePath);
            //MessageBox.Show(getINIItem(strSec, "Mode"));
            try
            {
                if (getINIItem(strSec, "Mode").Equals("Socket"))
                    radioButtonSocket.Select();
                else
                    radioButtonSerial.Select();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

            String[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            comboBoxPort.Items.AddRange(ports);
            comboBoxPort.SelectedIndex = comboBoxPort.Items.Count > 0 ? 0 : -1;
            comboBoxBaudRate.SelectedIndex = comboBoxBaudRate.Items.IndexOf("115200");


            port.NewLine = "\r\t";
            port.DataReceived += port_DataReceived;
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int count = port.BytesToRead;
            byte[] bufferReceive = new byte [1024];
            port.Read(bufferReceive, 0, count);

            serialChannel.basicIOTab.update(bufferReceive, count);
            serialChannel.advancedIOTab.update(bufferReceive, count);
            serialChannel.chartTab.update(serialChannel.advancedIOTab.stringFormat, serialChannel.advancedIOTab.decodeResult, serialChannel.advancedIOTab.countDecode);
        }

        private void receivingProc(object obj)
        {
            SocketThreadParameter parameters = (SocketThreadParameter)obj;
            ChannelTab channelTab = listChannels[parameters.id];
            BasicIOTab basicIOTab = channelTab.basicIOTab;
            AdvancedIOTab advancedIOTab = channelTab.advancedIOTab;
            ChartTab chartTab = channelTab.chartTab;
            while (true)
            {
                try
                {
                    byte[] bufferReceive = new byte[1024];
                    int count = channelTab.Socket.Receive(bufferReceive);
                    if (advancedIOTab.isOpen == false)
                        basicIOTab.update(bufferReceive, count);
                    else
                    {
                        advancedIOTab.update(bufferReceive, count);
                        chartTab.update(advancedIOTab.stringFormat, advancedIOTab.decodeResult, advancedIOTab.countDecode);
                    }
                }
                catch (SocketException socketException)
                {
                    countTCP--;
                    freshTCPFunction();
                    channelTab.Socket.Close();
                    break;
                }
            }
        }

        public void listeningThread()
        {
            while (true)
            {
                try
                {
                    Socket clientSocket = socket.Accept();
                    ChannelTab channelTab = new ChannelTab(countTab, "Channel" + countTab.ToString(), clientSocket, tabControlMain.Size);
                    listChannels.Add(channelTab);
                    addTabPageFunction(channelTab.tabPage);
                    ParameterizedThreadStart receivingStart = new ParameterizedThreadStart(receivingProc);
                    receivingThread = new Thread(receivingStart);
                    receivingThread.Start(new SocketThreadParameter(channelTab.id));
                    countTCP++;
                    countTab++;
                    freshTCPFunction();
                    //toolLabelTCPNumber.Text = "TCP:" + countTCP.ToString();
                    
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

        public void addTabPageFunction(TabPage tabPage)
        {
            if (tabControlMain.InvokeRequired)
            {
                AddTabPage addTabPage = new AddTabPage(addTabPageFunction);
                Invoke((EventHandler)delegate{
                    addTabPage(tabPage);
                });
            }
            else
            {
                tabControlMain.TabPages.Add(tabPage);
            }
        }

        public void freshTCPFunction()
        {
            if (this.InvokeRequired)
            {
                FreshTCP freshTCP = new FreshTCP(freshTCPFunction);
                this.Invoke(freshTCP);
            }
            else
            {
                toolLabelTCPNumber.Text = "TCP:" + countTCP.ToString();
            }
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            if (radioButtonSocket.Enabled == true)
            {
                radioButtonSocket.Enabled = false;
                radioButtonSerial.Enabled = false;
            }
            if (radioButtonSocket.Checked)
            {
                if (isOpen == false)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint ipEndPoint = new IPEndPoint(serverIP, int.Parse(textBoxHostPort.Text));
                    socket.Bind(ipEndPoint);
                    socket.Listen(10);
                    ThreadStart threadStart = new ThreadStart(listeningThread);
                    threadListening = new Thread(threadStart);
                    threadListening.Start();
                    isOpen = true;
                    buttonOpen.Text = "Close";
                }
                else
                {
                    /*foreach (Socket connectionSocket in listConnection)
                    {
                        try
                        {
                            connectionSocket.Close();
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    foreach (Thread receivingThread in listReceivingThread)
                    {
                        try
                        {
                            receivingThread.Abort();
                        }
                        catch (Exception ex)
                        {
                        }
                    }*/
                    socket.Close();
                    isOpen = false;
                    buttonOpen.Text = "Open";
                }
            }
            else
            {
                if (isOpen == false)
                {
                    try
                    {
                        port.PortName = comboBoxPort.SelectedItem.ToString();
                        port.BaudRate = int.Parse(comboBoxBaudRate.SelectedItem.ToString());
                        serialChannel = new ChannelTab(0, "Serial", port, tabControlMain.Size);
                        addTabPageFunction(serialChannel.tabPage);
                        port.Open();
                        isOpen = true;
                        buttonOpen.Text = "Close";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                else
                {
                    port.Close();
                    isOpen = false;
                    buttonOpen.Text = "Open";
                }
            }
        }

        private void radioButtonSocket_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSocket.Checked)
                WritePrivateProfileString(strSec, "Mode", "Socket", strFilePath);
            else
                WritePrivateProfileString(strSec, "Mode", "Serial", strFilePath);
        }

        private string getINIItem(string section, string key)
        {
            StringBuilder builder = new StringBuilder(1024); 
            GetPrivateProfileString(section, key, "", builder, 1024, strFilePath);
            return builder.ToString();
        }
    }
}
