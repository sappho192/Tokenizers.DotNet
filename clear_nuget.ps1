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
