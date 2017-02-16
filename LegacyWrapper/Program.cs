using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LegacyWrapper.Common.Serialization;
using LegacyWrapper.ErrorHandling;
using LegacyWrapper.Interop;

namespace LegacyWrapper
{
    public class Program
    {
        /// <summary>
        /// Main method of the legacy dll wrapper.
        /// </summary>
        /// <param name="args">
        /// The first parameter is expected to be a string.
        /// The Wrapper will use this string to create a named pipe.
        /// </param>
        static void Main(string[] args)
        {
            if (args.Length < 1) return;

            string token = args[0];

            // Create new named pipe with token from client
            using (var pipe = new NamedPipeServerStream(token, PipeDirection.InOut, 1, PipeTransmissionMode.Message))
            {
                pipe.WaitForConnection();

                // Receive CallData from client
                var formatter = new BinaryFormatter();
                CallData data;

                while ((data = (CallData)formatter.Deserialize(pipe)).Status != KeepAliveStatus.Close)
                {
                    LoadLibrary(data, formatter, pipe);
                }
            }
        }

        private static void LoadLibrary(CallData data, IFormatter formatter, Stream pipeStream)
        {
            try
            {
                // Load requested library
                using (var library = NativeLibrary.Load(data.Library, NativeLibraryLoadOptions.SearchAll))
                {
                    IntPtr func = library.GetFunctionPointer(data.ProcedureName);
                    var method = Marshal.GetDelegateForFunctionPointer(func, data.Delegate);

                    // Invoke requested method
                    object result = method.DynamicInvoke(data.Parameters);

                    // Write result back to client
                    formatter.Serialize(pipeStream, result);
                }
            }
            catch (Exception e)
            {
                // Write Exception to client
                formatter.Serialize(pipeStream, new LegacyWrapperException(
                    "An error occured while calling a library function. See the inner exception for details.", e));
            }
        }
    }
}
