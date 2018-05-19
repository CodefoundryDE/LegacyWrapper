using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegacyWrapper.Handler
{
    public interface ICallRequestHandler
    {
        /// <summary>
        /// Outsourced main method of the legacy dll wrapper.
        /// </summary>
        void Call();
    }
}
