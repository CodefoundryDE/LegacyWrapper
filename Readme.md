# LegacyWrapper

## About

LegacyWrapper uses a wrapper process to call dlls from a process of the opposing architecture (X86 or AMD64).

Since you can't load a dll of another architecture directly, the wrapper utilizes a named pipe to abstract the call. You won't notice this though, because all the magic is hidden behind a single static method.

## NuGet Package

There is a NuGet package available here: [Codefoundry.LegacyWrapper @ nuget.org](https://www.nuget.org/packages/Codefoundry.LegacyWrapper/)

## Usage

If you want to compile the LegacyWrapper yourself, make sure to place both the wrapper executable, LegacyWrapperClient.dll and LegacyWrapper.Common.dll in your directory.

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

Please note that loading a 64bit dll will only work on 64bit operating systems.

## Further reading

View [this blog post](https://codefoundry.de/programming/2015/09/28/legacy-wrapper-invoking-an-unmanaged-32bit-library-out-of-a-64bit-process.html) to obtain a basic understanding of how the library works internally. 

* [There is also a blog post about the dynamic method call feature in LegacyWrapper 3.0](https://codefoundry.de/programming/2019/02/03/legacywrapper-3-0-released.html).
* [There is also a blog post about the new 64bit feature in LegacyWrapper 2.1](https://codefoundry.de/programming/2017/08/20/legacywrapper-2-1-is-out.html).

## Contributing

Feel free to submit any [suggestions/issues](https://github.com/CodefoundryDE/LegacyWrapper/issues) and contribute to LegacyWrapper.

## License

Copyright (c) 2019, Franz Wimmer. (MIT License)

See LICENSE for more info.
