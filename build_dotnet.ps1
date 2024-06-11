Push-Location

Set-Location -Path ".\dotnet\Tokenizers.DotNet"
dotnet build --configuration Release -o "../../nuget"

Pop-Location

