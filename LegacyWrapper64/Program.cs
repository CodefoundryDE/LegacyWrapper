using System.Diagnostics;
using System.Runtime.ExceptionServices;
using LegacyWrapper.Handler;
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
            //Debugger.Launch();
            ICallRequestHandler requestHandler = CallRequestHandlerFactory.GetInstance(args);
            requestHandler.Call();
        }
    }
}
