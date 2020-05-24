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
    [string] $verbosity = "minimal",
    [string] $output = "bin"
)

function Test-Success([int] $exitCode) {
    if ($exitCode -ne 0) {
        Write-Error "Last command returned error $exitCode, therefore the build is canceled."
        exit
    }
}

# Use submodules
git submodule update --init --recursive --depth 1
Test-Success $LASTEXITCODE

# Restore NuGet packages
dotnet restore
Test-Success $LASTEXITCODE

# Run tests
dotnet test --configuration $configuration --verbosity $verbosity
Test-Success $LASTEXITCODE

# Publish solution
dotnet publish --configuration $configuration --verbosity $verbosity --framework netcoreapp3.1 --output $output /p:DebugType=None /p:DebugSymbols=false
$LASTEXITCODE = 0 # Hack due to a regression at compiling multi-targeting framework projects
Test-Success $LASTEXITCODE