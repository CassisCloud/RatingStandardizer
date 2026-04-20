param(
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'build-jellyfin-debug',
        'publish-jellyfin-release',
        'publish-jellyfin-compat-matrix',
        'deploy-jellyfin-debug',
        'deploy-jellyfin-release',
        'run-jellyfin',
        'stop-jellyfin',
        'restart-jellyfin',
        'open-jellyfin-log',
        'build-emby-debug',
        'publish-emby-release',
        'deploy-emby-debug',
        'deploy-emby-release',
        'run-emby',
        'stop-emby',
        'restart-emby',
        'open-emby-log',
        'launch-all')]
    [string]$Action,

    [string]$JellyfinVersion,

    [string]$EmbyVersion,

    [string[]]$JellyfinVersions
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ProjectRoot = Split-Path -Parent $PSScriptRoot
$CompatibilityMatrixVersions = if ($JellyfinVersions -and $JellyfinVersions.Count -gt 0) { $JellyfinVersions } else { @('10.11.0', '10.11.6', '10.11.8') }

$ServerProfiles = @{
    Jellyfin = @{
        ProjectFile = Join-Path $ProjectRoot 'RatingStandardizer.Jellyfin\RatingStandardizer.Jellyfin.csproj'
        PackageVersionProperty = 'JellyfinVersion'
        PackageVersionOverride = $JellyfinVersion
        ExePath = 'C:\Development\jellyfin\jellyfin\jellyfin.exe'
        WorkingDir = 'C:\Development\jellyfin\jellyfin'
        DataDir = 'C:\Development\jellyfin\Z2SFF-SRV58_Testy\data'
        CacheDir = 'C:\Development\jellyfin\Z2SFF-SRV58_Testy\cache'
        LogDir = 'C:\Development\jellyfin\Z2SFF-SRV58_Testy\data\log'
        ProcessName = 'jellyfin.exe'
        PublishRoot = Join-Path $ProjectRoot 'artifacts\publish\jellyfin'
        DistRoot = Join-Path $ProjectRoot 'artifacts\dist\jellyfin'
        UsePluginSubfolder = $true
    }
    Emby = @{
        ProjectFile = Join-Path $ProjectRoot 'RatingStandardizer.Emby\RatingStandardizer.Emby.csproj'
        PackageVersionProperty = 'EmbyVersion'
        PackageVersionOverride = $EmbyVersion
        ExePath = 'C:\Users\miyag\PApp\embyserver-win-x64-4.9.3.0\system\EmbyServer.exe'
        WorkingDir = 'C:\Users\miyag\PApp\embyserver-win-x64-4.9.3.0\system'
        DataDir = 'C:\Users\miyag\PApp\embyserver-win-x64-4.9.3.0\programdata'
        CacheDir = $null
        LogDir = 'C:\Users\miyag\PApp\embyserver-win-x64-4.9.3.0\programdata\logs'
        ProcessName = 'EmbyServer.exe'
        PublishRoot = Join-Path $ProjectRoot 'artifacts\publish\emby'
        DistRoot = Join-Path $ProjectRoot 'artifacts\dist\emby'
        UsePluginSubfolder = $false
    }
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed with exit code $LASTEXITCODE"
    }
}

function Get-XmlPropertyValue {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlElement[]]$PropertyGroups,
        [Parameter(Mandatory = $true)]
        [string]$PropertyName
    )

    $node = $PropertyGroups | Where-Object { $_.$PropertyName } | Select-Object -First 1
    if ($null -eq $node) {
        return $null
    }

    $value = $node.$PropertyName
    if ($value -is [System.Xml.XmlElement]) {
        return [string]$value.InnerText
    }

    return [string]$value
}

function Get-PackageVersion {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server
    )

    $profile = $ServerProfiles[$Server]
    if (-not [string]::IsNullOrWhiteSpace($profile.PackageVersionOverride)) {
        return $profile.PackageVersionOverride.Trim()
    }

    [xml]$projectXml = Get-Content -Path $profile.ProjectFile -Raw
    $propertyGroups = @($projectXml.Project.PropertyGroup)
    return Get-XmlPropertyValue -PropertyGroups $propertyGroups -PropertyName $profile.PackageVersionProperty
}

