# Remove Docker bulid artifacts
if (Test-Path -Path ".\out") {
    Remove-Item "out" -Force -Recurse
}
