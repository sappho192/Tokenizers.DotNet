# Tokenizers.DotNet

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

Tokenizers.DotNet is a .NET wrapper library for the HuggingFace Tokenizers Rust library. The project consists of two main components: a Rust native library (`rust/` folder) that wraps HuggingFace Tokenizers, and a .NET wrapper library (`dotnet/` folder) that provides C# bindings for the native library.

## Working Effectively

### Prerequisites and Setup
- Install Rust: `curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh && source ~/.cargo/env`
- Install .NET 9 SDK: `wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh && chmod +x /tmp/dotnet-install.sh && /tmp/dotnet-install.sh --channel 9.0`
- Install .NET 8 SDK (required for tests): `/tmp/dotnet-install.sh --channel 8.0`
- Add .NET to PATH: `export PATH="$HOME/.dotnet:$PATH"`
- Install PowerShell 7.4+: Available in most Linux distributions via package manager
- Install Docker (optional for cross-platform builds): Available via package manager

### Build Process
- Build Rust native library: `pwsh -File build_rust.ps1` -- takes 1-2 minutes. NEVER CANCEL. Set timeout to 5+ minutes.
- **CRITICAL Linux Fix**: After Rust build, manually copy the library: `cp rust/target/release/libhf_tokenizers.so nuget/linux-x64/ && cp rust/target/release/libhf_tokenizers.so dotnet/Tokenizers.DotNet.Test/`
- Build .NET library: `pwsh -File build_dotnet.ps1` -- takes 15-20 seconds. NEVER CANCEL. Set timeout to 2+ minutes.
- Build everything clean: `pwsh -File build_all_clean.ps1` -- takes 1-2 minutes total. NEVER CANCEL. Set timeout to 10+ minutes.
  - **Note**: After build_all_clean.ps1, always run the Linux library copy command above.

### Testing
- Run .NET tests: `pwsh -File test_dotnet.ps1` -- takes 5-10 seconds. NEVER CANCEL. Set timeout to 2+ minutes.
- **Expected Behavior**: Tests will FAIL in restricted network environments with `Name or service not known (huggingface.co:443)` errors
- **Note**: Test failures due to network restrictions are EXPECTED and do not indicate build problems
- **Note**: Tests require .NET 8+ runtime. Install with `/tmp/dotnet-install.sh --channel 8.0` if missing.

### Manual Validation
- Build console example: `cd dotnet/ConsoleExample && export PATH="$HOME/.dotnet:$PATH" && dotnet build --configuration Release`
- **Note**: The console example requires network access to download tokenizer files and will not work in restricted environments.

### Cross-Platform Docker Build
- For cross-platform builds supporting Windows x64/arm64 and Linux x64/arm64:
  ```bash
  docker build -f Dockerfile -t tokenizers-dotnet:latest .
  docker run -v ./nuget:/out --rm tokenizers-dotnet:latest
  ```
- **Note**: Docker build takes 45+ minutes. NEVER CANCEL. Set timeout to 90+ minutes.
- **Note**: Requires updating version first with `pwsh -File update_version.ps1`

## Project Structure

### Key Directories
- `rust/` - Native Rust library that wraps HuggingFace Tokenizers
  - `src/lib.rs` - Main Rust FFI interface
  - `Cargo.toml` - Rust project configuration
  - `build.rs` - Build script that generates C# bindings
- `dotnet/` - .NET wrapper and projects
  - `Tokenizers.DotNet/` - Main .NET library
  - `Tokenizers.DotNet.Test/` - Unit tests
  - `ConsoleExample/` - Example application
  - `Tokenizers.DotNet.sln` - Visual Studio solution
- `nuget/` - Output directory for NuGet packages organized by platform
- `.github/workflows/` - CI/CD pipelines for Windows and multi-platform builds

### Key Files
- `NATIVE_LIB_VERSION.txt` - Version number for the native library
- `build_all_clean.ps1` - Complete clean build script
- `build_rust.ps1` - Rust library build script  
- `build_dotnet.ps1` - .NET library build script
- `test_dotnet.ps1` - .NET test runner script
- `clear_*.ps1` - Various cleanup scripts
- `update_version.ps1` - Version update script

## Validation Requirements

### After Making Changes
- Always run `pwsh -File build_rust.ps1` to rebuild the native library (1-2 minutes)
- **CRITICAL on Linux**: Always run `cp rust/target/release/libhf_tokenizers.so nuget/linux-x64/ && cp rust/target/release/libhf_tokenizers.so dotnet/Tokenizers.DotNet.Test/` after Rust build
- Always run `pwsh -File build_dotnet.ps1` to rebuild the .NET library (15-20 seconds)
- Always run `pwsh -File test_dotnet.ps1` to run tests (5-10 seconds, EXPECTED to fail due to network restrictions in CI environments)

### Build Timing Expectations
- **CRITICAL**: NEVER CANCEL long-running builds. Build processes can take significant time.
- Rust compilation: 1-2 minutes for single platform
- .NET compilation: 15-20 seconds for all target frameworks (net6.0, net7.0, net8.0, net9.0)
- Docker cross-platform build: 45+ minutes for all platforms
- **Always use timeouts of 5+ minutes for Rust builds, 2+ minutes for .NET builds, 90+ minutes for Docker builds**

### Common Issues
- On Linux, build_rust.ps1 looks for .dll but creates .so files - manually copy the library with the command above
- Tests require internet access to HuggingFace Hub and may fail in CI/restricted environments  
- NuGet packaging requires `nuget` command line tool (optional for development, mainly used for publishing)
- Multiple .NET SDKs (6.0, 7.0, 8.0, 9.0) are required for multi-targeting
- Docker builds may fail due to SSL certificate issues in some environments

## Target Frameworks and Platforms

### .NET Target Frameworks
- net6.0, net7.0, net8.0, net9.0 (multi-targeting)

### Native Library Platforms
- Windows x64 (`win-x64`) - produces `hf_tokenizers.dll`
- Windows ARM64 (`win-arm64`) - produces `hf_tokenizers.dll` 
- Linux x64 (`linux-x64`) - produces `libhf_tokenizers.so`
- Linux ARM64 (`linux-arm64`) - produces `libhf_tokenizers.so`

### NuGet Package Structure
- `Tokenizers.DotNet` - Main managed library
- `Tokenizers.DotNet.runtime.win-x64` - Windows x64 native runtime
- `Tokenizers.DotNet.runtime.win-arm64` - Windows ARM64 native runtime  
- `Tokenizers.DotNet.runtime.linux-x64` - Linux x64 native runtime
- `Tokenizers.DotNet.runtime.linux-arm64` - Linux ARM64 native runtime

## Common Commands Reference

### Repository Structure
```
├── rust/                    # Native Rust library
├── dotnet/                  # .NET wrapper projects
│   ├── Tokenizers.DotNet/   # Main library
│   ├── Tokenizers.DotNet.Test/  # Unit tests
│   └── ConsoleExample/      # Example application
├── nuget/                   # Build output directory
├── .github/workflows/       # CI/CD pipelines
└── *.ps1                    # Build scripts
```

### Build Dependencies
- Rust 1.85+ with cargo
- .NET SDK 6.0, 7.0, 8.0, 9.0
- PowerShell 7.4+
- Docker (optional, for cross-platform)
- NuGet CLI (for packaging)

Always verify that both Rust and .NET components build successfully and that the native library is properly copied to the required locations before considering a build complete.