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
        /// <param name="pipeToken"></param>
        [HandleProcessCorruptedStateExceptions]
        public void Call(string pipeToken)
        {
            using (var pipe = CreatePipeStream(pipeToken))
            {
                pipe.WaitForConnection();

                try
                {
                    ProcessMessages(pipe);
                }
                catch (Exception e)
                {
                    WriteExceptionToClient(pipe, e);
                }
            }
        }

        private void ProcessMessages(PipeStream pipe)
        {
            CallData data;
            do
            {
                data = (CallData)Formatter.Deserialize(pipe);
                InvokeFunction(data, pipe);
            } while (data.Status != KeepAliveStatus.Close);
        }

        private NamedPipeServerStream CreatePipeStream(string token)
        {
            PipeDirection pipeDirection = PipeDirection.InOut;
            PipeTransmissionMode pipeTransmissionMode = PipeTransmissionMode.Message;
            int maxNumberOfServerInstances = 1;

            return new NamedPipeServerStream(token, pipeDirection, maxNumberOfServerInstances, pipeTransmissionMode);
        }

        private void InvokeFunction(CallData callData, Stream pipe)
        {
            CallResult callResult = UnmanagedLibraryLoader.InvokeUnmanagedFunction(callData);

            Formatter.Serialize(pipe, callResult);
        }

        private void WriteExceptionToClient(Stream pipe, Exception e)
        {
            string errorMessage = "An error occured while calling a library function. See the inner exception for details.";

            CallResult callResult = new CallResult();
            callResult.Exception = new LegacyWrapperException(errorMessage, e);

            Formatter.Serialize(pipe, callResult);
        }
    }
}
