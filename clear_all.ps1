Push-Location

Set-Location -Path ".\nuget\win-x64"
if (Test-Path -Path "Tokenizers.DotNet.runtime.win.*.nupkg") {
    Remove-Item "Tokenizers.DotNet.runtime.win.*.nupkg" -Force
}
if (Test-Path -Path "hf_tokenizers.dll") {
    Remove-Item "hf_tokenizers.dll" -Force
}

Set-Location -Path ".."
if (Test-Path -Path "Tokenizers.DotNet.*") {
    Remove-Item "Tokenizers.DotNet.*" -Force
}

Pop-Location