# This script builds the MSIX installer for the Dusk Control application.
# The resulting .msix package can be found in the AppPackages directory.

Write-Host "Building MSIX package for Dusk Control..." -ForegroundColor Cyan
dotnet publish DuskControl.csproj -c Release -p:WindowsPackageType=MSIX -p:AppxPackageSigningEnabled=false -p:UapAppxPackageBuildMode=SideloadOnly -p:GenerateAppxPackageOnBuild=true
Write-Host "Build complete! Check the AppPackages directory for your .msix file." -ForegroundColor Green
