using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapperClient.Architecture
{
    /// <summary>
    /// Target architecture enumeration.
    /// Indicates which architecture the DLL is compiled to.
    /// </summary>
    public enum TargetArchitecture
    {
        /// <summary>
        /// The DLL is compiled for 32bit (x86) architectures.
        /// </summary>
        X86,

        /// <summary>
        /// The DLL is compiled for 64bit (AMD64) architectures.
        /// </summary>
        Amd64
    }
}
