using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;



namespace WinOB
{
    class Window
    {
        public string Title="";
        public IntPtr Handle;
        public int state;

        public Process process;
        public String filename=null;
        public int pID { get; set; }

        public const int INVISIBLE= 0;
        public const int OPEN = 1;
        public const int FOREGROUND = 2;
        
        public override string ToString()
        {
            return Title;
        }

        public Process getProcess()
        {
            return this.process;
        }
        
        public String getFilename()
        {
            
            if (this.filename != null)
            {
                return this.filename;
            }
           
            filename = "Unknown";
            return filename;
        }
    }
}
