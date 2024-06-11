Push-Location

Set-Location -Path ".\dotnet\Tokenizers.DotNet"
dotnet build --configuration Release
Copy-Item "bin\Release\Tokenizers.DotNet.*.nupkg" "..\..\nuget"
Pop-Location

