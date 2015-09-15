# LegacyWrapper

## About

LegacyWrapper uses a x86 wrapper to call legacy dlls from 64 bit processes.

## Usage

Make sure to place both the wrapper executable and the LegacyWrapperClient.dll in your directory.

```csharp
// Define delegate matching api function
private delegate int GetSystemMetrics(int index);

// Make call providing library name, function name, and parameters
int x = (int)WrapperClient.Call<GetSystemMetrics>("User32.dll", "GetSystemMetrics", new object[] { 0 });
int y = (int)WrapperClient.Call<GetSystemMetrics>("User32.dll", "GetSystemMetrics", new object[] { 1 });
```

## License

Copyright (c) 2015, Franz Wimmer. (MIT License)

See LICENSE for more info.

## Credits

This library includes [Nuane.Interop](https://github.com/lukaaash/Nuane.Interop) written by Lukas Pokorny.