Push-Location
# Step 1: Read the version from VERSION.txt
$version = Get-Content -Path ".\NATIVE_LIB_VERSION.txt"
Write-Output "Version: $version"

# Step 3: Replace the version in.nuspec file
$nuspecFilePath = ".\nuget\Tokenizers.DotNet.nuspec"
$nuspecContent = Get-Content -Path $nuspecFilePath
$nuspecContent = $nuspecContent -replace '(?<=<version>)[^<]*', $version
Set-Content -Path $nuspecFilePath -Value $nuspecContent
Write-Output "Updated version in nuspec file"

Pop-Location
Push-Location

Set-Location -Path ".\dotnet\Tokenizers.DotNet"
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    exit $LASTEXITCODE
}
# Copy-Item "bin\Release\net*" "..\..\nuget" -Force
Pop-Location

Push-Location
Set-Location -Path "nuget"
nuget pack Tokenizers.DotNet.nuspec
Pop-Location
