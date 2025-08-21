# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Tokenizers.DotNet is a .NET wrapper library for the HuggingFace Tokenizers Rust library. It provides C# bindings for tokenizing and detokenizing text using HuggingFace models.

## Architecture

The project consists of two main components:

1. **Rust Native Library** (`rust/` directory):
   - `src/lib.rs` - Rust FFI interface wrapping HuggingFace Tokenizers
   - Uses `tokenizers` crate (0.21.1) with onig and progressbar features
   - Session-based tokenizer management with UUID identifiers
   - Provides C-compatible functions for tokenizer operations

2. **.NET Wrapper** (`dotnet/` directory):
   - `Tokenizers.DotNet/` - Main managed library with unsafe C# bindings
   - `Tokenizers.DotNet.Test/` - Unit tests
   - `ConsoleExample/` - Example application
   - Uses `csbindgen` for auto-generating P/Invoke bindings from Rust

The Rust library generates C# bindings automatically via `csbindgen` build script, creating `NativeMethods.cs` with P/Invoke declarations.

## Key Classes and Components

- **`Tokenizer`** (C#): Main wrapper class with session-based tokenizer management
- **`HuggingFace`**: Static utility class for downloading tokenizer files from HuggingFace Hub
- **`TokenizerException`**: Custom exception for native library errors
- **`ByteBuffer`** (Rust): Memory management structure for data transfer between native/managed code
- **Session Management**: Each tokenizer instance gets a UUID session ID for native library operations

## Build Commands

### Prerequisites Setup
```bash
# Install Rust
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh && source ~/.cargo/env

# Install .NET SDKs (requires 6.0, 7.0, 8.0, 9.0 for multi-targeting)
wget https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh && chmod +x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 9.0
/tmp/dotnet-install.sh --channel 8.0
export PATH="$HOME/.dotnet:$PATH"
```

### Build Process
```bash
# Build Rust native library (1-2 minutes)
pwsh -File build_rust.ps1

# CRITICAL Linux Fix: Manually copy library after Rust build
cp rust/target/release/libhf_tokenizers.so nuget/linux-x64/
cp rust/target/release/libhf_tokenizers.so dotnet/Tokenizers.DotNet.Test/

# Build .NET library (15-20 seconds)
pwsh -File build_dotnet.ps1

# Build everything clean (1-2 minutes total)
pwsh -File build_all_clean.ps1
# Always run Linux library copy after build_all_clean.ps1
```

### Testing
```bash
# Run .NET tests (5-10 seconds) - EXPECTED to fail in restricted network environments
pwsh -File test_dotnet.ps1

# Manual validation - build console example
cd dotnet/ConsoleExample
dotnet build --configuration Release
```

### Cross-Platform Docker Build
```bash
# Update version first
pwsh -File update_version.ps1

# Build for all platforms (45+ minutes)
docker build -f Dockerfile -t tokenizers-dotnet:latest .
docker run -v ./nuget:/out --rm tokenizers-dotnet:latest
```

## Version Management

- Version is stored in `NATIVE_LIB_VERSION.txt`
- Rust library version in `rust/Cargo.toml` should match
- Update version with `update_version.ps1` before releases

## Target Platforms

### .NET Framework Targets
- net6.0, net7.0, net8.0, net9.0 (multi-targeting)

### Native Library Platforms
- Windows x64/ARM64 - produces `hf_tokenizers.dll`
- Linux x64/ARM64 - produces `libhf_tokenizers.so`

### NuGet Package Structure
- `Tokenizers.DotNet` - Main managed library
- `Tokenizers.DotNet.runtime.{platform}` - Platform-specific native runtimes

## Critical Build Notes

- **NEVER CANCEL** long-running builds - use proper timeouts (5+ minutes for Rust, 90+ minutes for Docker)
- On Linux, always manually copy `libhf_tokenizers.so` after Rust builds
- Tests require internet access and may fail in CI/restricted environments
- Build system uses PowerShell scripts - requires PowerShell 7.4+
- Clean builds remove all artifacts - use `clear_*.ps1` scripts for selective cleanup

## Development Workflow

1. Make changes to Rust or .NET code
2. Run `pwsh -File build_rust.ps1` (always required after Rust changes)
3. On Linux: Copy library with `cp` command above
4. Run `pwsh -File build_dotnet.ps1`
5. Run `pwsh -File test_dotnet.ps1` (expect network failures in restricted environments)
6. Test manually with ConsoleExample if needed