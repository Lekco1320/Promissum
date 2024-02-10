#nullable disable
using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;

// Modified from https://www.cnblogs.com/dongweian/p/15750448.html.
namespace Lekco.Promissum.Apps
{
    public class PipeClient : PipeBase
    {
        private NamedPipeClientStream Client { get; set; }
        public PipeClient(string serverName, string pipeName)
        {
            Client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None);
            Client.Connect();
            PipeReader = new StreamReader(Client);
            PipeWriter = new StreamWriter(Client) { AutoFlush = true };
        }

        public void DoRequest(string input)
        {
            if (!Client.IsConnected)
            {
                throw new Exception("Connected connection has been disrupted.");
            }
            Send(input);
        }

        public void CloseServer()
        {
            Send(Exit);
            Client.Close();
        }
    }
}
