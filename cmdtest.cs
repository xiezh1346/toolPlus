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
using System.Text.RegularExpressions;
using System.Reflection;

namespace toolPlus
{
    public partial class cmdtest : Form
    {

        private bool closing = false;//是否正在关闭串口，执行Application.DoEvents，并阻止再次invoke 
        private bool Listening = false;//是否没有执行完invoke相关操作 
        Dictionary<string, string> openWith = new Dictionary<string, string>();
        private List<byte> buffer = new List<byte>(4096);

        private byte[] p = new byte[1000];
        byte[] load_buff = new byte[1000];
        int resiveligth = 0;

        byte[,] cmdbuff = new byte[50,50];
        byte[,] cmdbuff_458 = new byte[50, 50];
        string[] cmdbuff_458t = new string[50];

        byte[] send_buff = new byte[1000];
        const int GDlen = 8;
        bool remoteflag = true; // 遥控器开启 关闭标志位
        UInt32 keycode = 0;     // 发送的键值


        public cmdtest()
        {
            InitializeComponent();
        }

        void usart_receive(object sender, SerialDataReceivedEventArgs e)
        {
            int l = 0;

            if (closing) return;//如果正在关闭，忽略操作，直接返回，尽快的完成串口监听线程的一次循环

            l = ToolPluse.ComDevice.BytesToRead;
            byte[] ReDatas = new byte[l];
            ToolPluse.ComDevice.Read(ReDatas, 0, l);

            bool dat_catch = false;

            buffer.AddRange(ReDatas);

            while (buffer.Count >= 4)                           //这里用while是因为里面有break 和continue
            {
                if (buffer[0] == 0x5A)         //判断头
                {
                    int len;                            //下位机发送的字节数
                    len = buffer[3];


                    if (buffer.Count < len + 4) break;              //如果接受数据数小于字节数，继续接受 

                                                                    //if (checksum != buffer[len + 3])                //如果效验失败，这个数据不要，继续接受下一个数据
                                                                    //{ buffer.RemoveRange(0, len + 4); continue; }   //这里的continue是说while循环，不是if
                    buffer.CopyTo(0, p, 0, len + 4);                //复制一条完整数据到具体的数据缓存                                     
                    dat_catch = true;
                    buffer.RemoveRange(0, len + 4);                 //正确分析一条数据，从缓存中移除数据。
                    try
                    {
                        Listening = true;//设置标记，说明我已经开始处理数据，一会儿要使用系统UI的。   
                        AddData(p);
                    }
                    finally
                    {
                        Listening = false;//我用完了，ui可以关闭串口了。   
                    }
                    if (dat_catch)
                    {
                        // deal_byte();                                    //用变量保存想要的数据
                    }
                }
                else
                { buffer.RemoveAt(0); }                             //如果包的第一个数据错误，则重新开始
            }
        }

        public bool DisplaySendData(byte[] data, int len)
        {
            // a5  91 20 0f 00 06 ff 00 00 82 ff           00 00 00 00 00 00 00 00 00 80 00 00 00 00 00 00 00 00 00 00 00 00 00 00 6b
            // 0  1  2  3  4  5  6  7  8  9  10           11 12 13 14 15 16 17 18 19 20 21 22

            Int32 m_mode, led, cmd, m_timer, m_head_streng, m_foot_streng, head_wb, foot_wb, ubb, d_status;

            led = data[6];  //按摩时间指示灯
            m_mode = data[8]; //按摩模式
            cmd = data[9];

            m_timer = (data[11] << 8) | data[12];

            m_head_streng = data[14];
            m_foot_streng = data[15];

            head_wb = (data[16] << 8) | data[17];
            foot_wb = (data[18] << 8) | data[19];

            ubb = data[20];
            d_status = data[22];



            string temp = "led状态 ：" + led.ToString() + "\r\n" + "按摩模式 ：" + m_mode.ToString() + "\r\n" + "按摩时间 ：" + m_timer.ToString() + "\r\n"
                           + "头部按摩强度 ：" + m_foot_streng + "\r\n" + "脚部按摩强度 ：" + m_head_streng + "\r\n" + "头部文波 ：" + head_wb + "\r\n" + "脚部文波 ："
                           + foot_wb + "\r\n" + "床底灯 ：" + ubb.ToString("x") + "\r\n" + "电机状态 ：" + Convert.ToString(d_status, 2) + "\r\n";



            return true;
        }
        private void AddData(byte[] data)
        {
            UInt16 temp_value = 0;
            

            Buffer.BlockCopy(data, 0, load_buff, resiveligth, data.Length);

            resiveligth = data.Length;
            if ((load_buff[0] == 0x5A))
            {
                this.Invoke((EventHandler)(delegate
                {

                    StringBuilder sb = new StringBuilder();             
                    for (int i = 0; i < load_buff[3] + 4; i++)
                    {
                        sb.AppendFormat("{0:x2}" + " ", load_buff[i]);

                        int asciicode = (int)(load_buff[i]);
                  
                    }
                    string s = System.Text.Encoding.ASCII.GetString(load_buff,4, load_buff[3]);






                    this.richTextBox2.AppendText(s + "\r\n");
                    this.richTextBox2.AppendText(sb.ToString() + "\r\n\r\n");
                   this.richTextBox2.ScrollToCaret();
                    

                    //DisplaySendData(load_buff, load_buff[2] + 4);
                }));
                Array.Clear(load_buff, 0, load_buff.Length);
                resiveligth = 0;
            }

        }

