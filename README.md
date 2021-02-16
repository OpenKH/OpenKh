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

OpenKH tools require the instllation of the [.NET Core 3.1 Runtime](https://dotnet.microsoft.com/download/dotnet-core/3.1). All the UI tools are designed to work on Windows, while command line tools will work on any operating system.
Note: All CLI and GUI programs **should** be cross-platform, though extensive testing primarily happens on Windows systems.

<p align="center">
  <img src="./images/Runtime.jpg" width="400">
</p>

## OpenKH in depth

<p align="center">
  <img src="./images/diagram.png" width="908">
</p>

From an architectural point of view, the code is structured to abstract low-level implementation such as file parsers and infrastructural logic to high-level functionalties such as 3D rendering or tools. The projects are layered to be able to share as much as code possible, but isolated in order to avoid coupling.

From a community perspective, OpenKH will provide the best form of documentation, modding portal and fan-game support that is derived from it.

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

The entire content of the repository is protected by the Apache 2.0 license. Some of the key points of the license are:

* You **can** copy, modify, and distribute the software.
* You **can** use this software privately.
* You **can** use this software for commercial purposes.
* You **can** append to the "NOTICE" file, if said file exists in the main repository.
* You **cannot** hold any contributor to the repository liable for damages.
* You **cannot** change or otherwise modify any patent, trademark, and attribution notices from the source repository.
* You **must** indicate changes made to the code, if any.
* You **must** include the same NOTICE file in every distribution, if included within the original repository.
* You **must** include the license and copyright notice with each and every distribution and fork.
* Any modifications of this code base **ABSOLUTELY MUST** be distributed with the same license, Apache 2.0.
