using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace SocketAmigoServer.Secondary_Tabs
{
    class AdvancedIOTab
    {
        public TabPage tabPage = new TabPage();
        public GroupBox groupCombos = new GroupBox();
        public GroupBox groupDisplay = new GroupBox();
        public ComboBox[,] comboBoxFormat = new ComboBox[6, 5];
        public String[,] stringFormat = new string[6, 5];
        public Label[,] labelDisplay = new Label[6, 5];
        public Button buttonSaveFormat = new Button();
        public RichTextBox textBoxReceive = new RichTextBox();
        public Button buttonSwitch = new Button();
        private Button buttonClear = new Button();
        public bool isOpen = false;
        public String textReceive;
        public object[, ,] decodeResult = new object[6, 5, 2];
        public int[] countDecode = new int[2];
        private delegate void FreshBox();
        private delegate String GetComboValue(int row, int column);
        private delegate void SetLableValue(int row, int column, string text);
        public AdvancedIOTab(Size motherSize)
        {
            #region Controls
            int column = 0, row = 0;
            String[] formats = { "UInt16", "Int16", "UInt8","UInt32", "Float" };
            for (; row < 6; row++)
            {
                column = 0;
                for (; column < 5; column++)
                {
                    comboBoxFormat[row, column] = new ComboBox();
                    comboBoxFormat[row, column].Location = new Point(column * 61 + 5, row * 21 + 15);
                    comboBoxFormat[row, column].Size = new Size(60, 20);
                    comboBoxFormat[row, column].Items.AddRange(formats);
                    comboBoxFormat[row, column].SelectedIndex = 0;
                    groupCombos.Controls.Add(comboBoxFormat[row, column]);

                    labelDisplay[row, column] = new Label();
                    labelDisplay[row, column].Location = comboBoxFormat[row, column].Location;
                    labelDisplay[row, column].Size = comboBoxFormat[row, column].Size;
                    labelDisplay[row, column].Text = "0";
                    groupDisplay.Controls.Add(labelDisplay[row, column]);
                }
            }

            buttonSaveFormat.Text = "Save";
            buttonSaveFormat.Location = new Point(310, 15);
            buttonSaveFormat.Size = new Size(40, 120);
            groupCombos.Controls.Add(buttonSaveFormat);

            groupCombos.Location = new Point(3, 3);
            groupCombos.Size = new Size(360, 140);
            groupCombos.Text = "Data Format";
            tabPage.Controls.Add(groupCombos);

            textBoxReceive.Location = new Point(groupCombos.Size.Width + groupCombos.Location.X + 5, groupCombos.Location.Y);
            textBoxReceive.Size = new Size(motherSize.Width - groupCombos.Width - groupCombos.Location.X - 20, groupCombos.Size.Height);
            textBoxReceive.TextChanged += textBoxReceive_TextChanged;
            tabPage.Controls.Add(textBoxReceive);

            buttonClear.Location = new Point(textBoxReceive.Location.X, textBoxReceive.Location.Y + textBoxReceive.Size.Height + 5);
            buttonClear.Size = new Size(75, 23);
            buttonClear.Text = "Clear";
            buttonClear.MouseClick += buttonClear_MouseClick;
            tabPage.Controls.Add(buttonClear);

            groupDisplay.Location = new Point(3, groupCombos.Location.Y + groupCombos.Size.Height + 5);
            groupDisplay.Size = new Size(groupCombos.Size.Width, groupCombos.Size.Height + 6);
            groupDisplay.Paint += groupDisplay_Paint;
            groupDisplay.Text = "Display";
            tabPage.Controls.Add(groupDisplay);

            buttonSwitch.Text = "Open Advanced IO";
            buttonSwitch.Location = new Point(3, motherSize.Height - 50);
            buttonSwitch.Size = new Size(150, 23);
            buttonSwitch.MouseClick += buttonSwitch_Click;
            tabPage.Controls.Add(buttonSwitch);
            #endregion

            tabPage.Text = "Advanced IO";
        }

        void buttonClear_MouseClick(object sender, MouseEventArgs e)
        {
            textBoxReceive.Text = "";
        }

        public void update(byte[] bufferReceive, int count)
        {
            if (isOpen == true)
            {
                #region display
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < count; i++)
                    builder.Append(bufferReceive[i].ToString("X2") + " ");
                textReceive = "\r\n" + (builder.ToString());
                freshReceiveBox();
                #endregion
                if (bufferReceive[0] == 0xAA && bufferReceive[1] == 0xAA)
                {
                    List<String> listType = new List<String>();
                    int length = bufferReceive[3];
                    int countLength = 4;
                    countDecode[0] = countDecode[1] = 0;
                    for(int countRow = 0; countRow < 6;countRow++)
                    {
                        for (int countColumn = 0; countColumn < 5; countColumn++)
                        {
                            if (countLength >= length + 4)
                                break;
                            countDecode[1]++;
                            stringFormat[countRow, countColumn] = (String)getComboValue(countRow, countColumn);
                            switch (stringFormat[countRow, countColumn])
                            {
                                #region Byte Decode
                                case "UInt16":
                                    UInt16 _uint16;
                                    unsafe
                                    {
                                        byte* pDes = (byte*)(&_uint16);
                                        *pDes++ = bufferReceive[countLength+1];
                                        *pDes++ = bufferReceive[countLength+0];
                                    }
                                    countLength += 2;
                                    setLabelValue(countRow, countColumn, _uint16.ToString());
                                    decodeResult[countRow, countColumn, 1] = _uint16;
                                    break;
                                case "UInt8":
                                    byte _uint8;
                                    unsafe
                                    {
                                        byte* pDes = (byte*)(&_uint8);
                                        *pDes++ = bufferReceive[countLength+0];
                                        countLength += 1;
                                    }
                                    setLabelValue(countRow, countColumn, _uint8.ToString());
                                    decodeResult[countRow, countColumn, 1] = _uint8;
                                    break;
                                case "UInt32":
                                    UInt32 _uint32;
                                    unsafe
                                    {
                                        byte* pDes = (byte*)(&_uint32);
                                        *pDes++ = bufferReceive[countLength + 3];
                                        *pDes++ = bufferReceive[countLength + 2];
                                        *pDes++ = bufferReceive[countLength + 1];
                                        *pDes++ = bufferReceive[countLength + 0];
                                    }
                                    countLength += 4;
                                    setLabelValue(countRow, countColumn, _uint32.ToString());
                                    decodeResult[countRow, countColumn, 1] = _uint32;
                                    break;
                                case "Float":
                                    float _float;
                                    unsafe
                                    {
                                        byte* pDes = (byte*)(&_float);
                                        *pDes++ = bufferReceive[countLength + 3];
                                        *pDes++ = bufferReceive[countLength + 2];
                                        *pDes++ = bufferReceive[countLength + 1];
                                        *pDes++ = bufferReceive[countLength + 0];
                                    }
                                    countLength += 4;
                                    setLabelValue(countRow, countColumn, _float.ToString());
                                    decodeResult[countRow, countColumn, 1] = _float;
                                    break;
                                #endregion
                            }
                        }
                    }
                }
            }
        }


        private void buttonSwitch_Click(object sender, EventArgs e)
        {
            if (isOpen == false)
            {
                buttonSwitch.Text = "Close Advanced IO";
                isOpen = true;
            }
            else
            {
                buttonSwitch.Text = "Open Advanced IO";
                isOpen = false;
            }
        }

        private void groupDisplay_Paint(object sender, EventArgs e)
        {
            Graphics graphics = ((PaintEventArgs)e).Graphics;
            Pen pen = new Pen(Color.Black);
            for (int i = 1; i <= 4; i++)
                graphics.DrawLine(pen, new Point(61 * i + 4, 15), new Point(61 * i + 4, 140));
            for (int i = 1; i <= 5; i++)
                graphics.DrawLine(pen, new Point(0, 21 * i + 14), new Point(310, 21 * i + 14));
        }

        public void freshReceiveBox()
        {
            if (isOpen == true)
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
        }

        private string getComboValue(int row, int column)
        {
            string value = "";
            if (comboBoxFormat[row, column].InvokeRequired)
            {
                GetComboValue getValue = new GetComboValue(getComboValue);
                comboBoxFormat[row, column].Invoke((EventHandler)delegate
                {
                    value = getValue(row, column);
                });
            }
            else
                value = comboBoxFormat[row, column].SelectedItem.ToString();
            return value;
        }

        private void setLabelValue(int row, int column, string text)
        {
            if (labelDisplay[row, column].InvokeRequired)
            {
                SetLableValue setValue = new SetLableValue(setLabelValue);
                labelDisplay[row, column].Invoke((EventHandler)delegate
                {
                    setValue(row, column, text);
                });
            }
            else
                labelDisplay[row, column].Text = text;
        }

        private void textBoxReceive_TextChanged(object sender, EventArgs e)
        {
            textBoxReceive.SelectionStart = textBoxReceive.TextLength;
            textBoxReceive.SelectionLength = 0;
            textBoxReceive.ScrollToCaret();
        }
    }
}
