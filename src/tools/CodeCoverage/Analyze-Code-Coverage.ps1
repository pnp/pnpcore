<#
.SYNOPSIS
	The function runs unit tests for the selected library or class and prints the code coverage report using html format.
	
​.PARAMETER TestProjectName
	Test project name, for example PnP.Core.Test. Either TestProjectName or FqdnClassName should be provided.

​.PARAMETER FqdnClassName
	Fully qualified class name, for example PnP.Core.Test.SharePoint.FilesTests. Either TestProjectName or FqdnClassName should be provided.
	
​.PARAMETER OpenReport
	Whether to open a report using the default browser, default is TRUE

.EXAMPLE
	.\Analyze-Code-Coverage.ps1 -FqdnClassName PnP.Core.Test.SharePoint.FilesTests
​
	The above command will run all tests in the specified class and will launch a browser with the coverage report
	
	.\Analyze-Code-Coverage.ps1 -TestProjectName PnP.Core.Test -OpenReport $false
	
	The above command will run all tests in the specified test project and will NOT open a browser with the coverage report
#>
param (
[Parameter(Mandatory = $false)]
[string]$TestProjectName,

[Parameter(Mandatory = $false)]
[string]$FqdnClassName,

[Parameter(Mandatory = $false)]
[bool]$OpenReport = $true)

if(!$TestProjectName -and !$FqdnClassName) {
	throw "You should provide either -TestProjectName or -FqdnClassName parameter"
	return;
}

$projectPath = ""

if($TestProjectName) {
	$projectPath = "../../sdk/$($TestProjectName)/$($TestProjectName).csproj"
} else {
	$index = $FqdnClassName.IndexOf(".Test.") + ".Test.".Length;
	$libName = $FqdnClassName.Substring(0, $index - 1)
	$projectPath = "../../sdk/$($libName)/$($libName).csproj"
}

$tempFolder = [System.IO.Path]::GetTempPath()
$pnpTestsFolder = "$($tempFolder)pnp-sdk-tests"

$testResultsPath = "$($pnpTestsFolder)/TestResults"
$coveragePath = "$($pnpTestsFolder)/coverage"

if (Test-Path -Path $pnpTestsFolder) {
	Remove-Item -Recurse -Force $pnpTestsFolder
}


if(!$FqdnClassName) {
	dotnet test $projectPath --no-restore --verbosity normal --collect:"XPlat Code Coverage" --results-directory $testResultsPath
} else {
	dotnet test $projectPath --no-restore --verbosity normal --collect:"XPlat Code Coverage" --results-directory $testResultsPath --filter ClassName=$FqdnClassName
}

reportgenerator -reports:"$($testResultsPath)/**/*.cobertura.xml" -targetdir:$coveragePath -reporttypes:html

$coverageHtml = "$($coveragePath)/index.html"

Write-Host ""
Write-Host "The coverage file is generated under '$($coverageHtml)'" -ForegroundColor Yellow
Write-Host ""

if($OpenReport) {
	Invoke-Item $coverageHtml
	Write-Host ""
}
