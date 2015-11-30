using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace TaskLoggerApplication
{
    public partial class ChartForm : Form
    {
        string m_file = "";
        public ChartForm()
        {
            InitializeComponent();
        }

        private void Chart_Load(object sender, EventArgs e)
        {
            CreateChart();
        }

        private void CreateChart()
        {
            //chart1.GetToolTipText += new EventHandler<ToolTipEventArgs>(chart1_GetToolTipText);

            // Set custom chart area position
            chart1.ChartAreas["Default"].Position = new ElementPosition(5, 20, 85, 80);
            chart1.ChartAreas["Default"].InnerPlotPosition = new ElementPosition(15, 0, 90, 90);

            // Enable X axis margin
            chart1.ChartAreas["Default"].AxisX.IsMarginVisible = true;

            chart1.ChartAreas["Default"].CursorX.IsUserEnabled = true;
            chart1.ChartAreas["Default"].CursorX.IsUserSelectionEnabled = true;
            chart1.ChartAreas["Default"].AxisX.ScaleView.Zoomable = true;
            chart1.ChartAreas["Default"].AxisX.ScrollBar.IsPositionedInside = true;
        }

        public void FillChart(string file)
        {
            StreamReader myFile = null;
            string FileText = "";
            List<string> lineList = new List<string>();
            List<DateTime> ListTime = new List<DateTime>();
            List<double> ListSensor1 = new List<double>();
            List<double> ListSensor2 = new List<double>();
            List<double> ListSensor3 = new List<double>();
            try
            {
                if(file != String.Empty)
                    m_file = file;

                this.Text = "Chart - " + m_file;
                string strpath = Application.StartupPath + "\\" + "Tasks\\" + m_file + ".log";
                myFile = new StreamReader(strpath);
                FileText = myFile.ReadLine();
                while (FileText != null)
                {
                    lineList.Add(FileText);
                    FileText = myFile.ReadLine();
                }
            }
            catch (Exception ex)
            {
                return;
            }
            finally
            {
                if (myFile != null)
                {
                    myFile.Close();
                    myFile.Dispose();
                }
            }
            if (lineList.Count == 0)
                return;
            try
            {
                for (int n = 0; n < lineList.Count; n++)
                {
                    string[] strings = lineList[n].Split(';');
                    if (strings.Length < 5)
                        throw new ApplicationException("History File corrupted.");
                    ListTime.Add(Convert.ToDateTime(strings[0]));
                    ListSensor1.Add(Convert.ToDouble(strings[1]));
                    ListSensor2.Add(Convert.ToDouble(strings[2]));
                    ListSensor3.Add(Convert.ToDouble(strings[3]));
                }

                //TODO: Handle missing points. Query the lists for timeframes > 1min, and add missing points (double.NaN)

                //set common properties for all series
                foreach (Series tmpser in chart1.Series)
                {
                    tmpser.Points.Clear();
                    tmpser.ChartType = SeriesChartType.Line;
                    tmpser.XValueType = ChartValueType.Time;
                    tmpser.BorderWidth = 2;
                    // Set empty points visual appearance attributes
                    tmpser.EmptyPointStyle.Color = Color.Gray;
                    tmpser.EmptyPointStyle.BorderWidth = 2;
                    tmpser.EmptyPointStyle.BorderDashStyle = ChartDashStyle.Dash;
                    tmpser.EmptyPointStyle.MarkerSize = 7;
                    tmpser.EmptyPointStyle.MarkerStyle = MarkerStyle.Cross;
                    tmpser.EmptyPointStyle.MarkerBorderColor = Color.Black;
                    tmpser.EmptyPointStyle.MarkerColor = Color.LightGray;

                    // Adjust visual appearance attributes depending on the user selection
                    tmpser.EmptyPointStyle.BorderWidth = 0;
                    tmpser.EmptyPointStyle.MarkerStyle = MarkerStyle.None;
                }

                chart1.Series["Working Set"].Color = Color.Red;
                chart1.Series["Private Working Set"].Color = Color.Green;

                for (int pointIndex = 0; pointIndex < ListTime.Count; pointIndex++)
                {
                    chart1.Series["Working Set"].Points.AddXY(ListTime[pointIndex].ToOADate(), ListSensor1[pointIndex]);
                    chart1.Series["Private Working Set"].Points.AddXY(ListTime[pointIndex].ToOADate(), ListSensor2[pointIndex]);
                    chart1.Series["CPU"].Points.AddXY(ListTime[pointIndex].ToOADate(), ListSensor3[pointIndex]);
                }

                // Create extra Y axis for CPU Meter
                CreateYAxis(chart1, chart1.ChartAreas["Default"], chart1.Series["CPU"], 5, 0, "%");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CreateYAxis(Chart chart, ChartArea area, Series series, float axisOffset, float labelsSize, string labelTitle)
        {
            // Create new chart area for original series
            ChartArea areaSeries;
            if (chart.ChartAreas.Count < 2)
                areaSeries = chart.ChartAreas.Add("ChartArea_" + series.Name);
            else
                areaSeries = chart.ChartAreas[2];
            areaSeries.BackColor = Color.Transparent;
            areaSeries.BorderColor = Color.Transparent;
            areaSeries.Position.FromRectangleF(area.Position.ToRectangleF());
            areaSeries.InnerPlotPosition.FromRectangleF(area.InnerPlotPosition.ToRectangleF());
            areaSeries.AxisX.MajorGrid.Enabled = false;
            areaSeries.AxisX.MajorTickMark.Enabled = false;
            areaSeries.AxisX.LabelStyle.Enabled = false;
            areaSeries.AxisY.MajorGrid.Enabled = false;
            areaSeries.AxisY.MajorTickMark.Enabled = false;
            areaSeries.AxisY.LabelStyle.Enabled = false;
            areaSeries.AxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;
            areaSeries.AxisY.Title = labelTitle;
            //areaSeries.AxisY.LabelStyle.Font = new Font((area.AxisY.LabelStyle.Font., area.AxisY.LabelStyle.Font.Size);
            areaSeries.AxisY.TitleAlignment = StringAlignment.Near;

            series.ChartArea = areaSeries.Name;

            // Create new chart area for axis
            ChartArea areaAxis = chart.ChartAreas.FindByName("AxisY_" + series.ChartArea);
            if (areaAxis == null)
                areaAxis = chart.ChartAreas.Add("AxisY_" + series.ChartArea);
            areaAxis.BackColor = Color.Transparent;
            areaAxis.BorderColor = Color.Transparent;
            areaAxis.Position.FromRectangleF(chart.ChartAreas[series.ChartArea].Position.ToRectangleF());
            areaAxis.InnerPlotPosition.FromRectangleF(chart.ChartAreas[series.ChartArea].InnerPlotPosition.ToRectangleF());

            // Create a copy of specified series
            Series seriesCopy = chart.Series.FindByName(series.Name + "_Copy");
            if (seriesCopy == null)
                seriesCopy = chart.Series.Add(series.Name + "_Copy");
            seriesCopy.ChartType = series.ChartType;
            foreach (DataPoint point in series.Points)
            {
                seriesCopy.Points.AddXY(point.XValue, point.YValues[0]);
            }

            // Hide copied series
            seriesCopy.IsVisibleInLegend = false;
            seriesCopy.Color = Color.Transparent;
            seriesCopy.BorderColor = Color.Transparent;
            seriesCopy.ChartArea = areaAxis.Name;

            // Disable grid lines & tickmarks
            areaAxis.AxisX.LineWidth = 0;
            areaAxis.AxisX.MajorGrid.Enabled = false;
            areaAxis.AxisX.MajorTickMark.Enabled = false;
            areaAxis.AxisX.LabelStyle.Enabled = false;
            areaAxis.AxisY.MajorGrid.Enabled = false;
            areaAxis.AxisY.IsStartedFromZero = area.AxisY.IsStartedFromZero;

            // Adjust area position
            areaAxis.Position.X -= axisOffset;
            areaAxis.InnerPlotPosition.X += labelsSize;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FillChart("");
        }

        private void ChartForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
