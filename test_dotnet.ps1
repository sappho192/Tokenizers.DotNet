Push-Location

Set-Location -Path "dotnet/Tokenizers.DotNet.Test"
dotnet test --configuration Release --verbosity normal
if ($LASTEXITCODE -ne 0) {
    Pop-Location
    exit $LASTEXITCODE
}

Pop-Location