#Requires -Version 5.1
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [string]$OutputDirectory = '',

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

$packableProjects = @(
    'BLE.Toolkit.Interfaces\BLE.Toolkit.Interfaces.csproj'
    'BLE.Toolkit\BLE.Toolkit.csproj'
    'BLE.Toolkit.Windows\BLE.Toolkit.Windows.csproj'
)

$version = dotnet msbuild $versionProjectPath `
    -getProperty:Version `
    -p:Configuration=$Configuration `
    -nologo

if ([string]::IsNullOrWhiteSpace($version)) {
    throw 'Unable to resolve package version from MSBuild properties.'
}

Write-Host "Packing NuGet packages version $version ($Configuration)..."

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

$packArgs = @(
    'pack'
    '--configuration', $Configuration
    '--output', $OutputDirectory
    "-p:PackageVersion=$version"
)

if ($NoBuild) {
    $packArgs += '--no-build'
}

foreach ($project in $packableProjects) {
    $projectPath = Join-Path $solutionDirectory $project
    Write-Host "Packing $project..."
    & dotnet @packArgs $projectPath
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed for $project."
    }
}

Write-Host ''
Write-Host "Packages created in: $OutputDirectory"
Get-ChildItem -Path $OutputDirectory -Filter *.nupkg | ForEach-Object {
    Write-Host "  $($_.Name)"
}
