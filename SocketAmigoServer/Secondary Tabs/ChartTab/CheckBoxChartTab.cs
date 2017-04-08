using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace SocketAmigoServer.Secondary_Tabs.ChartTab
{
    class CheckBoxChartTab
    {
        public TabPage tabPage = new TabPage();
        public CheckBox[,] checkBoxChart = new CheckBox[2, 9];
        public Color[,] colorTable;
        public CheckBoxChartTab(String name, String[,] nameCheckBox, Size motherSize)
        {
            colorTable = new Color[,]{
                {Color.Red,   Color.Blue, Color.Brown,       Color.Green,   Color.Gray,   Color.Yellow,     Color.Purple, Color.Olive,   Color.Orange},
                {Color.Black, Color.Cyan, Color.GreenYellow, Color.HotPink, Color.Indigo, Color.RosyBrown,  Color.Lime,   Color.Magenta, Color.Maroon}
            };
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    checkBoxChart[row, column] = new CheckBox();
                    checkBoxChart[row, column].Text = nameCheckBox[row, column];
                    checkBoxChart[row, column].Location = new Point(126 * column + 20, 21 * row + 15);
                    checkBoxChart[row, column].Size = new Size(98, 20);
                    tabPage.Controls.Add(checkBoxChart[row, column]);
                }
            }

            tabPage.Text = name;
            tabPage.Paint += tabPage_Paint;
        }

        private void tabPage_Paint(object sender, EventArgs e)
        {
            PaintEventArgs args = (PaintEventArgs)e;
            Graphics graphics = args.Graphics;
            /*Pen pen = new Pen(Color.Blue);
            Brush brush = pen.Brush;*/

            for (int i = 0; i < 9; i++)
            {
                Brush brush = (new Pen(colorTable[0, i])).Brush;
                graphics.FillRectangle(brush, 126 * i, 17, 15, 15);
                brush = (new Pen(colorTable[1, i])).Brush;
                graphics.FillRectangle(brush, 126 * i, 38, 15, 15);
            }
        }
    }
}
