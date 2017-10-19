using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Attributes;

namespace LegacyWrapperTest.Interface
{
    public interface IUser32Dll : IDisposable
    {
        [LegacyDllImport("User32.dll", CallingConvention = CallingConvention.Winapi)]
        int GetSystemMetrics(int nIndex);
    }
}
