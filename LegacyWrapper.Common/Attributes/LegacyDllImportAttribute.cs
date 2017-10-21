using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapper.Common.Attributes
{
    /// <summary>
    /// This attribute marks an interface for usage with the WrapperClient.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public class LegacyDllImportAttribute : Attribute
    {
        /// <summary>
        /// The name of the library to load.
        /// </summary>
        public string LibraryName { get; }

        /// <summary>
        /// Creates a new instance of LegacyDllImportAttribute.
        /// </summary>
        /// <param name="libraryName">The name of the library to load.</param>
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
