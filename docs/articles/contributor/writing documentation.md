# Writing documentation

The documentation system is based on [DocFX](https://dotnet.github.io/docfx/), and combines inline code comments (the so called triple slash comments) with articles written in MD format. The resulting generated documentation is hosted on https://pnp.github.io/pnpcore. To extend documentation you can:

- Author articles
- Write inline code documentation via the triple slash comments

Once you've made changes to the documentation and your changes are merged into the `dev` branch then a GitHub workflow will be triggered and this workflow will refresh documentation automatically.

## Writing articles

Articles are at the core of the PnP Core SDK documentation and they live in the `docs\articles` folder. Articles are written in [DocFX Flavored Markdown](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html?tabs=tabid-1%2Ctabid-a), which is an extension on top of GitHub flavored markdown. Articles target either the library consumer or the library contributor, hence they should be added to the **consumer** or **contributor** folder respectively. When an article requires images, then all images are added in the `docs\images` folder. You can eventually organize images in sub-folders of the `docs\images` folder.

If you want to show your article in the table of contents, then you need to make the needed changes in `toc.yml`, which you find in the root of the `docs` folder.

## Writing inline code documentation

Documentation written in the code files themselves is used to generate the **Api Documentation** and depends on docfx parsing the triple slash comments that you add to the code. Below resources help you get started:

- [Triple slash (also called XML documents) commenting in .Net code files](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/)
- [DocFX support for triple slash comments](https://dotnet.github.io/docfx/spec/triple_slash_comments_spec.html)
