using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LegacyWrapper.Dll;

namespace LegacyWrapperClient.Client
{
    public static class WrapperClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dllName"></param>
        /// <param name="function"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Call<T>(string dllName, string function, object[] args) where T : class
        { 
            string token = Guid.NewGuid().ToString();

            // Pass token to child process
            Process.Start("LegacyWrapper", token);

            using (var pipe = new NamedPipeClientStream(".", token, PipeDirection.InOut))
            {
                pipe.Connect();
                pipe.ReadMode = PipeTransmissionMode.Message;
                var info = new CallData
                {
                    Library = dllName,
                    ProcedureName = function,
                    Parameters = args,
                    Delegate = typeof(T),
                };
                var formatter = new BinaryFormatter();
                // Write request to server
                formatter.Serialize(pipe, info);

                // Receive result from server
                object result = formatter.Deserialize(pipe);

                var exception = result as Exception;
                if (exception != null)
                {
                    throw exception;
                }

                return result;
            }
        }
    }

}
