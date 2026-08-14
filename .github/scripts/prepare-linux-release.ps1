param (
    [string] $ReleaseDirectory = "openkh-linux-x64",
    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$launcherProject = Join-Path $repositoryRoot "OpenKh.Tools.Launcher\OpenKh.Tools.Launcher.csproj"
$modManagerProject = Join-Path $repositoryRoot "OpenKh.Tools.ModsManager.Avalonia\OpenKh.Tools.ModsManager.csproj"

if (Test-Path -LiteralPath $ReleaseDirectory) {
    throw "Release directory '$ReleaseDirectory' already exists."
}

$applicationsDirectory = Join-Path $ReleaseDirectory "Apps"
New-Item -ItemType Directory -Path $applicationsDirectory -Force | Out-Null

dotnet publish `
    $launcherProject `
    --configuration $Configuration `
    --runtime linux-x64 `
    --self-contained true `
    --source "https://api.nuget.org/v3/index.json" `
    --output $ReleaseDirectory `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:DebugType=None `
    /p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    throw "Publishing the Linux launcher failed with exit code $LASTEXITCODE."
}

dotnet publish `
    $modManagerProject `
    --configuration $Configuration `
    --runtime linux-x64 `
    --self-contained true `
    --source "https://api.nuget.org/v3/index.json" `
    --output $applicationsDirectory `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:DebugType=None `
    /p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    throw "Publishing the Linux Mod Manager failed with exit code $LASTEXITCODE."
}

Get-ChildItem -LiteralPath $ReleaseDirectory -Filter "*.pdb" -File -Recurse |
    Remove-Item -Force

Copy-Item `
    -LiteralPath (Join-Path $repositoryRoot "distribution\README-LINUX.txt") `
    -Destination (Join-Path $ReleaseDirectory "README-FIRST.txt")
Copy-Item -LiteralPath (Join-Path $repositoryRoot "distribution\install-openkh-linux.sh") -Destination $ReleaseDirectory
Copy-Item -LiteralPath (Join-Path $repositoryRoot "LICENSE") -Destination $ReleaseDirectory
Copy-Item -LiteralPath (Join-Path $repositoryRoot "NOTICE") -Destination $ReleaseDirectory
