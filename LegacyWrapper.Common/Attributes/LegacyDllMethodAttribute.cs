using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapper.Common.Attributes
{
    /// <summary>
    /// Marks a method in an interface for usage with the WrapperClient.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class LegacyDllMethodAttribute : Attribute
    {
        /// <summary>
        /// CallingConvention to use when calling a P/Invoke function.
        /// </summary>
        public CallingConvention CallingConvention { get; set; }

        /// <summary>
        /// Charset to use when calling a P/Invoke function.
        /// </summary>
        public CharSet CharSet { get; set; }

        /// <summary>
        /// Creates a new instance of LegacyDllMethodAttribute.
        /// </summary>
        public LegacyDllMethodAttribute()
        {
            CallingConvention = CallingConvention.StdCall;
            CharSet = CharSet.Auto;
        }
    }
}
