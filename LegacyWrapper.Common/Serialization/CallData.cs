using System;
using System.Runtime.InteropServices;
using LegacyWrapper.Common.Attributes;

namespace LegacyWrapper.Common.Serialization
{
    /// <summary>
    /// Class to transmit info to the server. The server will execute an appropriate call and eventually return the results.
    /// </summary>
    [Serializable]
    public class CallData
    {
        public CallData()
        {
            Status = KeepAliveStatus.KeepAlive;
        }

        public string LibraryName { get; set; }

        /// <summary>
        /// Name of the procedure to call.
        /// </summary>
        public string ProcedureName { get; set; }

        /// <summary>
        /// Array of parameters to pass to the function call.
        /// </summary>
        public object[] Parameters { get; set; }

        public Type[] ParameterTypes { get; set; }

        /// <summary>
        /// Type of the called function's return value
        /// </summary>
        public Type ReturnType { get; set; }

        public CallingConvention CallingConvention { get; set; }

        public CharSet CharSet { get; set; }

        /// <summary>
        /// Status indicating if the wrapper executable should close the connection and terminate itself
        /// </summary>
        public KeepAliveStatus Status { get; set; }
    }

    public enum KeepAliveStatus
    {
        KeepAlive,
        Close
    }
}
