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
using System.IO;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace SocketAmigoServer
{
    public partial class FormMain : Form
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retval, int size, string filePath);

        bool isOpen = false;
        private delegate void FreshControls(int id);
        private delegate void FreshTCP();
        public int countTCP = 0;
        ToolStripLabel toolLabelTCPNumber;
        ToolStripComboBox toolComboIP = new ToolStripComboBox();
        SerialPort port = new SerialPort();
        ChannelTab serialChannel;

        private string strFilePath = Application.StartupPath + "\\config.ini";
        private string strSec = "";

        Object connection;

        RawDataTab rawDataTab;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            List<IPAddress> listIP = (new SocketConnection()).getIPList();
            for (int i = 0; i < listIP.Count; i++)
                toolComboIP.Items.Add(listIP[i]);
            toolComboIP.SelectedIndex = 0;

            toolLabelTCPNumber = new ToolStripLabel();
            toolLabelTCPNumber.Text = "TCP Number:0";
            statusStripMain.Items.AddRange(new ToolStripItem[]{toolComboIP, toolLabelTCPNumber});

            rawDataTab = new RawDataTab(tabControlMain.Size);
            tabControlMain.TabPages.Add(rawDataTab.tabPage);

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

           // port.NewLine = "\r\t";
         //   port.DataReceived += port_DataReceived;
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
                    isOpen = true;
                    buttonOpen.Text = "Close";
                    connection = new SocketConnection((IPAddress)toolComboIP.SelectedItem, int.Parse(textBoxHostPort.Text));
                    ((SocketConnection)connection).DataReceived += rawDataTab.OnDataRecieved;
                }
                else
                {
                    isOpen = false;
                    buttonOpen.Text = "Open";
                    ((SocketConnection)connection).Close();
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

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
