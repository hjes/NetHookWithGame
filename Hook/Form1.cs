using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace Hook
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("ws2_32.dll")]
        public static extern int recv(int s1, IntPtr buf1, int len1, int flag1);
        [DllImport("ws2_32.dll")]
        public static extern int send(int s, byte[] buf, int len, int flag);
        APIHOOK send_Hook = new APIHOOK();
        APIHOOK recv_Hook = new APIHOOK();
        public delegate int sendCallback(int s, IntPtr buf, int len, int flag);
        public delegate int recvCallback(int s1, IntPtr buf1, int len1, int flag1);
        private static Delegate deleg;
        private static Delegate delegR;
      
        private void Form1_Load(object sender, EventArgs e)
        {
            webBrowser1.Navigate("http://baidu.com");
            deleg = new sendCallback(sendProc);
            delegR = new recvCallback(recvProc);
            send_Hook.Install("ws2_32.dll", "send", Marshal.GetFunctionPointerForDelegate(deleg));
            recv_Hook.Install("ws2_32.dll", "recv", Marshal.GetFunctionPointerForDelegate(delegR));
        }
        int sendProc(int s, IntPtr buf, int len, int flag)
        {
           
            int ret = 0;
            byte[] buffer = new byte[len];
            Marshal.Copy(buf, buffer, 0, len); //读封包数据 
            string sendstr = Encoding.UTF8.GetString(buffer);
            Console.WriteLine("Send:"+sendstr);
            send_Hook.Suspend(); //暂停拦截，转交系统调用
            ret = send(s, buffer, len, flag); //发送数据，此处可进行拦截
            send_Hook.Continue(); //恢复HOOK  
            return ret;
        }
        int recvProc(int s2, IntPtr buf2, int len2, int flag2)
        {
            int ret = 0;
            byte[] buffer1 = new byte[len2];
            recv_Hook.Suspend(); //暂停拦截，转交系统调用 
            ret = recv(s2, buf2, len2, flag2); //接收数据，此处可进行拦截
            recv_Hook.Continue(); //恢复HOOK
            if (ret == -1) //SOCKET_ERROR
                return ret;
            byte[] buffer2 = new byte[ret];
            Marshal.Copy(buf2, buffer2, 0, buffer2.Length); //读封包数据 
            Console.WriteLine("Recv:"+Encoding.UTF8.GetString(buffer2));
            return ret;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate("http://baidu.com");
        }
    }
}
