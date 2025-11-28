# Build script for Nobat API
Write-Host "Building Nobat API..." -ForegroundColor Green

# Navigate to API project directory
Set-Location src/Nobat.API

# Build the project
dotnet build

# Check if build was successful
if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild completed successfully!" -ForegroundColor Green
} else {
    Write-Host "`nBuild failed with errors!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Return to original directory
Set-Location ..\..