function Get-ProjectMetadataForServer {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server
    )

    [xml]$projectXml = Get-Content -Path $ServerProfiles[$Server].ProjectFile -Raw
    $propertyGroups = @($projectXml.Project.PropertyGroup)
    $projectDir = Split-Path -Parent $ServerProfiles[$Server].ProjectFile
    $propertyName = $ServerProfiles[$Server].PackageVersionProperty

    return @{
        ProjectFile = $ServerProfiles[$Server].ProjectFile
        ProjectDir = $projectDir
        AssemblyName = Get-XmlPropertyValue -PropertyGroups $propertyGroups -PropertyName 'AssemblyName'
        TargetFramework = Get-XmlPropertyValue -PropertyGroups $propertyGroups -PropertyName 'TargetFramework'
        Version = Get-XmlPropertyValue -PropertyGroups $propertyGroups -PropertyName 'Version'
        PackageVersion = Get-XmlPropertyValue -PropertyGroups $propertyGroups -PropertyName $propertyName
    }
}

function Get-PluginFolderName {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Metadata
    )

    return "Rating Standardizer_$($Metadata.Version)"
}

function Get-OutputDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Metadata,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration
    )

    return Join-Path $Metadata.ProjectDir "bin\$Configuration\$($Metadata.TargetFramework)"
}

function Get-BuildArguments {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server,
        [Parameter(Mandatory = $true)]
        [string]$Command,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Debug', 'Release')]
        [string]$Configuration,
        [string]$OutputDir,
        [string]$PackageVersion
    )

    $profile = $ServerProfiles[$Server]
    $arguments = @($Command, $profile.ProjectFile, '-c', $Configuration)
    if (-not [string]::IsNullOrWhiteSpace($OutputDir)) {
        $arguments += @('-o', $OutputDir)
    }

    if (-not [string]::IsNullOrWhiteSpace($PackageVersion)) {
        $arguments += "-p:$($profile.PackageVersionProperty)=$PackageVersion"
    }

    return $arguments
}

function Ensure-Directory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    New-Item -ItemType Directory -Force -Path $Path | Out-Null
}

function Reset-Directory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (Test-Path -LiteralPath $Path) {
        try {
            Remove-Item -LiteralPath $Path -Recurse -Force
        }
        catch [System.UnauthorizedAccessException], [System.IO.IOException] {
            throw "Failed to replace deployment directory '$Path'. The target is likely locked by a running server process. Stop the server first, then retry the deploy action."
        }
    }

    Ensure-Directory -Path $Path
}

function Get-PluginArtifactPaths {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDir,
        [Parameter(Mandatory = $true)]
        [string]$AssemblyName,
        [bool]$IncludePdb
    )

    $files = @()
    $files += Join-Path $SourceDir "$AssemblyName.dll"

    if ($IncludePdb) {
        $files += Join-Path $SourceDir "$AssemblyName.pdb"
    }

    $files = @($files | Where-Object { -not [string]::IsNullOrWhiteSpace($_) -and (Test-Path -LiteralPath $_) })
    if ($files.Count -eq 0) {
        throw "No plugin artifacts were found in $SourceDir"
    }

    return $files
}

function Copy-PluginArtifacts {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDir,
        [Parameter(Mandatory = $true)]
        [string]$DestinationDir,
        [Parameter(Mandatory = $true)]
        [string]$AssemblyName,
        [bool]$IncludePdb,
        [bool]$ResetDestination = $true
    )

    if ($ResetDestination) {
        Reset-Directory -Path $DestinationDir
    }
    else {
        Ensure-Directory -Path $DestinationDir
    }

    foreach ($file in (Get-PluginArtifactPaths -SourceDir $SourceDir -AssemblyName $AssemblyName -IncludePdb:$IncludePdb)) {
        $destinationFile = Join-Path $DestinationDir (Split-Path $file -Leaf)
        if (-not $ResetDestination -and (Test-Path -LiteralPath $destinationFile)) {
            Remove-Item -LiteralPath $destinationFile -Force
        }

        Copy-Item -LiteralPath $file -Destination $destinationFile -Force
    }
}

