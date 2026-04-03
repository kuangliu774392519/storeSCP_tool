param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$projectPath = Join-Path $repoRoot "src\StorescpTool.App\StorescpTool.App.csproj"
$outputDir = Join-Path $repoRoot "dist\$Runtime"

Write-Host "Publishing StorescpTool..."
Write-Host "Project: $projectPath"
Write-Host "Output:  $outputDir"

dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $outputDir

Write-Host "Publish completed: $outputDir"
