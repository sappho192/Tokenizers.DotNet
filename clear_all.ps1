Push-Location

# Remove every built nupkg and shared library
Get-ChildItem -Path "nuget" -Recurse -Include *.nupkg, *.dll, *.so, *.dylib -File -Force | ForEach-Object {
    try {
        Remove-Item $_.FullName -Force
        Write-Host "Removed file: $($_.FullName)"
    } catch {
        Write-Host "Failed to remove file: $($_.FullName) - $($_.Exception.Message)"
    }
}
Pop-Location

# Remove dotnet cache
Push-Location
Set-Location -Path ".\dotnet\Tokenizers.DotNet"
if (Test-Path -Path ".\bin") {
    Remove-Item "bin" -Force -Recurse
}
if (Test-Path -Path ".\obj") {
    Remove-Item "obj" -Force -Recurse
}
Pop-Location
Push-Location
Set-Location -Path ".\dotnet\ConsoleExample"
if (Test-Path -Path ".\bin") {
    Remove-Item "bin" -Force -Recurse
}
if (Test-Path -Path ".\obj") {
    Remove-Item "obj" -Force -Recurse
}
Pop-Location

# Remove Docker bulid artifacts
if (Test-Path -Path ".\out") {
    Remove-Item "out" -Force -Recurse
}
