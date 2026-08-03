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

New-Item -ItemType Directory -Path $ReleaseDirectory | Out-Null

$applicationsDirectory = Join-Path $ReleaseDirectory "Apps"
$modManagerDirectory = Join-Path $applicationsDirectory "ModManager"
New-Item -ItemType Directory -Path $modManagerDirectory -Force | Out-Null

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

dotnet publish `
    "OpenKh.Tools.ModsManager/OpenKh.Tools.ModsManager.csproj" `
    --configuration $Configuration `
    --output $modManagerDirectory `
    /p:DebugType=None `
    /p:DebugSymbols=false

if ($LASTEXITCODE -ne 0) {
    throw "Publishing Mods Manager failed with exit code $LASTEXITCODE."
}

$referencedCommandArtifacts = Get-ChildItem -LiteralPath $modManagerDirectory -File | Where-Object {
    $_.Name -like "OpenKh.Command.*" -and $_.Extension -ne ".dll"
}

foreach ($referencedCommandArtifact in $referencedCommandArtifacts) {
    Remove-Item -LiteralPath $referencedCommandArtifact.FullName
}

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

    Copy-Item -LiteralPath $sourcePath -Destination $modManagerDirectory
}

Copy-Item -LiteralPath "distribution/README-FIRST.txt" -Destination $ReleaseDirectory
Copy-Item -LiteralPath "LICENSE" -Destination $ReleaseDirectory
Copy-Item -LiteralPath "NOTICE" -Destination $ReleaseDirectory

$advancedToolsDirectory = Join-Path $ReleaseDirectory "AdvancedTools"
Move-Item -LiteralPath $BuildDirectory -Destination $advancedToolsDirectory

$duplicateApplicationFiles = Get-ChildItem -LiteralPath $advancedToolsDirectory -File | Where-Object {
    $_.Name -like "OpenKh.Launcher.*" -or
    $_.Name -like "OpenKh.Tools.ModsManager.*"
}

foreach ($duplicateFile in $duplicateApplicationFiles) {
    Remove-Item -LiteralPath $duplicateFile.FullName
}
