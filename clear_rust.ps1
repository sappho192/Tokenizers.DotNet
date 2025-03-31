Push-Location
Set-Location -Path ".\rust"
if (Test-Path -Path ".\debug") {
    Remove-Item "debug" -Force -Recurse
}
if (Test-Path -Path ".\target") {
    Remove-Item "target" -Force -Recurse
}
if (Test-Path -Path ".\Cargo.lock") {
    Remove-Item "Cargo.lock"
}
Pop-Location
