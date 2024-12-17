Push-Location

$arch = switch ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture) {
    "X64"  { "x64" }
    "X86"  { "x86" }
    "Arm"  { "arm" }
    "Arm64" { "arm64" }
    Default { "unknown" }
}

Set-Location -Path ".\nuget\win-$arch"
if (Test-Path -Path "Tokenizers.DotNet.runtime.win.*.nupkg") {
    Remove-Item "Tokenizers.DotNet.runtime.win.*.nupkg" -Force
}
if (Test-Path -Path "hf_tokenizers.dll") {
    Remove-Item "hf_tokenizers.dll" -Force
}

Set-Location -Path ".."
if (Test-Path -Path "Tokenizers.DotNet.*.nupkg") {
    Remove-Item "Tokenizers.DotNet.*.nupkg" -Force
}

if (Test-Path -Path "net*") {
    Remove-Item "net*" -Force
}
Pop-Location

Push-Location
Set-Location -Path ".\dotnet\Tokenizers.DotNet"
if (Test-Path -Path ".\bin") {
    Remove-Item "bin" -Force -Recurse
}
if (Test-Path -Path ".\obj") {
    Remove-Item "obj" -Force -Recurse
}
Pop-Location