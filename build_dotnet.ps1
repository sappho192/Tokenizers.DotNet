Push-Location

# Step 0: Build
Set-Location -Path "dotnet/Tokenizers.DotNet"
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    exit $LASTEXITCODE
}

Pop-Location

# Step 1: Pack every nuspec
$baseNugetFolder = Resolve-Path "nuget"

# Find all .nuspec files recursively
$nuspecFiles = Get-ChildItem -Path $baseNugetFolder -Recurse -Filter "*.nuspec" -File

foreach ($file in $nuspecFiles) {
    # Check if nuspec is in the nuget folder or not
    # If true, `nuget pack` will always be executed
    # If false, it will be checked if a shared library has been built for that platform
    # If that is the case it will be packed
    
    $parentFolder = Split-Path -Path $file.FullName -Parent
    $isRoot = ($parentFolder -eq $baseNugetFolder.Path)
    $shouldPack = $false

    if ($isRoot) {
        $shouldPack = $true
    } else {
        # Check for .so, .dll, or .dylib
        $libFiles = Get-ChildItem -Path $parentFolder -Recurse -Include *.so, *.dll, *.dylib -File
        if ($libFiles) {
            Write-Host "Packing $parentFolder"
            $shouldPack = $true
        } else {
            Write-Host "Skipping nuget pack of $parentFolder because of missing libs"
        }
    }

    if ($shouldPack) {
        Push-Location $parentFolder
        try {
            # Execute nuget pack command on the nuspec file name only
            nuget pack $file.Name
            Write-Host "Successfully packed: $($file.Name)"
        }
        catch {
            Write-Host "Error processing $($file.FullName): $($_.Exception.Message)"
        }
        finally {
            # Return to the previous directory
            Pop-Location
        }
    }
}