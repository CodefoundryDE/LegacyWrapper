# LegacyWrapper

## About

LegacyWrapper uses a x86 wrapper to call legacy dlls from a 64bit processes.

Since you can't load a 32bit dll into a 64bit process, this wrapper utilizes a named pipe to abstract the call. You won't notice this though, because all the magic is hidden behind a single static method.

## Usage

Make sure to place both the wrapper executable and the LegacyWrapperClient.dll in your directory.

```csharp
// Define delegate matching api function
private delegate int GetSystemMetrics(int index);

// Create new WrapperClient
// Remember to ensure a call to the Dispose()-Method!
using (var client = new WrapperClient())
{
    // Make calls providing library name, function name, and parameters
    int x = (int)client.Call<GetSystemMetrics>("User32.dll", "GetSystemMetrics", new object[] { 0 });
    int y = (int)client.Call<GetSystemMetrics>("User32.dll", "GetSystemMetrics", new object[] { 1 });
}
```

## Todo

* Support for Attributes like `[CallingConvention]`.
* Type safe usage of generics in Call<T>-Method

## Contributing

Feel free to submit any [suggestions/issues](https://github.com/CodefoundryDE/LegacyWrapper/issues) and contribute to LegacyWrapper.

## License

Copyright (c) 2015, Franz Wimmer. (MIT License)

See LICENSE for more info.

## Credits

This library includes [Nuane.Interop](https://github.com/lukaaash/Nuane.Interop) written by Lukas Pokorny.
