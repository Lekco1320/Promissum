using System.IO;
using System.IO.Pipes;

namespace Lekco.Promissum.App
{
    /// <summary>
    /// The class describing a client of a named pipe.
    /// </summary>
    public class NamedPipeClient
    {
        /// <summary>
        /// Name of this pipe.
        /// </summary>
        public string PipeName { get; }

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="pipeName">Name of this pipe.</param>
        public NamedPipeClient(string pipeName)
        {
            PipeName = pipeName;
        }

        /// <summary>
        /// Send arguments to the server of pipe.
        /// </summary>
        /// <param name="arg">Arguments to send.</param>
        public void Send(string arg)
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut);
            client.Connect();
            using var writer = new StreamWriter(client);
            writer.AutoFlush = true;
            writer.Write(arg);
        }
    }
}
