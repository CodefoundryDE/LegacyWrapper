using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapperClient.Dll
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

        /// <summary>
        /// Name of the library to call.
        /// </summary>
        public string Library { get; set; }

        /// <summary>
        /// Name of the procedure to call.
        /// </summary>
        public string ProcedureName { get; set; }

        /// <summary>
        /// Array of parameters to pass to the function call.
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// Delegate type to use for the call.
        /// </summary>
        public Type Delegate { get; set; }

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
