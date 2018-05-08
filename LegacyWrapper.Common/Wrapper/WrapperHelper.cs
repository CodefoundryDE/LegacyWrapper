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
        public static void Call(string[] args)
        {
            if (args.Length != 1)
            {
                return;
            }

            string token = args[0];

            // Create new named pipe with token from client
            using (var pipe = new NamedPipeServerStream(token, PipeDirection.InOut, 1, PipeTransmissionMode.Message))
            {
                pipe.WaitForConnection();

                try
                {
                    CallData data = (CallData)Formatter.Deserialize(pipe);

                    // Load requested library

                    // Receive CallData from client

                    while (data.Status != KeepAliveStatus.Close)
                    {
                        InvokeFunction(data, pipe);

                        data = (CallData)Formatter.Deserialize(pipe);
                    }
                }
                catch (Exception e)
                {
                    WriteExceptionToClient(pipe, e);
                }
            }
        }

        [HandleProcessCorruptedStateExceptions]
        private static void InvokeFunction(CallData data, Stream pipe)
        {
            Type dllHandle = CreateTypeBuilder(data);

            // Invoke requested method
            object result = dllHandle.GetMethod(data.ProcedureName)
                                     .Invoke(null, data.Parameters);

            CallResult callResult = new CallResult
            {
                Result = result,
                Parameters = data.Parameters,
            };

            // Write result back to client
            Formatter.Serialize(pipe, callResult);
        }

        private static Type CreateTypeBuilder(CallData callData)
        {
            AssemblyName asmName = new AssemblyName("LegacyWrapper");
            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule("LegacyWrapper", emitSymbolInfo: false);
            TypeBuilder typeBuilder = modBuilder.DefineType("LegacyWrapper.WrapperType", TypeAttributes.Class | TypeAttributes.Public);

            MethodBuilder pinvokeBuilder = typeBuilder.DefinePInvokeMethod(
                name: callData.ProcedureName,
                dllName: callData.LibraryName,
                attributes: MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.PinvokeImpl,
                callingConvention: CallingConventions.Standard,
                returnType: callData.ReturnType,
                parameterTypes: callData.ParameterTypes,
                nativeCallConv: callData.CallingConvention,
                nativeCharSet: callData.CharSet);

            pinvokeBuilder.SetImplementationFlags(pinvokeBuilder.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);

            return typeBuilder.CreateType();
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
