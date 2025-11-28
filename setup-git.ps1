# Git setup script for Nobat project
Set-Location "D:\Pro\Nobat"

# Initialize git repository
Write-Host "Initializing git repository..."
git init

# Add remote repository
Write-Host "Adding remote repository..."
git remote add origin https://github.com/hoseinshahbazi68/nobat.git

# Add all files
Write-Host "Adding all files..."
git add .

# Commit
Write-Host "Creating initial commit..."
git commit -m "Initial commit: Nobat project"

# Push to repository
Write-Host "Pushing to remote repository..."
git branch -M main
git push -u origin main

Write-Host "Done!"
