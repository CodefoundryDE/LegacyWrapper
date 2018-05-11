using System.Runtime.ExceptionServices;
using LegacyWrapper.Common.Wrapper;
using PommaLabs.Thrower;

namespace LegacyWrapper64
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
            string errorMessage = "The number of arguments passed to this executable has to be exactly 1.";
            Raise.ArgumentNullException.IfIsNull(args, nameof(args), errorMessage);
            Raise.ArgumentException.IfNot(args.Length == 1, nameof(args), errorMessage);

            WrapperHelper wrapperHelper = new WrapperHelper();
            wrapperHelper.Call(args[0]);
        }   
    }
}
