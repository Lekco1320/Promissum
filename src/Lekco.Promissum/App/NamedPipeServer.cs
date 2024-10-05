using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Lekco.Promissum.App
{
    /// <summary>
    /// The class describing a server of a named pipe.
    /// </summary>
    public class NamedPipeServer
    {
        /// <summary>
        /// Name of this pipe.
        /// </summary>
        public string PipeName { get; }

        /// <summary>
        /// Indicates whether the server is running.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Occurs when receiving arguments.
        /// </summary>
        public event EventHandler<string>? OnReceivedArg;

        /// <summary>
        /// Create an instance.
        /// </summary>
        /// <param name="pipeName">Name of this pipe.</param>
        public NamedPipeServer(string pipeName)
        {
            PipeName = pipeName;
        }

        /// <summary>
        /// Start up this server asynchronously.
        /// </summary>
        /// <returns>Task for starting up this server.</returns>
        public async Task StartUpAsync()
        {
            while (IsRunning)
            {
                // Create and waiting clients' connection.
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await server.WaitForConnectionAsync();

                // Read and cope with arguments.
                using var reader = new StreamReader(server);
                string receivedArgs = await reader.ReadToEndAsync();
                OnReceivedArg?.Invoke(this, receivedArgs);
            }
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
        }
    }
}
