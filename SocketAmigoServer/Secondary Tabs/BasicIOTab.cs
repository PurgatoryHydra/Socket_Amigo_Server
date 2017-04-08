using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.IO.Ports;

namespace SocketAmigoServer
{
    class BasicIOTab
    {
        public TabPage tabPage = new TabPage("Basic IO");
        public Socket socket;
        public Button buttonSend = new Button();
        public RichTextBox textBoxSend = new RichTextBox(), textBoxReceive = new RichTextBox();
        public RadioButton radioReceiveHex = new RadioButton(), radioReceiveASC = new RadioButton(), radioSendHex = new RadioButton(), radioSendASC = new RadioButton();
        public GroupBox groupReceive = new GroupBox(), groupSend = new GroupBox();
        public String textReceive;
        private Button buttonClear = new Button();
        private OpenFileDialog dialogFile = new OpenFileDialog();
        private Button buttonFileUpload = new Button();
        private delegate void FreshBox();
        private SerialPort port;
        private Mode mode;
        public BasicIOTab(Socket clientSocket, Size motherSize)
        {
            socket = clientSocket;
            addControls(motherSize);
            mode = Mode.Socket;
        }

        public BasicIOTab(SerialPort port, Size motherSize)
        {
            this.port = port;
            addControls(motherSize);
            mode = Mode.Serial;
        }

        private void addControls(Size motherSize)
        {
            #region Controls
            textBoxReceive.Location = new Point(5, 5);
            textBoxReceive.Size = new System.Drawing.Size(motherSize.Width - 15, (motherSize.Height - 100) * 3 / 4 - 30);
            textBoxReceive.TextChanged += textBoxReceive_TextChanged;
            textBoxReceive.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            tabPage.Controls.Add(textBoxReceive);

            radioReceiveHex.Location = new Point(5, 20);
            radioReceiveHex.Size = new System.Drawing.Size(41, 15);
            radioReceiveHex.Text = "Hex";
            radioReceiveHex.Checked = true;

            radioReceiveASC.Location = new Point(55, 20);
            radioReceiveASC.Size = new System.Drawing.Size(41, 15);
            radioReceiveASC.Text = "ASC";

            groupReceive.Location = new Point(5, textBoxReceive.Size.Height + textBoxReceive.Location.Y + 5);
            groupReceive.Size = new System.Drawing.Size(100, 50);
            groupReceive.Text = "Receive";
            groupReceive.Controls.Add(radioReceiveHex);
            groupReceive.Controls.Add(radioReceiveASC);
            tabPage.Controls.Add(groupReceive);

            radioSendHex.Location = new Point(5, 20);
            radioSendHex.Size = new System.Drawing.Size(41, 15);
            radioSendHex.Text = "Hex";
            radioSendHex.Checked = true;

            radioSendASC.Location = new Point(55, 20);
            radioSendASC.Size = new System.Drawing.Size(41, 15);
            radioSendASC.Text = "ASC";

            groupSend.Location = new Point(groupReceive.Location.X + groupReceive.Size.Width + 5, groupReceive.Location.Y);
            groupSend.Size = new System.Drawing.Size(100, 50);
            groupSend.Text = "Send";
            groupSend.Controls.Add(radioSendHex);
            groupSend.Controls.Add(radioSendASC);
            tabPage.Controls.Add(groupSend);

            buttonClear.Location = new Point(groupSend.Location.X + groupSend.Size.Width + 5, groupSend.Location.Y + groupSend.Size.Height - 30);
            buttonClear.Size = new Size(75, 23);
            buttonClear.Text = "Clear";
            buttonClear.MouseClick += buttonClear_MouseClick;
            tabPage.Controls.Add(buttonClear);

            buttonFileUpload.Location = new Point(buttonClear.Location.X + buttonClear.Size.Width + 5, buttonClear.Location.Y);
            buttonFileUpload.Size = new Size(75, 23);
            buttonFileUpload.Text = "Send File";
            buttonFileUpload.MouseClick += buttonFileUpload_MouseClick;
            tabPage.Controls.Add(buttonFileUpload);

            textBoxSend.Location = new Point(5, groupSend.Location.Y + groupSend.Size.Height + 5);
            textBoxSend.Size = new System.Drawing.Size(motherSize.Width-15, (motherSize.Height-100)/4);
            tabPage.Controls.Add(textBoxSend);

            buttonSend.Location = new Point(motherSize.Width - (15+75), textBoxSend.Height + textBoxSend.Location.Y + 5);
            buttonSend.Size = new System.Drawing.Size(75, 23);
            buttonSend.Text = "Send";
            buttonSend.MouseClick += buttonSend_Click;
            tabPage.Controls.Add(buttonSend);
            #endregion
        }

        void buttonFileUpload_MouseClick(object sender, MouseEventArgs e)
        {
            dialogFile.ShowDialog();
            byte[] bufferToSend = File.ReadAllBytes(dialogFile.FileName);
            MessageBox.Show(bufferToSend.Length.ToString());
            if (mode == Mode.Socket)
                socket.Send(bufferToSend);
            else
                port.Write(bufferToSend, 0, bufferToSend.Length);
        }

        void buttonClear_MouseClick(object sender, MouseEventArgs e)
        {
            textBoxReceive.Text = "";
        }

        public void update(byte[] bufferReceive, int count)
        {
            StringBuilder builder = new StringBuilder();
            textReceive = "";
            if (radioReceiveHex.Checked)
            {
                for (int i = 0; i < count; i++)
                    builder.Append(bufferReceive[i].ToString("X2") + " ");
                textReceive += builder.ToString();
            }
            else
            {
                textReceive += Encoding.ASCII.GetString(bufferReceive, 0, count);
            }
            freshReceiveBox();
        }

        private void freshReceiveBox()
        {
            if (textBoxReceive.InvokeRequired)
            {
                FreshBox freshBox = new FreshBox(freshReceiveBox);
                textBoxReceive.Invoke((EventHandler)delegate
                {
                    freshBox();
                });
            }
            else
                textBoxReceive.AppendText(textReceive);
        }

        private void textBoxReceive_TextChanged(object sender, EventArgs e)
        {
            textBoxReceive.SelectionStart = textBoxReceive.TextLength;
            textBoxReceive.SelectionLength = 0;
            textBoxReceive.ScrollToCaret();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (radioSendHex.Checked)
            {
                StringBuilder builder = new StringBuilder();
                String textToSend = textBoxSend.Text.Replace(" ", "");
                if (Math.IEEERemainder(textToSend.Length, 2) != 0)
                {
                    MessageBox.Show("Sending characters are not always in pairs.");
                }
                else
                {
                    byte[] bufferToSend = new byte[textToSend.Length / 2];
                    try
                    {
                        for (int i = 0; i < textToSend.Length; i += 2)
                            bufferToSend[i / 2] = (byte)int.Parse(textToSend.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                        if (mode == Mode.Socket)
                            socket.Send(bufferToSend);
                        else
                            port.Write(bufferToSend, 0, bufferToSend.Length);
                    }
                    catch (FormatException formatException)
                    {
                        MessageBox.Show("The format of the sending characters should be in hex.");
                    }
                }
            }
            else
                socket.Send(Encoding.ASCII.GetBytes(textBoxSend.Text));
        }
    }
}
