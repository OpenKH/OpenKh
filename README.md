<p align="center">
  <img src="./images/OpenKH.png" width="540">
</p>

Aims to centralize all the technical knowledge of the 'Kingdom Hearts' game series in one place, providing documentation, tools, code libraries, and the foundation for modding the commercial games.

[![Build Status](https://dev.azure.com/xeeynamo/OpenKH/_apis/build/status/Xeeynamo.OpenKh?branchName=master) ![Tests](https://img.shields.io/azure-devops/tests/xeeynamo/OpenKh/4) ![Coverage](https://img.shields.io/azure-devops/coverage/xeeynamo/OpenKh/4)](https://dev.azure.com/xeeynamo/OpenKH/_build/latest?definitionId=4&branchName=master)

## Documentation

All the documentation is located in the `/docs` folder in its raw form. A more web-friendly version can be accessed at: https://openkh.dev/

## Downloads

New builds of OpenKH are automatically generated every time one of the contributors inspects and approves a new proposed feature or fix. Those builds are considered stable as they are built from the `master` branch. The version format used in the builds is `YEAR.MONTH.DAY.BUILDID`.

[![OpenKh](https://img.shields.io/badge/OpenKh-Download-blue.svg)](https://github.com/Xeeynamo/OpenKh/releases)

All the builds from `master` and from pull requestes are generated from [Azure Pipelines](https://dev.azure.com/xeeynamo/OpenKH/_build).

OpenKH tools require the instllation of the [.NET Core 3.1 Runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1/runtime). All the UI tools are designed to work on Windows, while command line tools will work on any operating system.

## Build from source code

The minimum requirement is [.NET Core 3.1.x](https://dotnet.microsoft.com/download/dotnet-core/3.1). Once the repository is downloaded, `build.ps1` or `build.sh` needs be executed based from the operating system in use. That is all.

## Additional info

### Future plans

* Provide a fully fledged and user friendly modding toolchain.
* Centralize modding downloads with a review system.
* Provide a friendly environment for mod users and creators alike.
* Create a community site and forum where users can openly interact with and help one another with modifications using OpenKH tools and documentation.
* Create a custom game engine that is compatible with assets from the retail games.

### Contribution

There is a [guide](CONTRIBUTING.md) describing how to contact the team and contribute to the project.

### License

The entire content of the repository is protected by the GPLv3 license. Some of the key points of the license are:

* You **can** copy, modify, and distribute the software.
* You **must** include the license and copyright notice with each and every distribution.
* You **can** use this software privately.
* You **can** use this software for commercial purposes.
* If you modify it, you **must** indicate changes made to the code.
* Any modifications of this code base **ABSOLUTELY MUST** be distributed with the same license, GPLv3.
