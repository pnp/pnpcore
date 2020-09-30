#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

New-Item -Path ./dev/docs -Name "demos" -ItemType "directory"
copy-item -Force ./dev/src/samples/* -Destination ./dev/docs/demos
Get-item ./dev/src/samples/* | Foreach-Object {
  if($_.PSIsContainer){
      $_.BaseName
      Copy-Item "./dev/src/samples/$($_.Name)/*.md" -Destination "./dev/docs/demos/$($_.Name)" -Force
      Copy-Item "./dev/src/samples/$($_.Name)/*.png" -Destination "./dev/docs/demos/$($_.Name)" -Force

      if(Test-Path "./dev/src/samples/$($_.Name)/docs-images"){
        New-Item -Path "./dev/docs/demos/$($_.Name)/" -Name "docs-images" -ItemType "directory" -Force
        Copy-Item "./dev/src/samples/$($_.Name)/docs-images/*.png" -Destination "./dev/docs/demos/$($_.Name)/docs-images" -Force
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