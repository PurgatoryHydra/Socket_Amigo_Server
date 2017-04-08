using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO.Ports;
using SocketAmigoServer.Secondary_Tabs;
using SocketAmigoServer.Secondary_Tabs.ChartTab;

namespace SocketAmigoServer
{
    class ChannelTab
    {
        public TabPage tabPage;
        public TabControl tabControlChannel;
        public BasicIOTab basicIOTab;
        public AdvancedIOTab advancedIOTab;
        public ChartTab chartTab;
        public Socket Socket;
        public int id;
        public SerialPort port;
        public ChannelTab(int index, String name, Socket clientSocket, Size motherSize)
        {
            id = index;
            Socket = clientSocket;

            tabControlChannel = new TabControl();
            tabControlChannel.Location = new Point(3, 3);
            tabControlChannel.Size = new Size(motherSize.Width - 15, motherSize.Height - 30);
            tabControlChannel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            basicIOTab = new BasicIOTab(Socket, tabControlChannel.Size);
            tabControlChannel.TabPages.Add(basicIOTab.tabPage);

            addControls(name, motherSize);
        }
        public ChannelTab(int index, String name, SerialPort port, Size motherSize)
        {
            id = index;
            this.port = port;

            tabControlChannel = new TabControl();
            tabControlChannel.Location = new Point(3, 3);
            tabControlChannel.Size = new Size(motherSize.Width - 15, motherSize.Height - 30);
            tabControlChannel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            basicIOTab = new BasicIOTab(port, tabControlChannel.Size);
            tabControlChannel.TabPages.Add(basicIOTab.tabPage);

            addControls(name, motherSize);
        }

        private void addControls(String name, Size motherSize)
        {
            #region Controls

            advancedIOTab = new AdvancedIOTab(tabControlChannel.Size);
            tabControlChannel.TabPages.Add(advancedIOTab.tabPage);

            chartTab = new ChartTab(tabControlChannel.Size);
            tabControlChannel.TabPages.Add(chartTab.tabPage);

            tabPage = new TabPage();
            tabPage.Text = name;
            tabPage.Controls.Add(tabControlChannel);
            #endregion
        }
    }
}