function Remove-OldPluginDeployments {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server,
        [Parameter(Mandatory = $true)]
        [string]$CurrentDeploymentDir
    )

    $pluginDeployRoot = Join-Path $ServerProfiles[$Server].DataDir 'plugins'
    if (-not (Test-Path -LiteralPath $pluginDeployRoot)) {
        return
    }

    if (-not $ServerProfiles[$Server].UsePluginSubfolder) {
        Get-ChildItem -Path $pluginDeployRoot -Directory |
            Where-Object { $_.Name -like 'Rating Standardizer_*' } |
            ForEach-Object {
                Remove-Item -LiteralPath $_.FullName -Recurse -Force
                Write-Host "Removed legacy deployment folder: $($_.FullName)"
            }

        return
    }

    Get-ChildItem -Path $pluginDeployRoot -Directory |
        Where-Object { $_.Name -like 'Rating Standardizer_*' -and $_.FullName -ne $CurrentDeploymentDir } |
        ForEach-Object {
            Remove-Item -LiteralPath $_.FullName -Recurse -Force
            Write-Host "Removed old deployment: $($_.FullName)"
        }
}

function Remove-OldPluginFiles {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server,
        [Parameter(Mandatory = $true)]
        [string]$AssemblyName
    )

    $pluginDeployRoot = Join-Path $ServerProfiles[$Server].DataDir 'plugins'
    if (-not (Test-Path -LiteralPath $pluginDeployRoot)) {
        return
    }

    $patterns = @(
        "$AssemblyName.dll",
        "$AssemblyName.pdb",
        "$AssemblyName.deps.json",
        'RatingStandardizer.Core.dll',
        'RatingStandardizer.Core.pdb',
        'RatingStandardizer.Core.deps.json',
        'ARatingStandardizer.Core.dll',
        'ARatingStandardizer.Core.pdb',
        'ARatingStandardizer.Core.deps.json'
    )

    foreach ($pattern in $patterns) {
        Get-ChildItem -Path $pluginDeployRoot -Filter $pattern -File -ErrorAction SilentlyContinue |
            ForEach-Object {
                Remove-Item -LiteralPath $_.FullName -Force
                Write-Host "Removed old plugin artifact: $($_.FullName)"
            }
    }
}

function Build-Debug {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server,
        [string]$PackageVersion
    )

    Invoke-DotNet -Arguments (Get-BuildArguments -Server $Server -Command 'build' -Configuration 'Debug' -PackageVersion $PackageVersion)
    if (-not [string]::IsNullOrWhiteSpace($PackageVersion)) {
        Write-Host "Built Debug for $Server against package version $PackageVersion"
        return
    }

    Write-Host "Built Debug for $Server"
}

function Publish-Release {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server,
        [string]$PackageVersion
    )

    $profile = $ServerProfiles[$Server]
    $metadata = Get-ProjectMetadataForServer -Server $Server
    $pluginFolderName = Get-PluginFolderName -Metadata $metadata
    $publishDir = Join-Path $profile.PublishRoot "release\$pluginFolderName"
    $distDir = Join-Path $profile.DistRoot "release\$pluginFolderName"

    Reset-Directory -Path $publishDir
    Invoke-DotNet -Arguments (Get-BuildArguments -Server $Server -Command 'publish' -Configuration 'Release' -OutputDir $publishDir -PackageVersion $PackageVersion)
    Copy-PluginArtifacts -SourceDir $publishDir -DestinationDir $distDir -AssemblyName $metadata.AssemblyName -IncludePdb:$false -ResetDestination:$true

    Write-Host "Release artifacts prepared in: $distDir"
}

