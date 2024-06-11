$sourceFiles = @(
    "target/debug/hf_tokenizers.dll"
)

if ($IsWindows) {
    $destinationPath = "../nuget/win-x64/"
}
else {
    Write-Output "Unsupported OS"
    exit
}


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
    Write-Output "Copied $file to $destinationPath"
}