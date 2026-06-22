param(
   [switch]$IncludeSamples
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

# Main DocFX build
Write-Host "Building docfx..." -ForegroundColor Cyan

# Stage sample docs into docs/demos so existing docfx.json patterns continue to work.
$docsRoot = $PSScriptRoot
$samplesRoot = Join-Path $docsRoot "../samples"
$demosRoot = Join-Path $docsRoot "demos"

if (Test-Path $demosRoot)
{
   Remove-Item $demosRoot -Recurse -Force
}

New-Item -Path $docsRoot -Name "demos" -ItemType "directory" | Out-Null

if ($IncludeSamples)
{
   Write-Host "Including demo/sample content..." -ForegroundColor Cyan
   Copy-Item -Force (Join-Path $samplesRoot "*") -Destination $demosRoot

   Get-Item (Join-Path $samplesRoot "*") | ForEach-Object {
      if ($_.PSIsContainer)
      {
         $_.BaseName

         $sampleFolder = Join-Path $samplesRoot $_.Name
         $demoFolder = Join-Path $demosRoot $_.Name

         if (Test-Path (Join-Path $sampleFolder "*.md"))
         {
            Copy-Item (Join-Path $sampleFolder "*.md") -Destination $demoFolder -Force
         }

         if (Test-Path (Join-Path $sampleFolder "*.png"))
         {
            Copy-Item (Join-Path $sampleFolder "*.png") -Destination $demoFolder -Force
         }

         $sampleDocsImages = Join-Path $sampleFolder "docs-images"
         if (Test-Path $sampleDocsImages)
         {
            $demoDocsImages = Join-Path $demoFolder "docs-images"
            New-Item -Path $demoFolder -Name "docs-images" -ItemType "directory" -Force | Out-Null
            if (Test-Path (Join-Path $sampleDocsImages "*.png"))
            {
               Copy-Item (Join-Path $sampleDocsImages "*.png") -Destination $demoDocsImages -Force
            }
         }
      }
   }
}
else
{
   Write-Host "Skipping demo/sample content. Use -IncludeSamples to include it." -ForegroundColor Yellow
   @(
      "# Samples",
      "",
      "Sample and demo content is excluded from this local build.",
      "",
      "Run this script with -IncludeSamples to include sample documentation."
   ) | Set-Content -Path (Join-Path $demosRoot "README.md") -Encoding UTF8
}

<#
    DocFx is now installed as a .NET tool. To use it:
    
    1. Ensure .NET SDK is installed (version 6.0 or higher)
       - Download from: https://dotnet.microsoft.com/download
    
    2. Restore the docfx tool (if not already done):
       - Run: dotnet tool restore
    
    3. Run docfx using:
       - dotnet docfx build docfx.json

    4. Test the site locally: 
       - Open new terminal 
       - Navigate to _site 
       - either use http-server -c-1 (npm install -g http-server) or dotnet docfx serve
#>

Write-Host "Running docfx using .NET tool..." -ForegroundColor Cyan
dotnet docfx metadata docfx.json --warningsAsErrors $args
dotnet docfx build docfx.json --warningsAsErrors $args