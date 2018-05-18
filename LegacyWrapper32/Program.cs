using System.ComponentModel;
using System.Runtime.ExceptionServices;
using LegacyWrapper.Common.Wrapper;
using PommaLabs.Thrower;

namespace LegacyWrapper32
{
    public class Program
    {
        /// <summary>
        /// Main method of the legacy dll wrapper.
        /// </summary>
        /// <param name="args">
        /// The first parameter is expected to be a string.
        /// The Wrapper will use this string to create a named pipe.
        /// </param>
        static void Main(string[] args)
        {
            WrapperHelper wrapperHelper = new WrapperHelper(args);
            wrapperHelper.Call();
        }   
    }
}
