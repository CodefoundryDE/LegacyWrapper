using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapperClient.Client
{
    internal interface IWrapperProcessStarter : IDisposable
    {
        void StartWrapperProcess();
    }
}
