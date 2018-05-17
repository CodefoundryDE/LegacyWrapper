using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Attributes;

namespace LegacyWrapperClient.Configuration
{
    internal interface ILibraryNameProvider
    {
        string GetLibraryName(LegacyDllImportAttribute attribute);
    }
}
