using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Collections;
 

namespace Hook
{
    class APIHOOK
    {
        #region Api声明
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleA", SetLastError = true, CharSet = CharSet.Ansi, ThrowOnUnmappableChar = true)]
        public static extern IntPtr GetModuleHandleA(String lpModuleName);
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true, CharSet = CharSet.Unicode, ThrowOnUnmappableChar = true)]
        public static extern IntPtr GetModuleHandleW(String lpModuleName);
        [DllImport("Kernel32.dll")]
        static extern bool VirtualProtect(
            IntPtr lpAddress,
            int dwSize,
            int flNewProtect,
            ref int lpflOldProtect
            );
        [DllImport("Kernel32.dll", EntryPoint = "lstrcpynA", CharSet = CharSet.Ansi)]
        static extern IntPtr lstrcpyn(
            byte[] lpString1,
            byte[] lpString2,
            int iMaxLength
            );
        [DllImport("Kernel32.dll")]
        static extern IntPtr GetProcAddress(
            IntPtr hModule,
            string lpProcName
            );
        [DllImport("Kernel32.dll")]
        static extern bool FreeLibrary(
            IntPtr hModule
            );
        #endregion
        #region 常量定义表
        int PAGE_EXECUTE_READWRITE = 0x40;
        #endregion
        #region 变量表
        public IntPtr ProcAddress;
        int lpflOldProtect = 0;
        byte[] OldEntry = new byte[5];
        byte[] NewEntry = new byte[5];
        IntPtr OldAddress;
        #endregion
        public APIHOOK() { }
        public APIHOOK(string ModuleName, string ProcName, IntPtr lpAddress)
        {
            Install(ModuleName, ProcName, lpAddress);
        }
        public bool Install(string ModuleName, string ProcName, IntPtr lpAddress)
        {
            IntPtr hModule = GetModuleHandleA(ModuleName); //取模块句柄   
            if (hModule == IntPtr.Zero)
            {
                Console.WriteLine("您的机子因Sock问题无法运行此软件");
                return false;
            }
            try
            {
                ProcAddress = GetProcAddress(hModule, ProcName); //取入口地址   
                if (ProcAddress == IntPtr.Zero) return false;
                if (!VirtualProtect(ProcAddress, 5, PAGE_EXECUTE_READWRITE, ref lpflOldProtect)) return false; //修改内存属性   
                Marshal.Copy(ProcAddress, OldEntry, 0, 5); //读取前5字节   
                NewEntry = AddBytes(new byte[1] { 233 }, BitConverter.GetBytes((Int32)((Int32)lpAddress - (Int32)ProcAddress - 5))); //计算新入口跳转   
                Marshal.Copy(NewEntry, 0, ProcAddress, 5); //写入前5字节   
                OldEntry = AddBytes(OldEntry, new byte[5] { 233, 0, 0, 0, 0 });
                OldAddress = lstrcpyn(OldEntry, OldEntry, 0); //取变量指针   
                Marshal.Copy(BitConverter.GetBytes((double)((Int32)ProcAddress - (Int32)OldAddress - 5)), 0, (IntPtr)(OldAddress.ToInt32() + 6), 4); //保存JMP   
                FreeLibrary(hModule); //释放模块句柄   
            }
            catch (Exception)
            {
               Console.WriteLine("您的机子无法运行此软件");
               return false;

            }
            return true;
        }
        public void Suspend()
        {
            Marshal.Copy(OldEntry, 0, ProcAddress, 5);
        }
        public void Continue()
        {
            Marshal.Copy(NewEntry, 0, ProcAddress, 5);
        }
        public bool Uninstall()
        {
            if (ProcAddress == IntPtr.Zero) return false;
            Marshal.Copy(OldEntry, 0, ProcAddress, 5);
            ProcAddress = IntPtr.Zero;
            return true;
        }
        static byte[] AddBytes(byte[] a, byte[] b)
        {
            ArrayList retArray = new ArrayList();
            for (int i = 0; i < a.Length; i++)
            {
                retArray.Add(a[i]);
            }
            for (int i = 0; i < b.Length; i++)
            {
                retArray.Add(b[i]);
            }
            return (byte[])retArray.ToArray(typeof(byte));
        }
    }
}
