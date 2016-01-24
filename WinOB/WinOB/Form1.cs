using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinOB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {


            WindowEnumerator we = WindowEnumerator.Instance;



            List<Window> ws = we.getAllVisibleWindows();


            listBox1.Items.Clear();

            foreach (Window w in ws)
            {
                listBox1.Items.Add(w.Title+","+w.getFilename());
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            WindowEnumerator we=WindowEnumerator.Instance;
            Window fw = we.getForegroundWindow();
            this.Text = fw.Title;

            button1.Text = "|" + fw.getFilename() + "|";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Process.EnterDebugMode();
        }
    }
}
