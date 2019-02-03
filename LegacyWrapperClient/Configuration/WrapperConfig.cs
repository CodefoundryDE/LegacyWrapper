using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapperClient.Architecture;

namespace LegacyWrapperClient.Configuration
{
    internal class WrapperConfig : IWrapperConfig
    {
        public TargetArchitecture TargetArchitecture { get; set; }
    }
}
