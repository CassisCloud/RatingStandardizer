param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $projectRoot 'RatingStandardizer.sln'
$legacySolutionPath = Join-Path $projectRoot 'rating-standardizer.sln'

$projects = @(
    @{
        Name = 'RatingStandardizer.Core'
        Framework = 'netstandard2.0'
    },
    @{
        Name = 'RatingStandardizer.Jellyfin'
        Framework = 'net9.0'
    },
    @{
        Name = 'RatingStandardizer.Emby'
        Framework = 'net8.0'
    }
)

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

function Ensure-ProjectInSolution {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectFile
    )

    $relativeProjectPath = [IO.Path]::GetRelativePath($projectRoot, $ProjectFile)
    if (Select-String -Path $solutionPath -Pattern ([Regex]::Escape($relativeProjectPath)) -Quiet) {
        return
    }

    Invoke-DotNet -Arguments @('sln', $solutionPath, 'add', $ProjectFile)
}


Invoke-DotNet -Arguments @('new', 'sln', '-o', $projectRoot, '-n', 'RatingStandardizer', '--force')

if (-not (Test-Path -LiteralPath $solutionPath) -and (Test-Path -LiteralPath $legacySolutionPath)) {
    Move-Item -LiteralPath $legacySolutionPath -Destination $solutionPath -Force
}

if (-not (Test-Path -LiteralPath $solutionPath)) {
    throw "Expected solution file was not created: $solutionPath"
}

foreach ($project in $projects) {
    $projectDir = Join-Path $projectRoot $project.Name
    if (-not (Test-Path -LiteralPath $projectDir)) {
        Invoke-DotNet -Arguments @('new', 'classlib', '-n', $project.Name, '-f', $project.Framework)
    }

    $projectFile = Join-Path $projectDir "$($project.Name).csproj"
    if (-not (Test-Path -LiteralPath $projectFile)) {
        throw "Expected project file was not created: $projectFile"
    }

    Ensure-ProjectInSolution -ProjectFile $projectFile
}
