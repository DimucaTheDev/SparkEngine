using OpenTK.Windowing.Common;
using RotatingCube;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting;

namespace OpentkGraphics
{
    public partial class Form2 : Form
    {
        #region Extern
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetFocus(HandleRef hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        #endregion

        public static Form2 Instance;
        [Conditional("DEBUG")]
        public void Show() => base.Show();

        public Form2()
        {

            InitializeComponent();
            Instance = this;
            //Shown+=(s,e)=> SetFocus(new(this,Process.GetCurrentProcess().MainWindowHandle));this.TxtBox_ApercuFichier.Multiline = true;
            textBox1.WordWrap = false;
            textBox1.ScrollBars = ScrollBars.Horizontal | ScrollBars.Vertical;
            Closing += (s, e) =>
            {
                e.Cancel = true;
                Hide();
            };
        }

        private List<double> fp5s = new() { 0 };
        private List<double> fps2 = new() { 0 };
        private List<double> frt = new() { 0 };
        public void ft(double value)
        {
            if (Application.OpenForms.OfType<Form2>().Any())
                Invoke(() =>
               {
                   fp5s.Add(1 / Math.Min(value, 165));
                   fps2.Add(1 / Math.Min(value, 165));
                   frt.Add(value * 1000);
                   if (fps2.Count > 90) fps2.RemoveAt(0);
                   if (fp5s.Count > 90) fps2.RemoveAt(0);
                   if (frt.Count > 90) frt.RemoveAt(0);
                   label2.Text = $"FPS:  {1 / value}";

                   series3.Points.DataBindY(frt);
                   series1.Points.DataBindY(fps2);
               });
        }

        private string log;
        private bool closed = false;
        public void Log(string msg)
        {
            textBox1.Text = log += (string.IsNullOrWhiteSpace(msg.Replace("   ", "")) ? "" : msg.Replace("\n", Environment.NewLine) + Environment.NewLine);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (GetWindowRect(new(this, Process.GetCurrentProcess().MainWindowHandle), out RECT rect) && Program.Instance.IsFocused)
            {
                Left = rect.Left + (rect.Right - rect.Left);
                Top = rect.Top;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ScrollToCaret();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            label3.Text = $"FP5S: {fp5s.Average()}";
            //series2.Points.DataBindY(fp5s);
            fp5s = new() { 0 };
        }
        Series series1 = new Series("Group A");
        Series series3 = new Series("Group C");
        private void Form2_Load(object sender, EventArgs e)
        {
            label8.Text = $"Texture Size: {Program.TextureSize / 1024} KB";
            textBox1.Text = log;
            label7.Text = "Bad Shaders:" + string.Join("", Program.BadShaders.Where(v => v.Value > 0).Select(s => $"\n\r\t{s.Key} - {s.Value} Errors"));

            Chart chart1 = new();
            // create a series for each line
            checkBox1.Checked = ((int)Program.Instance.VSync) == 0 ? false : true;
            series1.Points.DataBindY(fp5s);
            series1.ChartType = SeriesChartType.FastLine;
            series1.Legend = "FPS (1/frametime)";

            series3.Points.DataBindY(frt);
            series3.ChartType = SeriesChartType.FastLine;
            series3.Legend = "Frame Time";
            series1.Color = Color.Black;
            //chart1.Legends.Add(series1.Legend);
            //chart1.Legends.Add(series3.Legend);
            chart1.ChartAreas.Add("test");
            chart1.Series.Clear();
            chart1.Series.Add(series1);
            chart1.Series.Add(series3);
            chart1.ResetAutoValues();
            chart1.Titles.Clear();
            chart1.ChartAreas[0].AxisY.Title = "Value";
            chart1.ChartAreas[0].AxisX.Title = "Draw calls";
            chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            groupBox1.Controls.Add(chart1);
            chart1.Left = 5;
            chart1.Top = 20;
            chart1.Size = chart1.Size with { Width = groupBox1.Size.Width - textBox1.Size.Width - 15 };
        }
        private Random rand = new(0);
        private void timer3_Tick(object sender, EventArgs e)
        {
            //fps2.Clear();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            Program.Instance.UpdateFrequency = trackBar1.Value;
            label5.Text = trackBar1.Value.ToString();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Program.Instance.VSync = checkBox1.Checked ? VSyncMode.On : VSyncMode.Off;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Program.Instance.WindowState = checkBox2.Checked
                ? OpenTK.Windowing.Common.WindowState.Fullscreen
                : OpenTK.Windowing.Common.WindowState.Normal;
        }
    }
}
