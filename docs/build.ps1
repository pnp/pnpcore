#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

cd ./dev/docs
docfx metadata ./dev/docs/docfx.json --warningsAsErrors $args
docfx build ./dev/docs/docfx.json --warningsAsErrors $args

# Copy the created site to the pnpcoredocs folder (= clone of the gh-pages branch)
Remove-Item ./gh-pages/api/* -Recurse -Force
Remove-Item ./gh-pages/articles/* -Recurse -Force
Remove-Item ./gh-pages/images/* -Recurse -Force
copy-item -Force -Recurse ./dev/docs/_site/* -Destination ./gh-pages
