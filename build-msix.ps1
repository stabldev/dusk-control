# This script builds the MSIX installer for the Dusk Control application.
# The resulting .msix package can be found in the AppPackages directory.

Write-Host "Building MSIX package for Dusk Control..." -ForegroundColor Cyan
dotnet build -c Release -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=false -p:WindowsPackageType=MSIX
Write-Host "Build complete! Check the AppPackages directory for your .msix file." -ForegroundColor Green
