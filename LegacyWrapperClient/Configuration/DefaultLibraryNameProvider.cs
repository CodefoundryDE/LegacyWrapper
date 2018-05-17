using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Attributes;
using PommaLabs.Thrower;

namespace LegacyWrapperClient.Configuration
{
    internal class DefaultLibraryNameProvider : ILibraryNameProvider
    {
        public string GetLibraryName(LegacyDllImportAttribute attribute)
        {
            Raise.ArgumentNullException.IfIsNull(attribute, nameof(attribute));

            return attribute.LibraryName;
        }
    }
}
