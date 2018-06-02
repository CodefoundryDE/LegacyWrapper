using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapperClient.Configuration
{
    internal interface IWrapperExecutableNameProvider
    {
        string GetWrapperExecutableName();
    }
}
