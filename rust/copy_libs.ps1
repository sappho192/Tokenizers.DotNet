$sourceFiles = @(
    "target/debug/hf_tokenizers.dll",
    "target/debug/hf_tokenizers.dll.exp",
    "target/debug/hf_tokenizers.dll.lib"
)
$destinationPath = "../dotnet/Tokenizers.DotNet/deps/"

if (-not (Test-Path -Path $destinationPath)) {
    New-Item -Path $destinationPath -ItemType Directory
    Write-Output "Directory created successfully!"
}
else {
    Write-Output "Directory already exists!"
}

# Copy each file to the destination, overwriting existing files
foreach ($file in $sourceFiles) {
    Copy-Item -Path $file -Destination $destinationPath -Force
}