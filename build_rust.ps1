Push-Location

# Build the library for the current platform
Set-Location -Path "rust"
cargo build --release
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    exit $LASTEXITCODE
}

$sourceFile = "target/release/hf_tokenizers.dll"

$arch = switch ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture) {
    "X64"  { "x64" }
    "X86"  { "x86" }
    "Arm"  { "arm" }
    "Arm64" { "arm64" }
    Default { "unknown" }
}

$os = switch -Wildcard ([System.Runtime.InteropServices.RuntimeInformation]::OSDescription) {
    "*Windows*" { "win"; break }
    "*Linux*"   { "linux"; break }
    Default    { "unknown" }
}

$destinationPath = "../nuget/$os-$arch/";

Copy-Item -Path $sourceFile -Destination $destinationPath -Force

$destinationPath = "../dotnet/Tokenizers.DotNet.Test/"

Copy-Item -Path $sourceFile -Destination $destinationPath -Force

Pop-Location