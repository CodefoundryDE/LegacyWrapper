using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapper.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class LegacyDllMethodAttribute : Attribute
    {
        public CallingConvention CallingConvention { get; set; }
        public CharSet CharSet { get; set; }

        public LegacyDllMethodAttribute()
        {
            CallingConvention = CallingConvention.StdCall;
            CharSet = CharSet.Auto;
        }
    }
}