function Publish-JellyfinCompatibilityMatrix {
    $profile = $ServerProfiles['Jellyfin']
    $metadata = Get-ProjectMetadataForServer -Server 'Jellyfin'
    $pluginFolderName = Get-PluginFolderName -Metadata $metadata

    foreach ($matrixVersion in $CompatibilityMatrixVersions) {
        $publishDir = Join-Path $ProjectRoot "artifacts\publish\jellyfin-$matrixVersion\$pluginFolderName"
        $distDir = Join-Path $ProjectRoot "artifacts\dist\jellyfin-$matrixVersion\$pluginFolderName"

        Reset-Directory -Path $publishDir
        Invoke-DotNet -Arguments (Get-BuildArguments -Server 'Jellyfin' -Command 'publish' -Configuration 'Release' -OutputDir $publishDir -PackageVersion $matrixVersion)
        Copy-PluginArtifacts -SourceDir $publishDir -DestinationDir $distDir -AssemblyName $metadata.AssemblyName -IncludePdb:$false -ResetDestination:$true
        Write-Host "Prepared compatibility build for Jellyfin $matrixVersion in: $distDir"
    }
}

function Get-ServerProcesses {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server
    )

    $profile = $ServerProfiles[$Server]
    $processes = Get-CimInstance Win32_Process -Filter "Name = '$($profile.ProcessName)'"

    if ($Server -eq 'Jellyfin') {
        return $processes |
            Where-Object {
                $_.ExecutablePath -eq $profile.ExePath -and
                $_.CommandLine -match [Regex]::Escape($profile.DataDir)
            }
    }

    return $processes |
        Where-Object {
            $_.ExecutablePath -eq $profile.ExePath
        }
}

function Stop-Server {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server
    )

    $processes = @(Get-ServerProcesses -Server $Server)
    if ($processes.Count -eq 0) {
        Write-Host "No matching $Server process is running."
        return
    }

    foreach ($process in $processes) {
        Stop-Process -Id $process.ProcessId -Force
        Write-Host "Stopped $Server process $($process.ProcessId)."
    }

    Start-Sleep -Seconds 2
}

function Run-Server {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server
    )

    $profile = $ServerProfiles[$Server]
    $argumentList = @()

    if ($Server -eq 'Jellyfin') {
        $argumentList = @('--datadir', $profile.DataDir, '--cachedir', $profile.CacheDir)
    }

    Start-Process -FilePath $profile.ExePath -WorkingDirectory $profile.WorkingDir -ArgumentList $argumentList
    Write-Host "Started $Server."
}

function Deploy-Debug {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server,
        [string]$PackageVersion
    )

    $profile = $ServerProfiles[$Server]
    $metadata = Get-ProjectMetadataForServer -Server $Server
    $pluginFolderName = Get-PluginFolderName -Metadata $metadata
    $debugOutputDir = Get-OutputDirectory -Metadata $metadata -Configuration 'Debug'
    $pluginDeployDir = if ($profile.UsePluginSubfolder) { Join-Path $profile.DataDir "plugins\$pluginFolderName" } else { Join-Path $profile.DataDir 'plugins' }

    Build-Debug -Server $Server -PackageVersion $PackageVersion
    Remove-OldPluginDeployments -Server $Server -CurrentDeploymentDir $pluginDeployDir
    Remove-OldPluginFiles -Server $Server -AssemblyName $metadata.AssemblyName
    Copy-PluginArtifacts -SourceDir $debugOutputDir -DestinationDir $pluginDeployDir -AssemblyName $metadata.AssemblyName -IncludePdb:$true -ResetDestination:$profile.UsePluginSubfolder
    Write-Host "Deployed debug plugin to: $pluginDeployDir"
}

