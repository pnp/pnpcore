#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

New-Item -Path ./dev/docs -Name "demos" -ItemType "directory"
copy-item -Force ./dev/src/samples/* -Destination ./dev/docs/demos
Get-item ./src/samples/* | Foreach-Object {
  if($_.PSIsContainer){
      $_.BaseName
      Copy-Item "./src/samples/$($_.Name)/*.md" -Destination "./docs/demos/$($_.Name)" -Force
      Copy-Item "./src/samples/$($_.Name)/*.png" -Destination "./docs/demos/$($_.Name)" -Force

      if(Test-Path "./src/samples/$($_.Name)/doc-images"){
        Copy-Item "./src/samples/$($_.Name)/doc-images/*.png" -Destination "./docs/demos/$($_.Name)" -Force
      }
      if(Test-Path "./src/samples/$($_.Name)/assets"){
        Copy-Item "./src/samples/$($_.Name)/assets/*.png" -Destination "./docs/demos/$($_.Name)" -Force
      }
  }
}

docfx metadata ./dev/docs/docfx.json --warningsAsErrors $args
docfx build ./dev/docs/docfx.json --warningsAsErrors $args

# Copy the created site to the pnpcoredocs folder (= clone of the gh-pages branch)
Remove-Item ./gh-pages/api/* -Recurse -Force
Remove-Item ./gh-pages/articles/* -Recurse -Force
Remove-Item ./gh-pages/images/* -Recurse -Force
copy-item -Force -Recurse ./dev/docs/_site/* -Destination ./gh-pages


Get-item ./src/samples/* | % { 
    if($_.PSIsContainer){
        Copy-Item *.md -Destination $_.FullName
    }
}