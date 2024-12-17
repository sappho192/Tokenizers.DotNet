$sourceFiles = @(
    "target/release/hf_tokenizers.dll"
)

$arch = switch ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture) {
    "X64"  { "x64" }
    "X86"  { "x86" }
    "Arm"  { "arm" }
    "Arm64" { "arm64" }
    Default { "unknown" }
}

if ($IsWindows -or $ENV:OS) {
    $destinationPath = "../nuget/win-$arch/"
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