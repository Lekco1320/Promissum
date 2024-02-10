using Microsoft.Win32;
using System.Security.AccessControl;

namespace Promissum.Agens
{
    internal class Program
    {
        static readonly string ProgramName;
        static readonly string ProgramPath;

        static Program()
        {
            ProgramName = "LekcoPromissum";
            string[] args = Environment.GetCommandLineArgs();
            ProgramPath = args[0][..args[0].LastIndexOf('\\')] + @"\Lekco.Promissum.exe";
        }

        static void Main(string[] args)
        {
            bool ret = true;
            foreach (var arg in args)
            {
                if (arg == "-enableAutoStart")
                {
                    ret &= EnableAutoStart();
                }
                if (arg == "-disableAutoStart")
                {
                    ret &= DisableAutoStart();
                }
            }
            Environment.Exit(ret ? 0 : -1);
        }

        static bool EnableAutoStart()
        {
            try
            {
                var myReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                //如果子键节点不存在，则创建之
                myReg ??= Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (myReg.GetValue(ProgramName) != null)
                {
                    //在注册表中设置自启动程序
                    myReg.DeleteValue(ProgramName);
                }
                myReg.SetValue(ProgramName, '"' + ProgramPath + "\" -background");
            }
            catch
            {
                return false;
            }
            return true;
        }

        static bool DisableAutoStart()
        {
            try
            {
                var myReg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                if (myReg?.GetValue(ProgramName) != null)
                {
                    myReg.DeleteValue(ProgramName);
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
