using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapperClient.ProcessHandling
{
    internal interface IWrapperProcessStarter : IDisposable
    {
        void StartWrapperProcess();
    }
}
