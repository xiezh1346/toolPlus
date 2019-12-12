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

namespace toolPlus
{
    public partial class iRemote : Form
    {

        struct MyStruct
        {
            public UInt32 key;
            public string strkey;
            public UInt32 timer;
            public string timertype;
        }


        private bool closing = false;//是否正在关闭串口，执行Application.DoEvents，并阻止再次invoke 
        private bool Listening = false;//是否没有执行完invoke相关操作 
        Dictionary<string, UInt32> openWith = new Dictionary<string, UInt32>();
        private List<byte> buffer = new List<byte>(4096);

        private byte[] p = new byte[1000];
        byte[] load_buff = new byte[1000];
        int resiveligth = 0;

        byte[] my_buff = new byte[1000];
        byte[] send_buff = new byte[1000];
        const int GDlen = 8;
        bool remoteflag = true; // 遥控器开启 关闭标志位
        UInt32 keycode = 0;     // 发送的键值
        UInt32 iruntimer = 0;
        UInt32 runcount;
        int irunsetp;
        
        MyStruct[] cmdinfo = new MyStruct[20];



        AutoSizeFormClass asc = new AutoSizeFormClass();

        void ComSend(UInt32 key)
        {
            UInt32 i = 0;
           byte[] tempbuff = new byte[100];

            tempbuff[0] = 0xa5;
            tempbuff[1] = 0x01;
            tempbuff[2] = 0x05;                  
            tempbuff[3] = (byte)(key>>24 & 0xff);
            tempbuff[4] = (byte)(key >> 16 & 0xff);
            tempbuff[5] = (byte)(key >> 8 & 0xff);
            tempbuff[6] = (byte)(key & 0xff);
            if (radioButton1.Checked == true)
            {
                tempbuff[7] = 0;
            }
            else if (radioButton2.Checked == true)
            {
                tempbuff[7] = 1;
            }
            else
            {
                tempbuff[7] = 2;
            }

            for (i = 0; i < 8; i++)
            {
                tempbuff[8] += tempbuff[i];
            }

            ToolPluse.ComDevice.Write(tempbuff, 0, 9);
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

            while (buffer.Count >= 3)                           //这里用while是因为里面有break 和continue
            {
                if (buffer[0] == 0xa5)         //判断头
                {
                    int len;                            //下位机发送的字节数
                    len = buffer[2];


                    if (buffer.Count < len + 4) break;              //如果接受数据数小于字节数，继续接受 
                    byte checksum = 0;                              //异或效验变量
                    for (int i = 0; i < len + 3; i++)               //len=5           
                    { checksum += buffer[i]; }                      //得到效验值
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
                           + "头部按摩强度 ：" + m_foot_streng  + "\r\n" + "脚部按摩强度 ：" + m_head_streng + "\r\n" + "头部文波 ："+ head_wb + "\r\n"+ "脚部文波 ："
                           + foot_wb + "\r\n"+ "床底灯 ：" + ubb.ToString("x") + "\r\n" + "电机状态 ："+ Convert.ToString(d_status, 2) + "\r\n";

            richTextBox3.Text = temp;


            return true;
        }
        private void AddData(byte[] data)
        {
            UInt16 temp_value = 0;


            Buffer.BlockCopy(data, 0, load_buff, resiveligth, data.Length);

            resiveligth = data.Length;
            if ((load_buff[0] == 0xa5))
            {
                this.Invoke((EventHandler)(delegate
                {

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < load_buff[2] + 4; i++)
                    {
                        sb.AppendFormat("{0:x2}" + " ", load_buff[i]);
                    }
                    if (checkBox1.Checked == true)
                    {
                        this.richTextBox1.AppendText(sb.ToString() + "\r\n\r\n");
                        this.richTextBox1.ScrollToCaret();
                    }
                  
                    DisplaySendData(load_buff, load_buff[2] + 4);
                }));
                Array.Clear(load_buff, 0, load_buff.Length);
                resiveligth = 0;
            }

        }

        public iRemote()
        {
            InitializeComponent();
            openWith["HEAD_OUT"] = 0x00000001;
            openWith["HEAD_IN"] = 0x00000002;
            openWith["FOOT_OUT"] = 0x00000004;
            openWith["FOOT_IN"] = 0x00000008;
            openWith["HEAD_T_OUT"] = 0x00000010;
            openWith["HEAD_T_IN"] = 0x00000020;
            openWith["LUMBAR_OUT"] = 0x00000040;
            openWith["LUMBAR_IN"] = 0x00000080;
            openWith["MASS_ALL"] = 0x00000100;
            openWith["MASS_TIMER"] = 0x00000200;
            openWith["MASS_FEET"] = 0x00000400;
            openWith["MASS_HEAD"] = 0x00000800;
            openWith["ZERO_G"] = 0x00001000;
            openWith["MEMORY2"] = 0x00002000;
            openWith["MEMORY3"] = 0x00004000;
            openWith["MEMORY4"] = 0x00008000;
            openWith["MEMORY5"] = 0x00010000;
            openWith["UBB"] = 0x00020000;
            openWith["StrechMove"] = 0x00040000;
            openWith["INTENSITY1"] = 0x00080000;
            openWith["INTENSITY2"] = 0x00100000;
            openWith["INTENSITY3"] = 0x00200000;
            openWith["MA_Waist_M"] = 0x00400000;
            openWith["MA_Head_MIN"] = 0x00800000;
            openWith["MA_FEET_M"] = 0x01000000;
            openWith["MA_STOP_ALL"] = 0x02000000;
            openWith["MASS_MODE"] = 0x04000000;
            openWith["ALL_FLAT"] = 0x08000000;
            openWith["MA_Waist_MIN"] = 0x10000000;
            openWith["AngleAdjust"] = 0x20000000;
            openWith["ExtMemory1"] = 0x40000000;
            openWith["ExtMemory2"] = 0x80000000;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {

            if (remoteflag == true)
            {
                ComSend(keycode);
                label1.Text = "发送的键值 ：" + keycode.ToString("X");
            }
            
        }

        private void IRemote_Load(object sender, EventArgs e)
        {

            asc.controllInitializeSize(this);
            string[] strs2 = File.ReadAllLines(@"系统文件\运行配置文件.txt");

            if (strs2.Length != 0)
            {
                for (int i = 0; i < strs2.Length; i++)
                {
                    comboBox1.Items.Add("模式" + i.ToString());
                }
            }

            comboBox1.SelectedIndex = 0;

       
            Iruncode();
            button39.Text = "遥控器关闭";
            button39.BackColor = Color.FromArgb(255, 128, 128);
            timer1.Enabled = true;
            timer1.Stop();
            remoteflag = true;
        }

        private void Button33_Click(object sender, EventArgs e)
        {
            if (ToolPluse.ComDevice.IsOpen == true)
            {
                timer1.Stop();
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
                ToolPluse.ComDevice.Parity = Parity.None;
                ToolPluse.ComDevice.PortName = ToolPluse.sportname;
                ToolPluse.ComDevice.DataBits = 8;
                button33.ForeColor = Color.Red;
                try
                {
                    ToolPluse.ComDevice.Open();

                    button33.Text = ToolPluse.ComDevice.IsOpen ? "串口关闭" : "串口打开";
                    timer1.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
              
               
            }
        }
        private void Button39_Click(object sender, EventArgs e)
        {
            if (button39.Text == "遥控器开启")
            {
                button39.Text = "遥控器关闭";
                button39.BackColor = Color.FromArgb(255, 128, 128);
                remoteflag = true;
            }
            else
            {
                button39.Text = "遥控器开启";
                button39.BackColor = Color.FromArgb(121, 240, 189);
                remoteflag = false;

            }
        }
        private void IRemote_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ToolPluse.ComDevice.IsOpen)
            {
                ToolPluse.ComDevice.DataReceived -= usart_receive;
                closing = true;
                while (Listening) Application.DoEvents();
                //打开时点击，则关闭串口   
                ToolPluse.ComDevice.Close();
                closing = false;
            }
            ToolPluse.ComDevice.DataReceived -= usart_receive;
        }

   

        private void Button37_KeyUp(object sender, KeyEventArgs e)
        {
            button39.Text = "11";
        }


        private void Button1_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button1.Text];
        }

