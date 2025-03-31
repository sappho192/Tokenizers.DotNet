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

# Step 2: Replace the version in every .nuspec file
$nuspecFiles = Get-ChildItem -Path ".\nuget" -Recurse -Filter "*.nuspec"

foreach ($file in $nuspecFiles) {
    $nuspecFilePath = $file.FullName;
    $nuspecContent = Get-Content -Path $nuspecFilePath
    $nuspecContent = $nuspecContent -replace '(?<=<version>)[^<]*', $version
    Set-Content -Path $nuspecFilePath -Value $nuspecContent
    
    Write-Output "Updated version in $nuspecFilePath"
}
