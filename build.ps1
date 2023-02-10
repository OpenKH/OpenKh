##############################################################################
# Build the entire solution in a very similar way of Azure Pipelines         #
#                                                                            #
# This script is not meant to create redistributable executables for OpenKH, #
# but they can be used for local usage.                                      #
#                                                                            #
# The only difference with Azure Pipelines is that this script does not run  #
# "pre-build.ps1", which is responsible to assign tags to projects for       #
# copyright and versioning. The main reason is because we do not want to     #
# modify existing files when compiling.                                      #
##############################################################################

param (
    [string] $configuration = "Release",
    [string] $sourceConfiguration = ".NET Release",
    [string] $verbosity = "minimal",
    [string] $output = "bin"
)

$solutionBase = "OpenKh.Windows"
$solution = "${solutionBase}.sln"

function Test-Success([int] $exitCode) {
    if ($exitCode -ne 0) {
        Remove-Item $solution -ErrorAction Ignore
        Write-Error "Last command returned error $exitCode, therefore the build is canceled."
        exit $exitCode
    }
}

function Get-CSProjects([string]$filter) {
    Get-ChildItem -Filter $filter | ForEach-Object {
        $csprojPath = (Join-Path $_.FullName $_.Name) + ".csproj"
        if ( Test-Path -Path $csprojPath -PathType Leaf ) {
            Write-Output $csprojPath
        }
    }
}

# Use submodules
git submodule update --init --recursive --depth 1
Test-Success $LASTEXITCODE

# Remove previously created solution file
Remove-Item $solution -ErrorAction Ignore

# Restore NuGet packages
dotnet restore
Test-Success $LASTEXITCODE

# Run tests
dotnet test --configuration $sourceConfiguration --verbosity $verbosity
Test-Success $LASTEXITCODE

# Create temporary solution
dotnet new sln -n $solutionBase --force
Test-Success $LASTEXITCODE

# Add items to solution
Get-CSProjects "OpenKh.Command.*" | ForEach-Object {
    dotnet sln $solution add $_
}
Get-CSProjects "OpenKh.Tools.*" | ForEach-Object {
    dotnet sln $solution add $_
    Test-Success $LASTEXITCODE
}
Get-CSProjects "OpenKh.WinShell.*" | ForEach-Object {
    dotnet sln $solution add $_
    Test-Success $LASTEXITCODE
}
Get-CSProjects "OpenKh.Game*" | ForEach-Object {
    dotnet sln $solution add $_
    Test-Success $LASTEXITCODE
}

# Publish solution
dotnet publish $solution --configuration $configuration --verbosity $verbosity --output $output /p:DebugType=None /p:DebugSymbols=false
Test-Success $LASTEXITCODE

# Remove the temporary solution after the solution is published
Remove-Item $solution -ErrorAction Ignore
