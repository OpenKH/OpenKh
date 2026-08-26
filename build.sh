#!/bin/sh

##############################################################################
# Build the entire solution in a very similar way of Azure Pipelines         #
#                                                                            #
# This script is essentially the same exact script of "build.ps1", but it    #
# works on both Linux and macOS without necessarly install PowerShell.       #     
#                                                                            #
# The only problem here is that .NET Core 3.1 sdk does not allow to build    #
# OpenKh.sln because of the Windows-only projects there. So this script      #
# temporarly creates a solution that only includes OpenKh.Command.* projects.#
#                                                                            #
# This also print some configuration to easily access CLI tools from a shell.#
##############################################################################

export configuration="Release"
export verbosity="minimal"
export output="bin"
export solutionBase="OpenKh.Linux"
export solution="$solutionBase.sln"

git submodule update --init --recursive --depth 1
if [ $? -ne 0 ]
then
    exit 1
fi

# Create solution for Linux and macOS
dotnet new sln -n $solutionBase --force
if [ $? -ne 0 ]
then
    exit 1
fi

# Add only command line tools to the new solution
for project in ./OpenKh.Command.*/*.csproj; do
    dotnet sln $solution add "$project"
done
for project in ./OpenKh.Game*/*.csproj; do
    dotnet sln $solution add "$project"
done

# Restore NuGet packages
dotnet restore $solution
if [ $? -ne 0 ]
then
    rm $solution
    exit 1
fi

# Run tests
dotnet test $solution --configuration $configuration --verbosity $verbosity
if [ $? -ne 0 ]
then
    rm $solution
    exit 1
fi

# Publish solution
dotnet publish $solution --configuration $configuration --verbosity $verbosity --framework net8.0 --output $output /p:DebugType=None /p:DebugSymbols=false

rm $solution

# Print some potentially useful info
echo "It is very recommended to append to your '~/.profile' file the path of"
echo "OpenKH binaries and their alias to run them from any folder. You only"
echo "need to do it once per user."
echo "To do so, please execute the following commands:"

awk '{ sub("\r$", ""); print }' ./openkh_alias > ./bin/openkh_alias
chmod +x ./bin/OpenKh.Command.*
export OPENKH_BIN="$(realpath ./bin)"
echo "echo 'export OPENKH_BIN=\"$OPENKH_BIN\"' >> ~/.profile"
echo "echo 'export PATH=\$PATH:\$OPENKH_BIN' >> ~/.profile"
echo "echo 'source \$OPENKH_BIN/openkh_alias' >> ~/.profile"
