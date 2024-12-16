Push-Location

# Step 0: Get the host's cpu architecture
$arch = switch ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture) {
    "X64"  { "x64" }
    "X86"  { "x86" }
    "Arm"  { "arm" }
    "Arm64" { "arm64" }
    Default { "unknown" }
}

# Step 1: Read the version from VERSION.txt
$version = Get-Content -Path ".\NATIVE_LIB_VERSION.txt"
Write-Output "Version: $version"

# Step 2: Replace the version in Cargo.toml
$cargoTomlPath = ".\rust\Cargo.toml"
$cargoContent = Get-Content -Path $cargoTomlPath
$cargoContent = $cargoContent -replace '(?<=^version\s*=\s*")[^"]*', $version
Set-Content -Path $cargoTomlPath -Value $cargoContent
Write-Output "Updated version in Cargo.toml"

# Step 3: Replace the version in.nuspec file
$nuspecFilePath = ".\nuget\win-$arch\Tokenizers.DotNet.runtime.win.nuspec"
$nuspecContent = Get-Content -Path $nuspecFilePath
$nuspecContent = $nuspecContent -replace '(?<=<version>)[^<]*', $version
Set-Content -Path $nuspecFilePath -Value $nuspecContent
Write-Output "Updated version in nuspec file"

# Step 4: Build the library
Set-Location -Path "rust"
cargo build --release
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    exit $LASTEXITCODE
}

&.\copy_libs.ps1
Set-Location -Path ".."
Set-Location -Path "nuget\win-$arch"
nuget pack Tokenizers.DotNet.runtime.win.nuspec
Pop-Location
