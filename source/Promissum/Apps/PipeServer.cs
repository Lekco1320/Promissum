#nullable disable
using System;
using System.IO;
using System.IO.Pipes;

// Modified from https://www.cnblogs.com/dongweian/p/15750448.html.
namespace Lekco.Promissum.Apps
{
    public class PipeServer : PipeBase
    {
        private NamedPipeServerStream Server { get; set; }
        public Action<string> Received { get; set; }
        public PipeServer(string pipeName)
        {
            Server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1);
            Server.WaitForConnection();
            PipeReader = new StreamReader(Server);
            PipeWriter = new StreamWriter(Server) { AutoFlush = true };
        }

        public new void Send(string input)
        {
            if (Server.IsConnected == false)
            {
                throw new Exception($"连接通过已断开");
            }
            base.Send(input);
        }

        public void BeginReceive()
        {
            while (true)
            {
                if (Server.IsConnected == false)
                {
                    throw new Exception($"连接通过已断开");
                }
                if (Receive(out string data))
                {
                    Received?.Invoke(data);
                }
                else
                {
                    return;
                }
            }
        }

        public void Close()
        {
            Server.Close();
        }

        public void WaitForConnection()
        {
            Server.WaitForConnection();
        }
    }
}
