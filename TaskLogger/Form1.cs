using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;


namespace TaskLoggerApplication
{
    public partial class MainForm : Form
    {
        System.Windows.Forms.Timer m_TmrLog = new System.Windows.Forms.Timer();
        bool m_bClose = false;
        List<string> procList = new List<string>();
        List<PerformanceCounter> counterList = new List<PerformanceCounter>();
        ChartForm m_FrmChart = new ChartForm();

        public MainForm()
        {
            m_TmrLog.Interval = 15000;
            InitializeComponent();
            this.Hide();
            m_TmrLog.Tick +=new EventHandler(m_TmrLog_Tick);
        }

        void m_TmrLog_Tick(object sender, EventArgs e)
        {
            SaveData();
            m_FrmChart.FillChart("");
        }

        private void SaveData()
        {
            if (!Directory.Exists(Application.StartupPath + "\\Tasks\\"))
                Directory.CreateDirectory(Application.StartupPath + "\\Tasks\\");

            Process[] procs = Process.GetProcesses();
            foreach (Process proc in procs)
            {
                //show all processes, showing cpupercent doesn't work yet
                if (!procList.Contains(proc.ProcessName) && (procList.Count > 0))
                    continue;

                int nProcessors = System.Environment.ProcessorCount;

                string strPath = Application.StartupPath + "\\Tasks\\" + proc.ProcessName + ".log";

                if (!File.Exists(strPath))
                    File.Create(strPath).Close();

                try
                {
                    using(StreamWriter myFile = new StreamWriter(strPath, true))
                    {
                            string cpupercent = "0";
                            try
                            {
                                cpupercent = ((int)(counterList[procList.IndexOf(proc.ProcessName)].NextValue() / nProcessors)).ToString();
                            }
                            catch { }
                            string line = DateTime.Now.ToString() + ";" + (proc.WorkingSet64 / 1024).ToString()
                                                                  + ";" + (proc.PrivateMemorySize64 / 1024).ToString()
                                                                  + ";" + cpupercent
                                                                  + ";" + (proc.TotalProcessorTime).ToString();
                            myFile.WriteLine(line);
                    }
                }
                catch { }
            }
        }

        private void endMenuItem_Click(object sender, EventArgs e)
        {
            m_bClose = true;
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!m_bClose)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Show();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
            //procList = textBox1.Lines;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            procList.Clear();
            counterList.Clear();

            contextMenuStrip1.Items.Clear();
            for (int n = 0; n < textBox1.Lines.Length; n++)
            {
                procList.Add(textBox1.Lines[n]);
                counterList.Add(new PerformanceCounter("Processor Information", "% Processor Time", textBox1.Lines[n]));
                ToolStripMenuItem item = new ToolStripMenuItem("Chart-" + textBox1.Lines[n]);
                item.Click += new EventHandler(item_Click);
                contextMenuStrip1.Items.Add(item);
            }
            contextMenuStrip1.Items.Add(showToolStripMenuItem);
            contextMenuStrip1.Items.Add(endMenuItem);

            SaveData();
            if (!m_TmrLog.Enabled)
                m_TmrLog.Start();
        }

        void item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            m_FrmChart.Show();
            m_FrmChart.FillChart(item.Text.Replace("Chart-", String.Empty));
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.Hide();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            m_FrmChart.Show();
            m_FrmChart.FillChart("");
        }
    }
}
