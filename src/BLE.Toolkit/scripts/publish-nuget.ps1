#Requires -Version 5.1
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$Source = 'https://api.nuget.org/v3/index.json',

    [string]$ApiKey = $env:NUGET_API_KEY,

    [string]$OutputDirectory = '',

    [switch]$SkipPack,

    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDirectory = Split-Path -Parent $scriptRoot
$versionProjectPath = Join-Path $solutionDirectory 'BLE.Toolkit\BLE.Toolkit.csproj'

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $solutionDirectory 'artifacts\nuget'
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw 'NuGet API key is required. Pass -ApiKey or set the NUGET_API_KEY environment variable.'
}

if (-not $SkipPack) {
    $packScript = Join-Path $scriptRoot 'pack-nuget.ps1'
    $packParams = @{
        Configuration   = $Configuration
        OutputDirectory = $OutputDirectory
    }

    if ($NoBuild) {
        $packParams.NoBuild = $true
    }

    & $packScript @packParams
}

$version = dotnet msbuild $versionProjectPath `
    -getProperty:Version `
    -p:Configuration=$Configuration `
    -nologo

if ([string]::IsNullOrWhiteSpace($version)) {
    throw 'Unable to resolve package version from MSBuild properties.'
}

$packages = Get-ChildItem -Path $OutputDirectory -Filter *.nupkg | Sort-Object Name
if ($packages.Count -eq 0) {
    throw "No .nupkg files found in $OutputDirectory."
}

Write-Host "Publishing NuGet packages version $version to $Source..."

foreach ($package in $packages) {
    Write-Host "Publishing $($package.Name)..."
    dotnet nuget push $package.FullName `
        --source $Source `
        --api-key $ApiKey `
        --skip-duplicate

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to publish $($package.Name)."
    }

    $symbolPackage = Join-Path $OutputDirectory ($package.BaseName + '.snupkg')
    if (Test-Path $symbolPackage) {
        Write-Host "Publishing $($package.BaseName).snupkg..."
        dotnet nuget push $symbolPackage `
            --source $Source `
            --api-key $ApiKey `
            --skip-duplicate

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to publish symbol package for $($package.Name)."
        }
    }
}

Write-Host 'Publish completed successfully.'
