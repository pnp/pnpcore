# PnP Core SDK documentation

This folder contains the PnP Core SDK documentation. The documentation system is based upon [DocFX](https://dotnet.github.io/docfx/) and combines inline code comments (the so called triple slash comments) with articles written in MD format. The resulting generated documentation is hosted on https://pnp.github.io/pnpcore.

## Documentation generation

The document generation consists of 3 steps outlined below, currently these are manual steps, but we plan to automate them via GitHub Actions:

- Extract the PnP Core SDK into YML based API documentation: use `docfx.exe metadata docfx.json`
- Build the documentation: in this step the YML API documentation and the articles are merged into a documentation site. Use `docfx.exe build docfx.json` and then check the generated `_site` folder
- Copy the generated documentation site to it's hosting location **toupdate**

> [!Note]
> Install the latest [DocFX](https://dotnet.github.io/docfx/) release to get `docfx.exe`

To run anywhere add docfx directory to your environment variables in windows

### References

Setting up the gh-pages branch as an orphaned branch was done using the steps outlined in https://www.gep13.co.uk/blog/how-to-create-gh-pages-branch. To actually work with both "code" and "docs" branches it's easiest to `git clone` the repo twice, once for coding and once for publishing documents to the `gh-pages` branch.

## Contributing to the documentation

We strongly encourage documentation contributions which can be done via improving API documentation (via the triple slash comments in the source code) or creating/updating articles in the `articles` folder.

## Martial UI

Credit to Oscar Vásquez [https://ovasquez.github.io/docfx-material/](https://ovasquez.github.io/docfx-material/) for awesome work on the skin.