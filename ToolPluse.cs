using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Configuration;
using System.Diagnostics;


namespace toolPlus
{

    public partial class ToolPluse : Form
    {

        public static SerialPort ComDevice = new SerialPort();
        public static string sportname = "COM1", btv = "9600", Parity = "None";
        bool isOpen = false;

        public ToolPluse()
        {
            InitializeComponent();
        }
        private bool ShowChildrenForm(string p_ChildrenFormText)
        {
            for (int index = 0; index < panel2.Controls.Count; index++)
            {
                System.Diagnostics.Debug.WriteLine(panel2.Controls[index].GetType().Name);
                if (panel2.Controls[index].GetType().Name == p_ChildrenFormText)
                    return false;
            }
            return true;
        }
        private void 关于此软件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ShowChildrenForm("About") == false)
                return;
            About About_w = new About();

            About_w.MdiParent = this;
            pictureBox1.Visible = false;
            this.panel2.Controls.Add(About_w);
            About_w.Show();
        }

        private void RemoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ShowChildrenForm("iRemote") == false)
                return;
            iRemote iRemote_w = new iRemote();

            iRemote_w.MdiParent = this;
            pictureBox1.Visible = false;
            this.panel2.Controls.Add(iRemote_w);
            iRemote_w.Show();
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo();
            Info.FileName = "calc.exe ";//"calc.exe"为计算器，"notepad.exe"为记事本，"mspaint.exe "为画图
            System.Diagnostics.Process Proc = System.Diagnostics.Process.Start(Info);
        }

        private void Portname_SelectedIndexChanged(object sender, EventArgs e)
        {
            sportname = portname.SelectedItem.ToString();
        }

        private void BaudrateBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            btv = BaudrateBox.SelectedItem.ToString();
        }

        private void Portname_DropDown(object sender, EventArgs e)
        {
            portname.Items.Clear();
            portname.Items.AddRange(SerialPort.GetPortNames());

            if (portname.Items.Count > 0)
            {
                portname.SelectedIndex = 0;
            }
            else
            {
                portname.Text = "未检测到串口";
            }
        }

        private void SmallToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
                if (ShowChildrenForm("SmallTool") == false)
                    return;
                SmallTools SmallTools_w = new SmallTools();

                SmallTools_w.MdiParent = this;
                pictureBox1.Visible = false;
                this.panel2.Controls.Add(SmallTools_w);
                SmallTools_w.Show();
           
        }

        private void ComboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("toolplus1.exe");
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("xiezhjt.exe");
        }

        private void ToolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Parity = toolStripComboBox1.Text;
        }

        private void CMDtestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ShowChildrenForm("cmdtest") == false)
                return;
            cmdtest cmdtest_w = new cmdtest();

            cmdtest_w.MdiParent = this;
            pictureBox1.Visible = false;
            this.panel2.Controls.Add(cmdtest_w);
            cmdtest_w.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

            DirectoryInfo folder = new DirectoryInfo(@".\背景图片");

            foreach (FileInfo file in folder.GetFiles("*.jpg"))
            {
                Console.WriteLine(file.FullName);
            }
            FileInfo[] filexx = folder.GetFiles("*.jpg");

            Random rd = new Random();

            //pictureBox1.Image = Image.FromFile(@filexx[rd.Next(0, folder.GetFiles("*.jpg").Length)].FullName);




            BaudrateBox.Text = "115200";
            btv = BaudrateBox.SelectedItem.ToString();
            portname.Items.Clear();
            portname.Items.AddRange(SerialPort.GetPortNames());
            if (portname.Items.Count > 0)
            {
                portname.SelectedIndex = 0;
                sportname = portname.SelectedItem.ToString();
            }
            else
            {
                portname.Text = "未检测到串口";
            }         
            panel2.BackColor = Color.FromArgb(255, 45, 45, 48);
        }
    }
}
