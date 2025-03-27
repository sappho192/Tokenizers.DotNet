$archList = @("x64", "arm64")

foreach ($arch in $archList) {
    $sourceFile = "out/$arch/hf_tokenizers.dll"
    $destinationPath = "nuget/win-$arch/"
    
    if (-not (Test-Path -Path $destinationPath)) {
        New-Item -Path $destinationPath -ItemType Directory
        Write-Output "Directory created successfully!"
    }
    else {
        Write-Output "Directory already exists!"
    }
    
    Copy-Item -Path $sourceFile -Destination $destinationPath -Force
    Write-Output "Copied $sourceFile to $destinationPath"
    
    Set-Location -Path "nuget\win-$arch"
    nuget pack Tokenizers.DotNet.runtime.win-$arch.nuspec
    Set-Location -Path "..\..\"
}