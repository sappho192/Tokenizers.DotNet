Push-Location

# Step 0: Read the version from VERSION.txt
$version = Get-Content -Path ".\NATIVE_LIB_VERSION.txt"
Write-Output "Version: $version"

# Step 1: Replace the version in Cargo.toml
$cargoTomlPath = ".\rust\Cargo.toml"
$cargoContent = Get-Content -Path $cargoTomlPath
$cargoContent = $cargoContent -replace '(?<=^version\s*=\s*")[^"]*', $version
Set-Content -Path $cargoTomlPath -Value $cargoContent
Write-Output "Updated version in Cargo.toml"

# Step 2: Replace the version in.nuspec file
$archList = @("x64", "arm64")
foreach ($arch in $archList) {
    $nuspecFilePath = ".\nuget\win-$arch\Tokenizers.DotNet.runtime.win-$arch.nuspec"
    $nuspecContent = Get-Content -Path $nuspecFilePath
    $nuspecContent = $nuspecContent -replace '(?<=<version>)[^<]*', $version
    Set-Content -Path $nuspecFilePath -Value $nuspecContent
    Write-Output "Updated version in nuspec file for architecture: $arch"
}

Pop-Location
