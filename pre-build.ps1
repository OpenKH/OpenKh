##############################################################################
# Assign assembly tags to every project                                      #
#                                                                            #
# This script essentially modifies every *.csproj file to create tags like   #
# "company name", "author" and mostly version number.                        #
# This script is not meant to be run locally, but only to be consumed by     #
# Azure Pipelines. It still work locally for the purpose of testing, but     #
# otherwise it is quite pointless.                                           #
##############################################################################

# https://docs.microsoft.com/en-us/azure/devops/pipelines/scripts/powershell?view=azure-devops

function Log([string]$text) {
    Write-Debug $text
}

# If this script is not running on a build server, remind user to
# set environment variables so that this script can be debugged
if (-not ($Env:BUILD_SOURCESDIRECTORY -and $Env:BUILD_BUILDNUMBER)) {
    Write-Warning "Executing locally, setting dummy values"

    $Env:BUILD_SOURCESDIRECTORY = (Get-Location).Path
    $Env:BUILD_SOURCEBRANCHNAME = "local"
}

# Make sure path to source code directory is available
if (-not $Env:BUILD_SOURCESDIRECTORY) {
    Write-Error "BUILD_SOURCESDIRECTORY environment variable is missing."
    exit 1
}
elseif (-not (Test-Path $Env:BUILD_SOURCESDIRECTORY)) {
    Write-Error "BUILD_SOURCESDIRECTORY does not exist: $Env:BUILD_SOURCESDIRECTORY"
    exit 1
}
Log "BUILD_SOURCESDIRECTORY: $Env:BUILD_SOURCESDIRECTORY"

# Make sure there is a build number
if (-not $Env:BUILD_BUILDNUMBER) {
    Write-Warning "BUILD_BUILDNUMBER environment variable is missing. Using default value."
    
    $dateNow = Get-Date
    $Env:BUILD_BUILDNUMBER = "{0:D04}{1:D02}{2:D02}.0" -f $dateNow.Year, $dateNow.Month, $dateNow.Day
}

Log "BUILD_BUILDNUMBER: $Env:BUILD_BUILDNUMBER"
Log "BUILD_SOURCEBRANCHNAME: $Env:BUILD_SOURCEBRANCHNAME"

$sourcesDirectory = $env:BUILD_SOURCESDIRECTORY

function Set-NodeValue($rootNode, [string]$nodeName, [string]$value) {
    $nodePath = "PropertyGroup/$($nodeName)"

    $node = $rootNode.Node.SelectSingleNode($nodePath)

    if ($null -eq $node) {
        $group = $rootNode.Node.SelectSingleNode("PropertyGroup")
        $node = $group.OwnerDocument.CreateElement($nodeName)
        $group.AppendChild($node) | Out-Null
    }

    $node.InnerText = $value

    Log "Set $($nodeName) to $($value)"
}

# We assume that BUILDNUMBER is '20190723.8'
$buildNumber = $Env:BUILD_BUILDNUMBER
$majorVersion = $buildNumber.Substring(2, 2)
$minorVersion = $buildNumber.Substring(4, 2)
$build = $buildNumber.Substring(6, 2)
$revision = $buildNumber.Substring(9)
$actualVersion = "$($majorVersion).$($minorVersion).$($build).$($revision)"
$informativeVersion = "$($actualVersion)-$($Env:BUILD_SOURCEBRANCHNAME)"

Get-ChildItem -Path $sourcesDirectory -Filter "*.csproj" -Recurse -File |
Where-Object { $_.FullName -like "*OpenKh.*" } |
Where-Object { $_.FullName -notlike "*OpenKh.*Test*" } |
ForEach-Object {
    Log "Patching $($_.FullName)"

    $projectPath = $_.FullName
    $project = Select-Xml $projectPath -XPath "//Project"

    Set-NodeValue $project "AssemblyVersion" $actualVersion
    Set-NodeValue $project "FileVersion" $actualVersion
    Set-NodeValue $project "Version" $informativeVersion
    Set-NodeValue $project "InformationalVersion" $informativeVersion
    Set-NodeValue $project "Authors" "OpenKH contributors"
    Set-NodeValue $project "Company" "OpenKH"
    Set-NodeValue $project "Copyright" "Copyright (C) OpenKH $($date.Year)"
    Set-NodeValue $project "Description" "https://github.com/OpenKH/OpenKh"

    $document = $project.Node.OwnerDocument
    $document.PreserveWhitespace = $true

    $document.Save($projectPath)
}
