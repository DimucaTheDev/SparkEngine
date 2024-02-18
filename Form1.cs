using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RotatingCube;

namespace OpentkGraphics
{
    public partial class Form1 : Form

    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        public static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

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
        public static int GWL_STYLE = -16;
        public static int WS_CHILD = 0x40000000;
        private Rectangle getWindowRectWrapper(IntPtr hWnd)
        {
            Rectangle myRect = new Rectangle();
            RECT rct;

            if (!GetWindowRect(new HandleRef(this, hWnd), out rct))
            {
                MessageBox.Show("Error on getting window rect");
            }
            else
            {
                myRect.X = rct.Left;
                myRect.Y = rct.Top;
                myRect.Width = rct.Right - rct.Left + 1;
                myRect.Height = rct.Bottom - rct.Top + 1;
            }
            return myRect;
        }
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Process children = Process.GetCurrentProcess();
            SetParent(children.MainWindowHandle, Handle);
            Rectangle rect = getWindowRectWrapper(children.MainWindowHandle);
            MoveWindow(children.MainWindowHandle, 0, 0, rect.Width, rect.Height, true);

            return;
            SetParent(Process.GetProcessById(Process.GetCurrentProcess().Id).MainWindowHandle, this.Handle);
            MoveWindow(Program.handle, 0, 0, this.Width, this.Height, true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            Process process = new Process();
            process.StartInfo.FileName = @"C:\Program Files (x86)\Roblox\Versions\version-717cf6a6f7614f44\RobloxPlayerBeta.exe";
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForInputIdle();
            Debug.WriteLine($"SetParent of {process.MainWindowHandle} to {processes[comboBox1.SelectedIndex].MainWindowHandle}");
            SetParent(process.MainWindowHandle, processes[comboBox1.SelectedIndex].MainWindowHandle);
            MoveWindow(process.MainWindowHandle, 0, 0, 512, 512, true);
            
            Process parent = processes[comboBox1.SelectedIndex];
            Process children = processes[comboBox2.SelectedIndex];
            SetParent(children.MainWindowHandle, parent.MainWindowHandle);
            Rectangle rect = getWindowRectWrapper(children.MainWindowHandle);
            MoveWindow(children.MainWindowHandle, 0, 0, rect.Width, rect.Height, true);*/
        }
    }
}