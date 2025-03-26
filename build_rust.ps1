Push-Location

# Step 0: Get the host's cpu architecture
$arch = switch ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture) {
    "X64"  { "x64" }
    "X86"  { "x86" }
    "Arm"  { "arm" }
    "Arm64" { "arm64" }
    Default { "unknown" }
}

# Step 1: Build the library
Set-Location -Path "rust"
cargo build --release
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    exit $LASTEXITCODE
}

# Step 2: Copy the library
&.\copy_libs.ps1
Set-Location -Path ".."
Set-Location -Path "nuget\win-$arch"
nuget pack Tokenizers.DotNet.runtime.win-$arch.nuspec
Pop-Location
