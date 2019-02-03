using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Attributes;
using LegacyWrapperClient.Architecture;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.Configuration
{
    internal class DefaultWrapperExecutableNameProvider : IWrapperExecutableNameProvider
    {
        private readonly IReadOnlyDictionary<TargetArchitecture, string> _wrapperNames = new Dictionary<TargetArchitecture, string>
        {
            { TargetArchitecture.X86,   "Codefoundry.LegacyWrapper32.exe" },
            { TargetArchitecture.Amd64, "Codefoundry.LegacyWrapper64.exe" },
        };

        private readonly IWrapperConfig _configuration;

        public DefaultWrapperExecutableNameProvider(IWrapperConfig configuration)
        {
            Raise.ArgumentNullException.IfIsNull(configuration);

            _configuration = configuration;
        }

        public string GetWrapperExecutableName()
        {
            return _wrapperNames[_configuration.TargetArchitecture];
        }
    }
}
