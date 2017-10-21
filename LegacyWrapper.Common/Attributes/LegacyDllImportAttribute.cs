using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapper.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public class LegacyDllImportAttribute : Attribute
    {
        public string LibraryName { get; }

        public LegacyDllImportAttribute(string libraryName)
        {
            if (libraryName == null)
            {
                throw new ArgumentNullException(nameof(libraryName));
            }

            if (string.IsNullOrWhiteSpace(libraryName))
            {
                throw new ArgumentException($"Parameter {nameof(libraryName)} must not be empty!");
            }

            LibraryName = libraryName;
        }
    }
}
