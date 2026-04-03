param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputDir = "",
    [switch]$CreatePortableZip,
    [string]$PackageVersion = "",
    [string]$PackageOutputDir = ""
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$projectPath = Join-Path $repoRoot "src\StorescpTool.App\StorescpTool.App.csproj"

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $outputDir = Join-Path $repoRoot "dist\$Runtime"
}
else {
    $outputDir = if ([System.IO.Path]::IsPathRooted($OutputDir)) {
        $OutputDir
    }
    else {
        Join-Path $repoRoot $OutputDir
    }
}

if ([string]::IsNullOrWhiteSpace($PackageOutputDir)) {
    $packageOutputDir = Join-Path $repoRoot ".artifacts\portable"
}
else {
    $packageOutputDir = if ([System.IO.Path]::IsPathRooted($PackageOutputDir)) {
        $PackageOutputDir
    }
    else {
        Join-Path $repoRoot $PackageOutputDir
    }
}

function Resolve-DotnetPath {
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $fallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $fallback) {
        return $fallback
    }

    throw "dotnet executable not found."
}

function Remove-DirectoryIfExists([string]$PathToRemove, [string]$WorkspaceRoot) {
    if (-not (Test-Path -LiteralPath $PathToRemove)) {
        return
    }

    $resolvedRoot = (Resolve-Path -LiteralPath $WorkspaceRoot).Path
    $resolvedTarget = (Resolve-Path -LiteralPath $PathToRemove).Path

    if (-not $resolvedTarget.StartsWith($resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to delete path outside workspace: $resolvedTarget"
    }

    Remove-Item -LiteralPath $resolvedTarget -Recurse -Force
}

$dotnet = Resolve-DotnetPath

Write-Host "Publishing StorescpTool..."
Write-Host "dotnet:   $dotnet"
Write-Host "Project: $projectPath"
Write-Host "Output:  $outputDir"

Remove-DirectoryIfExists $outputDir $repoRoot
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

& $dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $outputDir

Write-Host "Publish completed: $outputDir"

if (-not $CreatePortableZip) {
    return
}

$packageVersionLabel = if ([string]::IsNullOrWhiteSpace($PackageVersion)) {
    Get-Date -Format "yyyyMMdd_HHmmss"
}
else {
    $PackageVersion
}

$packageBaseName = "StorescpTool_{0}_portable_{1}" -f $Runtime, $packageVersionLabel
$stagingRoot = Join-Path $packageOutputDir "staging"
$stagingPackageDir = Join-Path $stagingRoot $packageBaseName
$zipPath = Join-Path $packageOutputDir ($packageBaseName + ".zip")
$hashPath = Join-Path $packageOutputDir ($packageBaseName + ".sha256.txt")

Remove-DirectoryIfExists $stagingRoot $repoRoot
New-Item -ItemType Directory -Path $stagingPackageDir -Force | Out-Null
New-Item -ItemType Directory -Path $packageOutputDir -Force | Out-Null

Copy-Item -Path (Join-Path $outputDir '*') -Destination $stagingPackageDir -Recurse -Force

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

if (Test-Path -LiteralPath $hashPath) {
    Remove-Item -LiteralPath $hashPath -Force
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($stagingRoot, $zipPath)

if (-not (Test-Path -LiteralPath $zipPath)) {
    throw "Portable zip was not created: $zipPath"
}

$hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $zipPath).Hash
"$hash  $(Split-Path -Leaf $zipPath)" | Set-Content -Path $hashPath -Encoding ASCII

Write-Host "Portable zip created: $zipPath"
Write-Host "SHA256 file created:   $hashPath"
