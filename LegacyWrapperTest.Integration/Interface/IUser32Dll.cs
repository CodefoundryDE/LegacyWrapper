using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Attributes;

namespace LegacyWrapperTest.Integration.Interface
{
    [LegacyDllImport("User32.dll")]
    public interface IUser32Dll : IDisposable
    {
        [LegacyDllMethod(CallingConvention = CallingConvention.Winapi)]
        int GetSystemMetrics(int nIndex);
    }
}
