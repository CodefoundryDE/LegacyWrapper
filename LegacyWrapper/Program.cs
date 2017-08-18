using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LegacyWrapper.Common.Interop;
using LegacyWrapper.Common.Serialization;
using LegacyWrapper.ErrorHandling;

namespace LegacyWrapper
{
    public class Program
    {
        private static readonly IFormatter Formatter = new BinaryFormatter();

        /// <summary>
        /// Main method of the legacy dll wrapper.
        /// </summary>
        /// <param name="args">
        /// The first parameter is expected to be a string.
        /// The Wrapper will use this string to create a named pipe.
        /// </param>
        [HandleProcessCorruptedStateExceptions]
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                return;
            }

            string token = args[0];
            string libraryName = args[1];

            // Create new named pipe with token from client
            using (var pipe = new NamedPipeServerStream(token, PipeDirection.InOut, 1, PipeTransmissionMode.Message))
            {
                pipe.WaitForConnection();

                try
                {
                    CallData data = (CallData)Formatter.Deserialize(pipe);

                    // Load requested library
                    using (NativeLibrary library = NativeLibrary.Load(libraryName, NativeLibraryLoadOptions.SearchAll))
                    {
                        // Receive CallData from client
                        
                        while (data.Status != KeepAliveStatus.Close)
                        {
                            InvokeFunction(data, pipe, library);

                            data = (CallData)Formatter.Deserialize(pipe);
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteExceptionToClient(pipe, e);
                }
            }
        }

        [HandleProcessCorruptedStateExceptions]
        private static void InvokeFunction(CallData data, Stream pipe, NativeLibrary library)
        {
            try
            {
                IntPtr func = library.GetFunctionPointer(data.ProcedureName);
                Delegate method = Marshal.GetDelegateForFunctionPointer(func, data.Delegate);

                // Invoke requested method
                object result = method.DynamicInvoke(data.Parameters);

                CallResult callResult = new CallResult
                {
                    Result = result,
                    Parameters = data.Parameters,
                };

                // Write result back to client
                Formatter.Serialize(pipe, callResult);
            }
            catch (Exception e)
            {
                WriteExceptionToClient(pipe, e);
            }
        }

        private static void WriteExceptionToClient(Stream pipe, Exception e)
        {
            Formatter.Serialize(pipe, new CallResult
            {
                Exception = new LegacyWrapperException("An error occured while calling a library function. See the inner exception for details.", e),
            });
        }
    }
}
