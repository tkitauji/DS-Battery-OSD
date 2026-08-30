param(
    [ValidatePattern('^\d+\.\d+\.\d+\.\d+$')]
    [string]$Version = "1.0.0.0",
    [string]$DotNetPath = "dotnet"
)

$ErrorActionPreference = "Stop"
$projectDirectory = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$packageRoot = Join-Path $projectDirectory "AppPackages\$Version"
$stagingDirectory = Join-Path $packageRoot "Package"
$packagePath = Join-Path $packageRoot "DSBatteryOSD_$Version`_x64.msix"

if (Test-Path -LiteralPath $packageRoot) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $stagingDirectory | Out-Null

& $DotNetPath publish (Join-Path $projectDirectory "DsBatteryOsd.csproj") `
    -c Release `
    -r win-x64 `
    --self-contained true `
    --source https://api.nuget.org/v3/index.json `
    -p:PublishSingleFile=false `
    -o $stagingDirectory
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Get-ChildItem -LiteralPath $stagingDirectory -Filter "*.pdb" -File | Remove-Item -Force

$manifest = [xml](Get-Content -LiteralPath (Join-Path $projectDirectory "Package.appxmanifest") -Raw)
$manifest.Package.Identity.Version = $Version
$manifest.Save((Join-Path $stagingDirectory "AppxManifest.xml"))

$assetDirectory = Join-Path $stagingDirectory "Assets"
New-Item -ItemType Directory -Force -Path $assetDirectory | Out-Null
@(
    "StoreLogo.png",
    "Square44x44Logo.png",
    "Square150x150Logo.png",
    "Wide310x150Logo.png",
    "Square310x310Logo.png"
) | ForEach-Object {
    Copy-Item -LiteralPath (Join-Path $projectDirectory "Assets\$_") -Destination $assetDirectory
}

$packagesDirectory = if ($env:NUGET_PACKAGES) {
    $env:NUGET_PACKAGES
} else {
    Join-Path $env:USERPROFILE ".nuget\packages"
}
$makeAppx = Join-Path $packagesDirectory "microsoft.windows.sdk.buildtools\10.0.28000.2705\bin\10.0.28000.0\x64\makeappx.exe"
if (-not (Test-Path -LiteralPath $makeAppx)) {
    throw "makeappx.exe was not found. Run 'dotnet restore' first. Expected: $makeAppx"
}

& $makeAppx pack /d $stagingDirectory /p $packagePath /o
if ($LASTEXITCODE -ne 0) {
    throw "makeappx failed with exit code $LASTEXITCODE"
}

Write-Host "Store package created: $packagePath"
