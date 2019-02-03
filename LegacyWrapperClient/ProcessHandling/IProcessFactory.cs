using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapperClient.ProcessHandling
{
    interface IProcessFactory
    {
        MockableProcess GetProcess(string executableName, string args);
    }
}
