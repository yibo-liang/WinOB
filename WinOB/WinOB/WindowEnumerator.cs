using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WinOB
{
    class WindowEnumerator
    {
        public delegate bool EnumedWindow(IntPtr handleWindow, int p);


        [DllImport("user32")]
        private static extern int GetWindowLongA(IntPtr hWnd, int index);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumedWindow enumFunc, int lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();


        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        private static WindowEnumerator instance = new WindowEnumerator();
        //-----------------------------------------
        private WindowEnumerator() { }

        public static WindowEnumerator Instance
        {
            get
            {
                return instance;
            }
        }

        private Window Contains(List<Window> ws, int pid)
        {
            foreach (Window w in ws)
            {
                if (w.pID == pid)
                {
                    return w;
                }
            }
            return null;
        }

        private List<Window> ObtainFilenames(List<Window> ws)
        {
            List<Window> result = new List<Window>();
            List<Window> temp = new List<Window>();
            foreach (Window w in ws)
            {
                try
                {

                    w.filename = w.process.MainModule.FileName;
                    result.Add(w);

                }
                catch (Exception e)
                {
                    temp.Add(w);
                }
            }
            if (temp.Count > 0)
            {
                String condition = "";
                if (temp.Count == 1)
                {
                    condition = " WHERE ProcessID = " + temp.First().pID;
                }
                string query = "SELECT ExecutablePath, ProcessID FROM Win32_Process"+condition;
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);


                var g = searcher.Get();
                foreach (ManagementObject item in g)
                {
                    object s_pid = item["ProcessID"];
                    object path = item["ExecutablePath"];
                    int pid = int.Parse(s_pid.ToString());
                    Window w = Contains(temp, pid);
                    if (w != null)
                    {
                        if (path != null)
                            w.filename = path.ToString();
                        result.Add(w);
                    }

                }
            }
            return result;


        }



        public List<Window> getAllVisibleWindows()
        {

            List<Window> all = getAllWindows();
            List<Window> result = new List<Window>();


            foreach (Window w in all)
            {
                if (w.state == Window.OPEN)
                {
                    result.Add(w);
                }
            }



            return ObtainFilenames(result);
        }
        public Window getForegroundWindow()
        {

            IntPtr shellWindow = GetShellWindow();
            IntPtr hwnd = GetForegroundWindow();
            Window t = new Window();
            if ((int)hwnd.ToInt32() > 0)
            {
                StringBuilder sb = new StringBuilder(100);
                GetWindowText(hwnd, sb, sb.Capacity);

                t.Handle = hwnd;

                if (hwnd == shellWindow)
                {
                    t.Title = "Desktop";
                }
                else
                {
                    t.Title = sb.ToString();

                }
                t.state = Window.FOREGROUND;

            }
            else
            {
                t.Handle = hwnd;
                t.Title = "";
                t.state = Window.INVISIBLE;

            }
            getProcess(t);
            List<Window> temp = new List<Window>();
            temp.Add(t);
            temp = ObtainFilenames(temp);


            return temp.First();
        }

        private void getProcess(Window w)
        {
            if (w.Handle.ToInt32() == 0)
                return;
            uint pID;
            GetWindowThreadProcessId(w.Handle, out pID);
            Process proc = Process.GetProcessById(unchecked((int)pID));
            w.process = proc;
            w.pID = unchecked((int)pID);

        }

        private List<Window> getAllWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            Dictionary<IntPtr, Window> windows = new Dictionary<IntPtr, Window>();

            var watch = Stopwatch.StartNew();
            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {

                Window t = new Window();

                if (!IsWindowVisible(hWnd)) return true;

                if (hWnd != shellWindow)
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length == 0) return true;
                }
                StringBuilder sb = new StringBuilder(100);
                GetWindowText(hWnd, sb, sb.Capacity);

                t.Handle = hWnd;
                t.Title = sb.ToString();
                t.state = Window.OPEN;

                if (hWnd == shellWindow)
                {
                    t.Title = "Desktop";
                }
                if (!IsWindowVisible(hWnd))
                {
                    t.state = Window.INVISIBLE;
                }
                if (GetForegroundWindow() == hWnd)
                {
                    t.state = Window.FOREGROUND;
                }
                getProcess(t);

                windows[hWnd] = t;

                return true;

            }, 0);
            return windows.Values.ToList();
        }

    }
}