        private void Button1_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }
        private void Button2_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button2.Text];
        }
        private void Button2_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }
        private void Button3_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button3.Text];
        }
        private void Button3_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }
        

        private void Button38_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = 0x80000111;
        }

        private void Button38_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button4_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button4.Text];
        }

        private void Button4_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button18_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button18.Text];
        }

        private void Button18_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button5_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button5.Text];
        }

        private void Button5_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button6_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button6.Text];
        }

        private void Button6_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button7_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button7.Text];
        }

        private void Button7_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button8_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button8.Text];
        }

        private void Button8_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button9_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button9.Text];
        }

        private void Button9_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button10_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button10.Text];
        }

        private void Button10_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button11_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button11.Text];
        }

        private void Button11_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button12_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button12.Text];
        }

        private void Button12_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button13_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button13.Text];
        }

        private void Button13_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button14_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button14.Text];
        }

        private void Button14_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button15_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button15.Text];
        }

        private void Button15_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button16_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button16.Text];
        }

        private void Button16_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button17_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button17.Text];
        }

        private void Button17_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button19_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button19.Text];
        }

        private void Button19_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button20_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button20.Text];
        }

        private void Button20_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button21_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button21.Text];
        }

        private void Button21_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button22_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button22.Text];
        }

        private void Button22_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button23_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button23.Text];
        }

        private void Button23_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button24_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button24.Text];
        }

        private void Button24_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button25_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button25.Text];
        }

        private void Button25_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button26_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button26.Text];
        }

        private void Button26_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button27_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button27.Text];
        }

        private void Button27_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button28_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button28.Text];
        }

        private void Button28_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button29_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button29.Text];
        }

        private void Button29_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button30_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button30.Text];
        }

        private void Button30_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button31_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith[button31.Text];
        }

        private void Button31_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button34_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";      
            richTextBox3.Text = "";
        }

        private void Button35_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = openWith["ALL_FLAT"] | openWith["ZERO_G"];
        }

        private void Button35_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void Button7_Click(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {

        }

        private void Button38_Click(object sender, EventArgs e)
        {

        }

        private void IRemote_SizeChanged(object sender, EventArgs e)
        {
            asc.controlAutoSize(this);
        }

        private void Button32_MouseDown(object sender, MouseEventArgs e)
        {
            keycode = 0x80000000;
        }

        private void Button32_MouseUp(object sender, MouseEventArgs e)
        {
            keycode = 0;
        }

        private void button36_Click(object sender, EventArgs e)
        {
            if (button36.Text == "自动运行")
            {
                button36.Text = "停止运行";
                timer2.Enabled = true;
                timer2.Start();
                iruntimer = 0;
                runcount = 0;
            }
            else
            {
                button36.Text = "自动运行";
                timer2.Enabled = false;
                timer2.Stop();
                iruntimer = 0;
                runcount = 0;
            }
            
        }

        void Iruncode()
        {                
            int i = 0;
            string[] strs2 = File.ReadAllLines(@"系统文件\运行配置文件.txt");
            string strtemp;
            string[,] strkey =  new string [10, 10];
            strtemp = strs2[comboBox1.SelectedIndex];
            string[] sArray = strtemp.Split(';',':');
            irunsetp = sArray.Length;

            for (i = 0; i < sArray.Length / 4; i++)
            {
                cmdinfo[i].strkey = sArray[i * 4 + 1];
                cmdinfo[i].timertype = sArray[i * 4 + 2];
                cmdinfo[i].timer = Convert.ToUInt32(sArray[i * 4 + 3]);
            }
            //步骤显示
            strtemp = "";
            for (i = 0; i < sArray.Length / 4; i++)
            {

                if (cmdinfo[i].strkey != "NONE")
                {
                    strtemp += cmdinfo[i].strkey;

                    if (cmdinfo[i].timertype == "continue")
                    {
                        strtemp += " 持续" + cmdinfo[i].timer.ToString()+"\r\n";
                    }
                    else if (cmdinfo[i].timertype == "delay")
                    {
                        strtemp += "延迟" + cmdinfo[i].timer.ToString() + "\r\n";
                    }
                }
                else if (cmdinfo[i].strkey == "NONE")
                {
                    strtemp += cmdinfo[i].strkey;

                    if (cmdinfo[i].timertype == "continue")
                    {
                        strtemp += " 持续" + cmdinfo[i].timer.ToString() + "\r\n";
                    }
                    else if (cmdinfo[i].timertype == "delay")
                    {
                        strtemp += " 延迟" + cmdinfo[i].timer.ToString() + "\r\n";
                    }
                }               
            }

            richTextBox2.Text = strtemp;

        }


        void dispalyCurrent(int i)
        {
            
        }


        void mysetrun()
        {
            int i = 0;
            string strtemp = "";
            //显示当前运行信息


            for (i = 0; i < irunsetp; i++)
            {

                if (cmdinfo[i].strkey != "NONE")
                {
                    strtemp += cmdinfo[i].strkey;

                    if (cmdinfo[i].timertype == "continue")
                    {
                        strtemp += " 持续" + cmdinfo[i].timer.ToString() + "\r\n";
                    }
                    else if (cmdinfo[i].timertype == "delay")
                    {
                        strtemp += "延迟" + cmdinfo[i].timer.ToString() + "\r\n";
                    }
                }
                else if (cmdinfo[i].strkey == "NONE")
                {
                    strtemp += cmdinfo[i].strkey;

                    if (cmdinfo[i].timertype == "continue")
                    {
                        strtemp += " 持续" + cmdinfo[i].timer.ToString() + "\r\n";
                    }
                    else if (cmdinfo[i].timertype == "delay")
                    {
                        strtemp += " 延迟" + cmdinfo[i].timer.ToString() + "\r\n";
                    }
                }
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            iruntimer++;
       
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
             Iruncode();
        }

        private void button37_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = "key:HEAD_OUT;continue:30;key:NONE;delay:20;key:HEAD_IN;continue:30;key:NONE;delay:20;";
        }

        private void button40_Click(object sender, EventArgs e)
        {

        }
    }
}
