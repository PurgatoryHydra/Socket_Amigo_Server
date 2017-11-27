using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;

namespace SocketAmigoServer
{
    class RawDataTab : ITab
    {
        const int WIDTH_TEXTBOX = 275;
        const int HEIGHT_TEXTBOX = 100;
        const int MARGIN_TEXTBOX = 10;

        public TabPage tabPage = new TabPage("Raw Data");
        RichTextBox[,] textBoxReceive = new RichTextBox[4, 4];
        GroupBox groupReceive = new GroupBox();
        RadioButton radioReceiveHex = new RadioButton();
        RadioButton radioReceiveASC = new RadioButton();
        CheckBox checkUpdating = new CheckBox();

        string textReceive;

        private delegate void FreshBox(int i, int j);
        public RawDataTab(Size sizeMother)
        {
            addControls(sizeMother);
        }

        private void addControls(Size sizeMother)
        {
            #region Controls
            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 4; j++)
                {
                    textBoxReceive[i, j] = new RichTextBox();
                    textBoxReceive[i, j].Location = new Point(MARGIN_TEXTBOX + i * (WIDTH_TEXTBOX + MARGIN_TEXTBOX), MARGIN_TEXTBOX + j * (HEIGHT_TEXTBOX + MARGIN_TEXTBOX));
                    textBoxReceive[i, j].Size = new Size(WIDTH_TEXTBOX, HEIGHT_TEXTBOX);
                    tabPage.Controls.Add(textBoxReceive[i, j]);
                }
            }

            radioReceiveHex.Location = new Point(5, 20);
            radioReceiveHex.Size = new System.Drawing.Size(41, 15);
            radioReceiveHex.Text = "Hex";
            radioReceiveHex.Checked = true;

            radioReceiveASC.Location = new Point(55, 20);
            radioReceiveASC.Size = new System.Drawing.Size(41, 15);
            radioReceiveASC.Text = "ASC";

            groupReceive.Location = new Point(MARGIN_TEXTBOX, 4 * (MARGIN_TEXTBOX + HEIGHT_TEXTBOX) + MARGIN_TEXTBOX);
            groupReceive.Size = new System.Drawing.Size(100, 50);
            groupReceive.Text = "Receive";
            groupReceive.Controls.Add(radioReceiveHex);
            groupReceive.Controls.Add(radioReceiveASC);
            tabPage.Controls.Add(groupReceive);

            checkUpdating.Location = new Point(MARGIN_TEXTBOX, groupReceive.Location.Y + groupReceive.Size.Height + MARGIN_TEXTBOX);
            checkUpdating.Size = new Size(60, 23);
            checkUpdating.Text = "Update";
            tabPage.Controls.Add(checkUpdating);
            #endregion
        }

        public void OnDataRecieved(Object sender, EventArgs e)
        {
            DataRecievedEventArgs args = (DataRecievedEventArgs)e;
            Coding coding = new Coding(args.buffer);
            StringBuilder builder = new StringBuilder();
            textReceive = "";
            for (int i = 0; i < coding.Length; i++)
                builder.Append(coding.Data[i].ToString("X2") + " ");
            textReceive += builder.ToString();
            freshReceiveBox(coding.ID / 4, coding.ID % 4);
        }

        private void freshReceiveBox(int i, int j)
        {
            if (textBoxReceive[i, j].InvokeRequired)
            {
                FreshBox freshBox = new FreshBox(freshReceiveBox);
                textBoxReceive[i, j].Invoke((EventHandler)delegate
                {
                    freshBox(i, j);
                });
            }
            else
            {
                textBoxReceive[j, i].AppendText(textReceive);
                textBoxReceive[j, i].SelectionStart = textBoxReceive[i, j].TextLength;
                textBoxReceive[j, i].SelectionLength = 0;
                textBoxReceive[j, i].ScrollToCaret();
            }
        }
    }
}
