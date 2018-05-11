using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using LegacyWrapper.Common.Serialization;
using LegacyWrapper.ErrorHandling;
using PommaLabs.Thrower;

namespace LegacyWrapper.Common.Interop
{
    internal class UnmanagedLibraryLoader
    {
        private const string AssemblyName = "LegacyWrapper";
        private const string ModuleName = "LegacyWrapper";
        private const string TypeName = "LegacyWrapper.WrapperType";

        public CallResult InvokeUnmanagedFunction(CallData callData)
        {
            Type dllHandle = CreateTypeBuilder(callData);
            MethodInfo methodInfo = dllHandle.GetMethod(callData.ProcedureName);

            Raise<LegacyWrapperException>.If(methodInfo == null, $"Requested method {callData.ProcedureName} was not found in unmanaged DLL.");

            object result = methodInfo.Invoke(null, callData.Parameters);

            return new CallResult()
            {
                Result = result,
                Parameters = callData.Parameters
            };
        }

        private Type CreateTypeBuilder(CallData callData)
        {
            AssemblyName asmName = new AssemblyName(AssemblyName);
            AssemblyBuilder asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule(ModuleName, emitSymbolInfo: false);
            TypeBuilder typeBuilder = modBuilder.DefineType(TypeName, TypeAttributes.Class | TypeAttributes.Public);

            MethodBuilder pinvokeBuilder = typeBuilder.DefinePInvokeMethod(
                name: callData.ProcedureName,
                dllName: callData.LibraryName,
                attributes: MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.PinvokeImpl,
                callingConvention: CallingConventions.Standard,
                returnType: callData.ReturnType,
                parameterTypes: callData.ParameterTypes,
                nativeCallConv: callData.CallingConvention,
                nativeCharSet: callData.CharSet);

            pinvokeBuilder.SetImplementationFlags(pinvokeBuilder.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);

            return typeBuilder.CreateType();
        }
    }
}
