#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

// Modified from https://www.cnblogs.com/dongweian/p/15750448.html.
namespace Lekco.Promissum.Apps
{
    public class PipeBase
    {
        protected Encoding Encoding { get; set; } = Encoding.UTF8;
        protected StreamWriter PipeWriter { get; set; }
        protected StreamReader PipeReader { get; set; }

        protected const string Exit = nameof(Exit);
        protected void Send(string input)
        {
            string base64 = Convert.ToBase64String(Encoding.GetBytes(input));
            PipeWriter.WriteLine(base64);
        }

        protected bool Receive([MaybeNullWhen(false)] out string output)
        {
            output = null;
            string base64 = PipeReader.ReadLine();
            if (string.IsNullOrEmpty(base64))
            {
                return false;
            }
            var bytes = Convert.FromBase64String(base64);
            output = Encoding.GetString(bytes);
            if (output == Exit)
            {
                return false;
            }
            return true;
        }
    }
}
