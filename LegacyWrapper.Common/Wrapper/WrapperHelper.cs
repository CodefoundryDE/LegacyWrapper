using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Interop;
using LegacyWrapper.Common.Serialization;
using LegacyWrapper.ErrorHandling;

namespace LegacyWrapper.Common.Wrapper
{
    public class WrapperHelper
    {
        private static readonly IFormatter Formatter = new BinaryFormatter();

        /// <summary>
        /// Outsourced main method of the legacy dll wrapper.
        /// </summary>
        /// <param name="args">
        /// The first parameter is expected to be a string.
        /// The Wrapper will use this string to create a named pipe.
        /// </param>
        [HandleProcessCorruptedStateExceptions]
        public void Call(string[] args)
        {
            if (args.Length != 1)
            {
                return;
            }

            string token = args[0];

            // Create new named pipe with token from client
            using (var pipe = CreatePipeStream(token))
            {
                pipe.WaitForConnection();

                try
                {
                    CallData data;
                    do
                    {
                        data = (CallData)Formatter.Deserialize(pipe);
                        InvokeFunction(data, pipe);
                    } while (data.Status != KeepAliveStatus.Close);
                }
                catch (Exception e)
                {
                    WriteExceptionToClient(pipe, e);
                }
            }
        }

        private NamedPipeServerStream CreatePipeStream(string token)
        {
            PipeDirection pipeDirection = PipeDirection.InOut;
            PipeTransmissionMode pipeTransmissionMode = PipeTransmissionMode.Message;
            int maxNumberOfServerInstances = 1;

            return new NamedPipeServerStream(token, pipeDirection, maxNumberOfServerInstances, pipeTransmissionMode);
        }

        [HandleProcessCorruptedStateExceptions]
        private void InvokeFunction(CallData callData, Stream pipe)
        {
            CallResult callResult = new UnmanagedLibraryLoader().InvokeUnmanagedFunction(callData);

            Formatter.Serialize(pipe, callResult);
        }

        private void WriteExceptionToClient(Stream pipe, Exception e)
        {
            CallResult callResult = new CallResult();
            callResult.Exception = new LegacyWrapperException("An error occured while calling a library function. See the inner exception for details.", e);

            Formatter.Serialize(pipe, callResult);
        }
    }
}
