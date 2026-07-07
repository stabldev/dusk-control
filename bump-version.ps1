param (
  [Parameter(Mandatory = $true)]
  [string]$NewVersion
)

# Ensure the new version is valid
if ($NewVersion -notmatch '^\d+\.\d+\.\d+$') {
  Write-Host "Error: Version must be in the format X.Y.Z (e.g., 1.0.2)" -ForegroundColor Red
  exit 1
}

$newVersionWithZeros = "$NewVersion.0"

Write-Host "Bumping version to $NewVersion..." -ForegroundColor Cyan

# 1. Update config.ts (format: X.Y.Z)
$configPath = "LandingPage\src\config.ts"
if (Test-Path $configPath) {
  (Get-Content $configPath) -replace 'export const VERSION = ".*";', "export const VERSION = `"$NewVersion`";" | Set-Content $configPath
  Write-Host "Updated $configPath" -ForegroundColor Green
}
else {
  Write-Host "Warning: $configPath not found!" -ForegroundColor Yellow
}

# 2. Update Package.appxmanifest (format: X.Y.Z.0)
$appxManifestPath = "Package.appxmanifest"
if (Test-Path $appxManifestPath) {
  (Get-Content $appxManifestPath) -replace '^\s*Version="[0-9\.]+"', "    Version=`"$newVersionWithZeros`"" | Set-Content $appxManifestPath
  Write-Host "Updated $appxManifestPath" -ForegroundColor Green
}
else {
  Write-Host "Warning: $appxManifestPath not found!" -ForegroundColor Yellow
}

# 3. Update app.manifest (format: X.Y.Z.0)
$appManifestPath = "app.manifest"
if (Test-Path $appManifestPath) {
  (Get-Content $appManifestPath) -replace 'version="[0-9\.]+" name="DuskControl.app"', "version=`"$newVersionWithZeros`" name=`"DuskControl.app`"" | Set-Content $appManifestPath
  Write-Host "Updated $appManifestPath" -ForegroundColor Green
}
else {
  Write-Host "Warning: $appManifestPath not found!" -ForegroundColor Yellow
}
