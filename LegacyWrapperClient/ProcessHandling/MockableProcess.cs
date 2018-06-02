using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapperClient.ProcessHandling
{
    /// <summary>
    /// This class is a wrapper for System.Diagnostics.Process for mocking purposes.
    /// </summary>
    internal class MockableProcess : Process
    {
        public new virtual bool Start()
        {
            return base.Start();
        }
    }
}
