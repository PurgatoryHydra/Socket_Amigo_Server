using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SocketAmigoServer.Secondary_Tabs.ChartTab
{
    class ChartTab
    {
        public TabPage tabPage = new TabPage();
        public TabControl tabCheckBox = new TabControl();
        public Chart chart = new Chart();
        Series[,,] series = new Series[2, 9, 2];
        CheckBoxChartTab[] checkBoxTab = new CheckBoxChartTab[2];
        int countTick = 0;
        ChartArea chartArea;
        private delegate void AddPoint(Series targetSeries, double value);
        public ChartTab(Size motherSize)
        {
            chart.Location = new Point(3, 3);
            chart.Size = new Size(motherSize.Width - 20, motherSize.Height - 120);

            tabCheckBox.Location = new Point(chart.Location.X, chart.Size.Height + chart.Location.Y + 5);
            tabCheckBox.Size = new Size(motherSize.Width - 20, motherSize.Height - tabCheckBox.Location.Y - 30);
            tabPage.Controls.Add(tabCheckBox);

            String[,] nameCheckBox = new String[,] {
            { "User Data 0", "User Data 1",  "User Data 2",  "User Data 3",  "User Data 4",  "User Data 5",  "User Data 6",  "User Data 7",  "User Data 8"  },
            { "User Data 9", "User Data 10", "User Data 11", "User Data 12", "User Data 13", "User Data 14", "User Data 15", "User Data 16", "User Data 17" } };
            checkBoxTab[0] = new CheckBoxChartTab("a", nameCheckBox, tabCheckBox.Size);
            tabCheckBox.TabPages.Add(checkBoxTab[0].tabPage);
            nameCheckBox = new String[,] {
            { "User Data 0", "User Data 1",  "User Data 2",  "User Data 3",  "User Data 4",  "User Data 5",  "User Data 6",  "User Data 7",  "User Data 8"  },
            { "User Data 9", "User Data 10", "User Data 11", "User Data 12", "User Data 13", "User Data 14", "User Data 15", "User Data 16", "User Data 17" } };
            checkBoxTab[1] = new CheckBoxChartTab("User Data", nameCheckBox, tabCheckBox.Size);
            tabCheckBox.TabPages.Add(checkBoxTab[1].tabPage);
            //tabCheckBox.SelectedIndexChanged += tabCheckBox_SelectedIndexChanged;

            tabPage.Text = "Chart";
            tabPage.Controls.Add(chart);
            chartArea = new ChartArea("hello");
            chartArea.BackColor = Color.Black;
            chartArea.AxisX.LineColor = Color.Red;
            chartArea.AxisX.MajorGrid.LineColor = Color.Red;
            chartArea.AxisX.MajorTickMark.LineColor = Color.Red;
            chartArea.AxisX.LabelStyle.ForeColor = Color.Red;
            chartArea.AxisY.LineColor = Color.Red;
            chartArea.AxisY.MajorGrid.LineColor = Color.Red;
            chartArea.AxisY.MajorTickMark.LineColor = Color.Red;
            chartArea.AxisY.LabelStyle.ForeColor = Color.Red;
            chartArea.AxisX.MaximumAutoSize = 60;
            
            chart.ChartAreas.Add(chartArea);
            chart.DragDrop += chart_DragDrop;
            chart.BackColor = Color.Black;
            
            /*Series series0 = new Series();
            
            chart.Series.Add(series0);
            series0.ChartType = SeriesChartType.Line;
            series0.Points.AddY(0);
            series0.Points.AddY(1);
            series0.Points.AddY(2);*/
            
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    /*int newRow = (row * 3 + column) / 9;
                    int newColumn = (row * 3 + column) % 9;*/
                    series[row, column, 1] = new Series();
                    series[row, column, 1].ChartType = SeriesChartType.Line;
                    chart.Series.Add(series[row, column, 1]);
                }
            }
        }

        void chart_DragDrop(object sender, DragEventArgs e)
        {
            MessageBox.Show(e.X.ToString());
            MessageBox.Show(e.Y.ToString());
            MessageBox.Show(e.Effect.ToString());
        }

        public void update(String[,] comboFormat, object[,,] decodeResult, int[] countDecode)
        {
            for (int row = 0; row < 6; row++)
            {
                for (int column = 0; column < 5; column++)
                {
                    if (countDecode[1] > 0)
                    {
                        int newRow = (row * 5 + column) / 9;
                        int newColumn = (row * 5 + column) % 9;
                        if (newRow == 2)
                            break;
                        countTick++;
                        //if (countTick > 60)
                        switch(comboFormat[row, column])
                        {
                            case "UInt16":
                                addPoint(series[newRow, newColumn, 1], (UInt16)decodeResult[row, column, 1]);
                                //series[newRow, newColumn, 1].Points.AddY((UInt16)decodeResult[row, column, 1]);
                                break;
                            case "UInt8":
                                addPoint(series[newRow, newColumn, 1], (byte)decodeResult[row, column, 1]);
                                break;
                            case "UInt32":
                                addPoint(series[newRow, newColumn, 1], (UInt32)decodeResult[row, column, 1]);
                                break;
                            case "Float":
                                addPoint(series[newRow, newColumn, 1], (float)decodeResult[row, column, 1]);
                                break;
                        }
                        countDecode[1]--;
                    }
                }
            }
        }

        private void addPoint(Series targetSeries, double value)
        {
            if (chart.InvokeRequired)
            {
                AddPoint delegateAddPoint = new AddPoint(addPoint);
                chart.Invoke((EventHandler)delegate
                {
                    delegateAddPoint(targetSeries, value);
                });
            }
            else
            {
                targetSeries.Points.AddY(value);
            }
        }

        /*private void tabCheckBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            chart.Series.Clear();
            for(int row = 0; row < 2; row++)
            {
                for(int column = 0; column < 9; column++)
                {
                    if(checkBoxTab[tabCheckBox.SelectedIndex].checkBoxChart[row, column].Checked)
                    {
                        chart.Series.Add(series[row, column, tabCheckBox.SelectedIndex]);
                    }
                }
            }
        }*/
    }
}
