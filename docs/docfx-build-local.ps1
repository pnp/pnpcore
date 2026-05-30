$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

# Data Generated Content
$runLocation  = Get-Location

# Main DocFX build
Write-Host "Building docfx..." -ForegroundColor Cyan

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