param (
    [string] $BuildDirectory = "bin",
    [string] $ReleaseDirectory = "openkh",
    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -LiteralPath $BuildDirectory -PathType Container)) {
    throw "Build directory '$BuildDirectory' does not exist."
}

if (Test-Path -LiteralPath $ReleaseDirectory) {
    throw "Release directory '$ReleaseDirectory' already exists."
}

$legacyFileNames = Get-ChildItem -LiteralPath $BuildDirectory -File |
    Select-Object -ExpandProperty Name |
    Sort-Object
$legacyDirectoryNames = Get-ChildItem -LiteralPath $BuildDirectory -Directory |
    Select-Object -ExpandProperty Name |
    Sort-Object

New-Item -ItemType Directory -Path $ReleaseDirectory | Out-Null

dotnet publish `
    "OpenKh.Tools.Launcher/OpenKh.Tools.Launcher.csproj" `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained false `
    --output $ReleaseDirectory `
    /p:PublishSingleFile=true `
    /p:DebugType=None `
    /p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    throw "Publishing OpenKH Launcher failed with exit code $LASTEXITCODE."
}

$compatibilityExecutable = Join-Path $ReleaseDirectory "OpenKh.Tools.ModsManager.exe"
Copy-Item `
    -LiteralPath (Join-Path $ReleaseDirectory "OpenKh.Launcher.exe") `
    -Destination $compatibilityExecutable
(Get-Item -LiteralPath $compatibilityExecutable).Attributes += "Hidden"

$panaceaFiles = @(
    "OpenKH.Panacea.dll",
    "avcodec-vgmstream-59.dll",
    "avformat-vgmstream-59.dll",
    "avutil-vgmstream-57.dll",
    "bass.dll",
    "bass_vgmstream.dll",
    "libatrac9.dll",
    "libcelt-0061.dll",
    "libcelt-0110.dll",
    "libg719_decode.dll",
    "libmpg123-0.dll",
    "libspeex-1.dll",
    "libvorbis.dll",
    "swresample-vgmstream-4.dll"
)

foreach ($fileName in $panaceaFiles) {
    $sourcePath = Join-Path $BuildDirectory $fileName
    if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
        throw "Required Panacea file '$sourcePath' does not exist."
    }
}

Copy-Item -LiteralPath "distribution/README-FIRST.txt" -Destination $ReleaseDirectory
Copy-Item -LiteralPath "LICENSE" -Destination $ReleaseDirectory
Copy-Item -LiteralPath "NOTICE" -Destination $ReleaseDirectory

$applicationsDirectory = Join-Path $ReleaseDirectory "Apps"
Move-Item -LiteralPath $BuildDirectory -Destination $applicationsDirectory

$duplicateLauncherFiles = Get-ChildItem -LiteralPath $applicationsDirectory -File | Where-Object {
    $_.Name -like "OpenKh.Launcher.*"
}

foreach ($duplicateFile in $duplicateLauncherFiles) {
    Remove-Item -LiteralPath $duplicateFile.FullName
}

$packagedModManager = Join-Path $applicationsDirectory "OpenKh.Tools.ModsManager.exe"
if (-not (Test-Path -LiteralPath $packagedModManager -PathType Leaf)) {
    throw "Required Mod Manager executable '$packagedModManager' does not exist."
}

$legacyFileManifest = Join-Path $applicationsDirectory "legacy-release-files.txt"
$legacyDirectoryManifest = Join-Path $applicationsDirectory "legacy-release-directories.txt"
$legacyFileNames | Set-Content -LiteralPath $legacyFileManifest -Encoding UTF8
$legacyDirectoryNames | Set-Content -LiteralPath $legacyDirectoryManifest -Encoding UTF8
