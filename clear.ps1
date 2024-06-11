Push-Location

Set-Location -Path ".\nuget\win-x64"
Remove-Item "hf_tokenizers.dll" -Force
Remove-Item "Tokenizers.DotNet.runtime.win.*.nupkg" -Force

Set-Location -Path ".."
Remove-Item "Tokenizers.DotNet.*" -Force

Pop-Location