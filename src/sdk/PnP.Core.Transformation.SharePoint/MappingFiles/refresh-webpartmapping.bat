pushd "C:\github\pnpcore\src\sdk\PnP.Core.Transformation.SharePoint\MappingFiles"
"C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8 Tools\xsd.exe" -c webpartmapping.xsd /n:PnP.Core.Transformation.SharePoint.MappingFiles
pwsh.exe -Command "(gc webpartmapping.cs) -replace 'partial class PageTransformation', 'partial class WebPartMapping' | Out-File -encoding ASCII webpartmapping.cs"
pwsh.exe -Command "$content = Get-Content webpartmapping.cs; $content[23] = '    [System.Xml.Serialization.XmlRootAttribute(Namespace=\"http://schemas.dev.office.com/PnP/2018/01/PageTransformationSchema\", IsNullable=false, ElementName = \"PageTransformation\")]'; $content | Set-Content webpartmapping.cs"
popd