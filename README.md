- [Tokenizers.DotNet](#tokenizersdotnet)
- [Nuget Package list](#nuget-package-list)
- [Requirements](#requirements)
- [Supported functionalities](#supported-functionalities)
- [How to use](#how-to-use)
  - [(1) Install the packages](#1-install-the-packages)
  - [(2) Write the code](#2-write-the-code)
- [How to build](#how-to-build)

# Tokenizers.DotNet

.NET wrapper of HuggingFace Tokenizers library

# Nuget Package list

| Package                       | main                                                                                                              | Description                     |
| ----------------------------- | ----------------------------------------------------------------------------------------------------------------- | ------------------------------- |
| Tokenizers.DotNet             | [![Nuget Tokenizers.DotNet](https://img.shields.io/nuget/v/Tokenizers.DotNet.svg?style=flat)](https://www.nuget.org/packages/Tokenizers.DotNet/)                         | Core library                    |
| Tokenizers.DotNet.runtime.win | [![Nuget Tokenizers.DotNet.runtime.win](https://img.shields.io/nuget/v/Tokenizers.DotNet.runtime.win.svg?style=flat)](https://www.nuget.org/packages/Tokenizers.DotNet.runtime.win/) | Native bindings for windows x64 |

# Requirements

- .NET 6 or above

# Supported functionalities

* [X] Download tokenizer files from Hugginface Hub
* [X] Load tokenizer file(`.json`) from local
* [X] Decode embeddings to string

# How to use

## (1) Install the packages

1. From the NuGet, install `Tokenizers.DotNet` package
2. And then, install `Tokenizers.DotNet.runtime.win` package too

## (2) Write the code

Check following example code:

```CSharp
using Tokenizers.DotNet;

var hubName = "skt/kogpt2-base-v2";
var filePath = "tokenizer.json";
var fileFullPath = await HuggingFace.GetFileFromHub(hubName, filePath, "deps");
Console.WriteLine($"Downloaded {fileFullPath}");

// Write the path of tokenizer.json to tokenizer.path.txt
var tokenizerPath = "tokenizer.path.txt";
await File.WriteAllTextAsync(tokenizerPath, fileFullPath);
Console.WriteLine($"Wrote {fileFullPath} to {tokenizerPath}");

// Create a tokenizer instance
var tokenizer = new Tokenizer();
var tokens = new uint[] { 9330, 387, 12857, 9376, 18649, 9098, 7656, 6969, 8084, 1 };
var decoded = tokenizer.Decode(tokens);
Console.WriteLine($"Decoded: {decoded}");
```

# How to build

1. Prepare following stuff:
   1.  Rust build system (`cargo`)
   2.  .NET build system (`dotnet 6.0`)
   3.  PowerShell (Recommend `7.4.2` or above)
2. Run `build_all_clean.ps1`
   1. To build `Tokenizers.DotNet.runtime.win` only, run `build_rust.ps1`
   2. To build `Tokenizers.DotNet` only, run `build_dotnet.ps1`

Each build artifacts will be in `nuget` directory.  
