# LegacyWrapper Changelog

## Version 3.0 [tbd]

**LegacyWrapper 3.0 is a major rewrite and not backwards compatible to prior versions.**

*   The old `Invoke<TDelegate>()` method has been replaced by a much simpler and more type-safe pattern:

```csharp
// Define a proxy interface with matching method names and signatures
// The interface must be derived from IDisposable!
[LegacyDllImport("User32.dll")]
public interface IUser32Dll : IDisposable
{
    [LegacyDllMethod(CallingConvention = CallingConvention.Winapi)]
    int GetSystemMetrics(int nIndex);
}

// Create configuration
IWrapperConfig configuration = WrapperConfigBuilder.Create()
        .TargetArchitecture(TargetArchitecture.X86)
        .Build();

// Create new Wrapper client providing the proxy interface
// Remember to ensure a call to the Dispose()-Method!
using (var client = WrapperProxyFactory<IUser32Dll>.GetInstance(configuration))
{
    // Make calls - it's that simple!
    int x = client.GetSystemMetrics(0);
    int y = client.GetSystemMetrics(1);
}
```

Therefore, there is _no longer_ the need to:
* Create a `delegate`
* Cast result values
* Pass function parameters as array of `object`
* Supply the method name as magic string

Other things that changed:

*   Documentation on classes and methods has been improved
*   There are more unit tests now (especially edge cases)

## Version 2.1 [20. Aug 2017]

* It is now possible to load 64bit libraries in a 32bit process:

```csharp
using (var client = new WrapperClient(TestDllPath, TargetArchitecture.Amd64))
{
    result = (int)client.Invoke<TestStdCallDelegate>("TestStdCall", new object[] { input });
}
```
* Please note that this will only work if the OS is 64 bit.
* As the second optional parameter defaults to X86, this release should be backwards compatible.

## Version 2.0.1 [17. Feb 2017]

* This is a minor release that updates some XML docs.

## Version 2.0 [17. Feb 2017]

* This version 2.0 of LegacyWrapper adds a major performance improvement. The wrapper executable now loads the requested dll only once until disposed.
* LegacyWrapper 2.0 is not backwards compatible.

## Version 1.1.0 [17. Feb 2017]

* Add support for ref parameters (values in passed parameters array will be updated)
* Improve error handling (wrapper exe will not crash randomly any more)

## Version 1.0.1 [1. Oct 2015]

* NuGet package dll is now compiled with AnyCPU (resulted in a BadImageException)
* Add batch file for creating a NuGet package

## Version 1.0 [1. Oct 2015]

* Initial Release