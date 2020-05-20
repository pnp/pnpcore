# Writing documentation

The documentation system is based upon [DocFX](https://dotnet.github.io/docfx/) and combines inline code comments (the so called triple slash comments) with articles written in MD format. The resulting generated documentation is hosted on https://pnp.github.io/pnpcore. To extend documentation you can:

- Author articles 
- Write inline code documentation via the tripple slash comments

Once you've made changes to the documentation then these changes are not immediatly reflected in the published documentation, this requires that [DocFX](https://dotnet.github.io/docfx/) build is ran again and the resulting content is published.

> [!Note]
> Currently documentation is refreshed manually but we're looking into automating this in the future.

## Writing articles

Articles are at the core of the PnP Core SDK documentation and they live in the `docs\articles` folder. Articles are written in [DocFX Flavored Markdown](https://dotnet.github.io/docfx/spec/docfx_flavored_markdown.html?tabs=tabid-1%2Ctabid-a), which is an extension on top of GitHub flavored markdown. Articles target either the library consumer or the library contributor, hence they should be added in either the **consumer** or **contributor** folder. When an article requires images then all images are added in the `docs\images` folder.

If your article needs to show in the table of contents then you need to make the needed changes in `toc.yml`, which you find in the root of the `docs` folder.

## Writing inline code documentation

Documentation written in the code files themselves is used to generate the **Api Documentation** and depends on docfx parsing the tripple slash comments that you add to the code. Below resources help you get started:

- [Tripple slash (also called XML documents) commenting in .Net code files](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/)
- [DocFX support for tripple slash comments](https://dotnet.github.io/docfx/spec/triple_slash_comments_spec.html)
