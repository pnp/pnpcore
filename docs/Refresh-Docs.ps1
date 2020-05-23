# refesh the metadata (= tripple slash comments)
docfx.exe metadata docfx.json

# Build the site again
docfx.exe build docfx.json

# Copy the created site to the pnpcoredocs folder (= clone of the gh-pages branch)
copy-item -Force -Recurse .\_site\* -Destination D:\github\pnpcoredocs