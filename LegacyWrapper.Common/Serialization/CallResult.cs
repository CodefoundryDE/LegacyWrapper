using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapper.Common.Serialization
{
    /// <summary>
    /// Class used to return the call result back to the client. 
    /// Includes return value, eventually changed ref params, and if thrown in the wrapper, an exception.
    /// </summary>
    [Serializable]
    public class CallResult
    {
        /// <summary>
        /// Result object of the call.
        /// </summary>
        public object Result { get; set; }

        /// <summary>
        /// Array of parameters passed to the function call.
        /// The original parameters may have changed due to ref parameters used in the dll function.
        /// </summary>
        public object[] Parameters { get; set; }

        /// <summary>
        /// If the library call in the wrapper throws an exception, it will be delivered here.
        /// </summary>
        public Exception Exception { get; set; }
    }
}
