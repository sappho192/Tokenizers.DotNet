Push-Location
Set-Location -Path ".\nuget"
if (Test-Path -Path "Tokenizers.DotNet.*") {
    Remove-Item "Tokenizers.DotNet.*" -Force
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
