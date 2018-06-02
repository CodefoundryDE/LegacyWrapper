using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.ProcessHandling
{
    internal class ProcessFactory : IProcessFactory
    {
        public virtual MockableProcess GetProcess(string executableName, string args)
        {
            Raise.ArgumentNullException.IfIsNull(executableName, nameof(executableName));
            Raise.ArgumentNullException.IfIsNull(args, nameof(args));

            MockableProcess process = new MockableProcess();
            process.StartInfo = new ProcessStartInfo(executableName, args);

            return process;
        }
    }
}