function Deploy-Release {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server,
        [string]$PackageVersion
    )

    $profile = $ServerProfiles[$Server]
    $metadata = Get-ProjectMetadataForServer -Server $Server
    $pluginFolderName = Get-PluginFolderName -Metadata $metadata
    $distDir = Join-Path $profile.DistRoot "release\$pluginFolderName"
    $pluginDeployDir = if ($profile.UsePluginSubfolder) { Join-Path $profile.DataDir "plugins\$pluginFolderName" } else { Join-Path $profile.DataDir 'plugins' }

    Publish-Release -Server $Server -PackageVersion $PackageVersion
    Remove-OldPluginDeployments -Server $Server -CurrentDeploymentDir $pluginDeployDir
    Remove-OldPluginFiles -Server $Server -AssemblyName $metadata.AssemblyName
    Copy-PluginArtifacts -SourceDir $distDir -DestinationDir $pluginDeployDir -AssemblyName $metadata.AssemblyName -IncludePdb:$false -ResetDestination:$profile.UsePluginSubfolder
    Write-Host "Deployed release plugin to: $pluginDeployDir"
}

function Restart-Server {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server,
        [string]$PackageVersion
    )

    Stop-Server -Server $Server
    Deploy-Debug -Server $Server -PackageVersion $PackageVersion
    Run-Server -Server $Server
}

function Open-LatestLog {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Jellyfin', 'Emby')]
        [string]$Server
    )

    $logDir = $ServerProfiles[$Server].LogDir
    $latestLog = Get-ChildItem -Path $logDir -File | Sort-Object LastWriteTimeUtc -Descending | Select-Object -First 1

    if ($null -eq $latestLog) {
        throw "No log file was found in $logDir"
    }

    Start-Process -FilePath $latestLog.FullName
    Write-Host "Opened log: $($latestLog.FullName)"
}

switch ($Action) {
    'build-jellyfin-debug' {
        Build-Debug -Server 'Jellyfin' -PackageVersion (Get-PackageVersion -Server 'Jellyfin')
    }

    'publish-jellyfin-release' {
        Publish-Release -Server 'Jellyfin' -PackageVersion (Get-PackageVersion -Server 'Jellyfin')
    }

    'publish-jellyfin-compat-matrix' {
        Publish-JellyfinCompatibilityMatrix
    }

    'deploy-jellyfin-debug' {
        Deploy-Debug -Server 'Jellyfin' -PackageVersion (Get-PackageVersion -Server 'Jellyfin')
    }

    'deploy-jellyfin-release' {
        Deploy-Release -Server 'Jellyfin' -PackageVersion (Get-PackageVersion -Server 'Jellyfin')
    }

    'run-jellyfin' {
        Run-Server -Server 'Jellyfin'
    }

    'stop-jellyfin' {
        Stop-Server -Server 'Jellyfin'
    }

    'restart-jellyfin' {
        Restart-Server -Server 'Jellyfin' -PackageVersion (Get-PackageVersion -Server 'Jellyfin')
    }

    'open-jellyfin-log' {
        Open-LatestLog -Server 'Jellyfin'
    }

    'build-emby-debug' {
        Build-Debug -Server 'Emby' -PackageVersion (Get-PackageVersion -Server 'Emby')
    }

    'publish-emby-release' {
        Publish-Release -Server 'Emby' -PackageVersion (Get-PackageVersion -Server 'Emby')
    }

    'deploy-emby-debug' {
        Deploy-Debug -Server 'Emby' -PackageVersion (Get-PackageVersion -Server 'Emby')
    }

    'deploy-emby-release' {
        Deploy-Release -Server 'Emby' -PackageVersion (Get-PackageVersion -Server 'Emby')
    }

    'run-emby' {
        Run-Server -Server 'Emby'
    }

    'stop-emby' {
        Stop-Server -Server 'Emby'
    }

    'restart-emby' {
        Restart-Server -Server 'Emby' -PackageVersion (Get-PackageVersion -Server 'Emby')
    }

    'open-emby-log' {
        Open-LatestLog -Server 'Emby'
    }

    'launch-all' {
        Restart-Server -Server 'Jellyfin' -PackageVersion (Get-PackageVersion -Server 'Jellyfin')
        Restart-Server -Server 'Emby' -PackageVersion (Get-PackageVersion -Server 'Emby')
    }
}