        private void Button33_Click(object sender, EventArgs e)
        {
            if (ToolPluse.ComDevice.IsOpen == true)
            {          
                closing = true;
                while (Listening) Application.DoEvents();

                //打开时点击，则关闭串口   
                ToolPluse.ComDevice.Close();
                closing = false;
                button33.Text = ToolPluse.ComDevice.IsOpen ? "串口关闭" : "串口打开";

                button33.ForeColor = Color.Green;
                ToolPluse.ComDevice.DataReceived -= usart_receive;
            }
            else
            {

                ToolPluse.ComDevice.DataReceived += usart_receive;

                ToolPluse.ComDevice.BaudRate = Convert.ToInt32(ToolPluse.btv.ToString());

                if (ToolPluse.Parity == "None")
                {
                    ToolPluse.ComDevice.Parity = Parity.None;
                }
                else if (ToolPluse.Parity == "Mark")
                {
                    ToolPluse.ComDevice.Parity = Parity.Mark;
                }
                else if (ToolPluse.Parity == "Even")
                {
                    ToolPluse.ComDevice.Parity = Parity.Even;
                }
                else if (ToolPluse.Parity == "Odd")
                {
                    ToolPluse.ComDevice.Parity = Parity.Odd;
                }
                else if (ToolPluse.Parity == "Space")
                {
                    ToolPluse.ComDevice.Parity = Parity.Space;
                }





                ToolPluse.ComDevice.PortName = ToolPluse.sportname;
                ToolPluse.ComDevice.DataBits = 8;
                button33.ForeColor = Color.Red;
                try
                {
                    ToolPluse.ComDevice.Open();

                    button33.Text = ToolPluse.ComDevice.IsOpen ? "串口关闭" : "串口打开";
                 
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }


            }
        }

        private void Cmdtest_Load(object sender, EventArgs e)
        {
            int i = 0;
            string[] strs1 = File.ReadAllLines(@"系统文件\命令.txt");
            string[] strs2 = File.ReadAllLines(@"系统文件\458-4.txt");
            for (i = 0; i < (strs1.Length) / 2; i++)
            {
                comboBox1.Items.Add(strs1[i]);
                string [] str= strs1[(strs1.Length) / 2 + i].Split(' ');

                for (int l = 0; l < str.Length; l++)
                {
                    cmdbuff[i,l] = Convert.ToByte(str[l], 16);
                }                  
            }

            for (i = 0; i < (strs2.Length) / 2; i++)
            {
                comboBox2.Items.Add(strs2[i]);              
        
                cmdbuff_458t[i] = strs2[(strs2.Length) / 2 + i];
             
            }
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        /*
         * 参数
         * sendbuff ： 发送的数组
         * len：要发送的数据长度
         * dis：是否显示发送数据  
         */

        void cmdsend(byte[] sendbuff, int len, bool dis)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                
                    if (checkBox1.Checked == true)
                    {
                        for (int i = 0; i < len; i++)
                        {
                            sb.AppendFormat("{0:x2}" + " ", sendbuff[i]);
                        }
                        richTextBox1.SelectionColor = Color.Yellow;
                        richTextBox1.AppendText(sb.ToString() + Environment.NewLine);
                        this.richTextBox1.ScrollToCaret();
                    }
                     ToolPluse.ComDevice.Write(sendbuff, 0, len);              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }      

        private void Button3_Click_1(object sender, EventArgs e)
        {
            byte[] buff = new byte[7];

            for (int i = 0; i < 6; i++)
            {
                buff[i] = cmdbuff[comboBox1.SelectedIndex, i];
            }
            cmdsend(buff, 6, true);
        }

        private void Button4_Click(object sender, EventArgs e)
        {

            Int32 i = 0;
            if (ToolPluse.ComDevice.IsOpen != true)
            {
                MessageBox.Show("请先打开串口");
                return;
            }
            try
            {
                 i = Convert.ToInt32(textBox4.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (button4.Text == "START")
            {
                button4.Text = "STOP";
                button4.ForeColor = Color.Red;
                timer1.Interval = i; 
                timer1.Start();
            }
            else
            {
                button4.Text = "START";
                button4.ForeColor = Color.Green;
                timer1.Stop();
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            byte[] buff = new byte[7];

            for (int i = 0; i < 6; i++)
            {
                buff[i] = cmdbuff[comboBox1.SelectedIndex, i];
            }
            cmdsend(buff, 6, true);
        }

        private void Button34_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            richTextBox2.Text = "";
        }

        private void ComboBox1_DropDown(object sender, EventArgs e)
        {
            button4.Text = "START";
            button4.ForeColor = Color.Green;
            timer1.Stop();
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            label1.Text = "解释：" + cmdbuff_458t[comboBox2.SelectedIndex];
        }


        byte suncheck(byte[]  buff, int len)
        {
            UInt32 i;
            byte sum = 0xff;

            for (i = 0; i < buff[0]; i++)
            {
                sum = (byte)(sum - buff[i]);
            }

            return sum;
        }


        private void Button2_Click(object sender, EventArgs e)
        {
            byte[] tbuff = new byte[50];
            UInt32 keycode = 0;
            if (comboBox2.SelectedIndex == 0)
            {
                keycode = 0;
            }
            else 
            {
                keycode = 1;
                keycode <<= (comboBox2.SelectedIndex-1);
            }
                    
          

            tbuff[0] = 5;
            tbuff[1] = 1;
            tbuff[2] = (byte)(keycode&0xff);
            tbuff[3] = (byte)(keycode>>8 & 0xff); 
            tbuff[4] = (byte)(keycode>>16 & 0xff); 
            tbuff[5] = (byte)(keycode>>24 & 0xff);
            tbuff[6] = 0;
            tbuff[7] = suncheck(tbuff, 7);


            cmdsend(tbuff, 8, true);
        }
    }
}
