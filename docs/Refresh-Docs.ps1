$gitDrive = "D:"

# refesh the metadata (= tripple slash comments)
docfx.exe metadata docfx.json

# Build the site again
docfx.exe build docfx.json

# Copy the created site to the pnpcoredocs folder (= clone of the gh-pages branch)
Remove-Item $gitDrive\github\pnpcoredocs\api\* -Recurse -Force
Remove-Item $gitDrive\github\pnpcoredocs\using-the-sdk\* -Recurse -Force
Remove-Item $gitDrive\github\pnpcoredocs\contributing\* -Recurse -Force
Remove-Item $gitDrive\github\pnpcoredocs\images\* -Recurse -Force
copy-item -Force -Recurse .\_site\* -Destination $gitDrive\github\pnpcoredocs