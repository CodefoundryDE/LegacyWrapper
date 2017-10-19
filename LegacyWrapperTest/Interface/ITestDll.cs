using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Attributes;

namespace LegacyWrapperTest.Interface
{
    // We assume the following dll functions in our Test.dll here: 
    // Every function is supposed to return the input parameter unchanged.
    //
    // function TestStdCall(const AParam: Integer): Integer; stdcall;
    // function TestNormalFunc(const AParam: Integer): Integer;
    // function TestPCharHandling(const AParam: PChar): PChar; stdcall;
    // function TestPWideCharHandling(const AParam: PWideChar): PWideChar; stdcall;
    //
    // These procedures are expected to increment their parameters by 1:
    //
    // procedure TestVarParamHandling(var AParam: Integer); stdcall;      
    // procedure TestMultipleVarParamsHandling(var AParam1: Integer; var AParam2: Integer); stdcall;

    public interface ITestDll
    {
        [LegacyDllImport("TestDll.dll", CallingConvention = CallingConvention.StdCall)]
        int TestStdCall(int AParam);

        [LegacyDllImport("TestDll.dll", CallingConvention = CallingConvention.StdCall)]
        int TestNormalFunc(int AParam);

        [LegacyDllImport("TestDll.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        string TestPCharHandling(string AParam);

        [LegacyDllImport("TestDll.dll", CallingConvention = CallingConvention.StdCall)]
        string TestPWideCharHandling(string AParam);

        // These procedures are expected to increment their parameters by 1:

        [LegacyDllImport("TestDll.dll", CallingConvention = CallingConvention.StdCall)]
        void TestVarParamHandling(ref int AParam);

        [LegacyDllImport("TestDll.dll", CallingConvention = CallingConvention.StdCall)]
        void TestMultipleVarParamsHandling(ref int AParam1, ref int AParam2);

        /// <summary>
        /// This Method exists to test how the WrapperClient handles a non existent library.
        /// </summary>
        [LegacyDllImport("TestNonExistingLibrary.dll", CallingConvention = CallingConvention.StdCall)]
        void TestNonExistingLibrary();

        /// <summary>
        /// This Method exists to test how the WrapperClient handles a non existent function.
        /// </summary>
        [LegacyDllImport("TestDll.dll", CallingConvention = CallingConvention.StdCall)]
        void TestNonExistingFunction();
    }
}
