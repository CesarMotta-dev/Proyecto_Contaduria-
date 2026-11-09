[CmdletBinding()]
param(
    [ValidateSet('Base', 'Architecture', 'Academic', 'All')]
    [string]$Category = 'All',
    [ValidatePattern('^DP-(0[1-9]|1[0-9]|2[0-2])$')]
    [string]$ChallengeId,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    [string]$ResultsDirectory,
    [switch]$Delivery
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$localDotnet = Join-Path $repositoryRoot '.tools/dotnet/dotnet.exe'
if (-not (Test-Path -LiteralPath $localDotnet -PathType Leaf)) { $localDotnet = Join-Path $repositoryRoot '.tools/dotnet/dotnet' }
$dotnetCommand = if (Test-Path -LiteralPath $localDotnet -PathType Leaf) { $localDotnet } elseif (Get-Command dotnet -ErrorAction SilentlyContinue) { 'dotnet' } else { $null }
if ($ChallengeId -and $Category -ne 'Academic') {
    throw '-ChallengeId solo se utiliza con -Category Academic. Una selección no acredita la suite completa.'
}
if (-not $ResultsDirectory) {
    $runId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
    $ResultsDirectory = Join-Path $repositoryRoot "artifacts/test-results/$runId"
}
$ResultsDirectory = [IO.Path]::GetFullPath($ResultsDirectory)
New-Item -ItemType Directory -Path $ResultsDirectory -Force | Out-Null
$records = [Collections.Generic.List[object]]::new()
$firstFailure = 0

function Save-CommandResult {
    param([string]$Name, [string]$FilePath, [string[]]$Arguments, [string]$Directory)
    New-Item -ItemType Directory -Path $Directory -Force | Out-Null
    $logPath = Join-Path $Directory 'console.log'
    Write-Host "Ejecutando $Name. Salida: $logPath"
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        & $FilePath @Arguments 2>&1 | Tee-Object -FilePath $logPath | Out-Host
        $commandExit = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $previousPreference
    }
    if ($null -eq $commandExit) { $commandExit = 2 }
    return [int]$commandExit
}

function Read-TrxCounters {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $null }
    try {
        [xml]$document = Get-Content -LiteralPath $Path -Raw
        $counter = $document.SelectSingleNode("//*[local-name()='Counters']")
        if ($null -eq $counter) { return $null }
        return [ordered]@{
            total = [int]$counter.total
            executed = [int]$counter.executed
            passed = [int]$counter.passed
            failed = [int]$counter.failed
            notExecuted = [int]$counter.notExecuted
            error = [int]$counter.error
            aborted = [int]$counter.aborted
        }
    }
    catch { return $null }
}

$projectPlan = @(
    @{ Name = 'UnitTests'; Category = 'Base'; Project = 'AccountingLab.UnitTests' },
    @{ Name = 'IntegrationTests'; Category = 'Base'; Project = 'AccountingLab.IntegrationTests' },
    @{ Name = 'ArchitectureTests'; Category = 'Architecture'; Project = 'AccountingLab.ArchitectureTests' },
    @{ Name = 'PatternChallenges'; Category = 'Academic'; Project = 'AccountingLab.PatternChallenges' }
)

Push-Location $repositoryRoot
try {
    foreach ($item in $projectPlan) {
        if ($Category -ne 'All' -and $item.Category -ne $Category) { continue }
        $projectPath = Join-Path $repositoryRoot "tests/$($item.Project)/$($item.Project).csproj"
        $resultPath = Join-Path $ResultsDirectory $item.Name
        $environmentIssue = $null
        if (-not $dotnetCommand) { $environmentIssue = 'No está disponible dotnet ni el SDK local .tools/dotnet.' }
        elseif (-not (Test-Path -LiteralPath $projectPath -PathType Leaf)) { $environmentIssue = "No existe el proyecto $($item.Project)." }
        elseif ($item.Name -eq 'IntegrationTests' -and [string]::IsNullOrWhiteSpace($env:ConnectionStrings__AccountingLabTest)) {
            $environmentIssue = 'Falta ConnectionStrings__AccountingLabTest para la base MySQL desechable, separada de la demostración.'
        }
        if ($environmentIssue) {
            New-Item -ItemType Directory -Path $resultPath -Force | Out-Null
            $environmentIssue | Set-Content -LiteralPath (Join-Path $resultPath 'environment-error.txt')
            Write-Warning "$($item.Name): $environmentIssue"
            $records.Add([ordered]@{ name = $item.Name; category = $item.Category; status = 'NotRun'; exitCode = 2; reason = $environmentIssue; counters = $null })
            if ($firstFailure -eq 0) { $firstFailure = 2 }
            continue
        }
        $testArguments = @('test', $projectPath, '--configuration', $Configuration, '--logger', 'trx;LogFileName=results.trx', '--results-directory', $resultPath)
        if ($ChallengeId) { $testArguments += @('--filter', "ChallengeId=$ChallengeId") }
        $testExit = Save-CommandResult -Name $item.Name -FilePath $dotnetCommand -Arguments $testArguments -Directory $resultPath
        $counters = Read-TrxCounters -Path (Join-Path $resultPath 'results.trx')
        $status = if ($null -eq $counters) { 'NoTestReport' } elseif ($testExit -eq 0) { 'Passed' } else { 'Failed' }
        if ($null -eq $counters -and $testExit -eq 0) { $testExit = 2 }
        $records.Add([ordered]@{ name = $item.Name; category = $item.Category; status = $status; exitCode = $testExit; reason = $null; counters = $counters })
        if ($firstFailure -eq 0 -and $testExit -ne 0) { $firstFailure = $testExit }
    }

    if ($Category -eq 'All') {
        $integrityArguments = @('-NoProfile', '-File', (Join-Path $PSScriptRoot 'Test-Integrity.ps1'))
        if ($Delivery) { $integrityArguments += '-Delivery' }
        $integrityExit = Save-CommandResult -Name 'Integrity' -FilePath 'pwsh' -Arguments $integrityArguments -Directory (Join-Path $ResultsDirectory 'Integrity')
        $records.Add([ordered]@{ name = 'Integrity'; category = 'Integrity'; status = $(if ($integrityExit -eq 0) { 'Passed' } else { 'Failed' }); exitCode = $integrityExit; counters = $null })
        if ($firstFailure -eq 0 -and $integrityExit -ne 0) { $firstFailure = $integrityExit }

        $frontendResult = Join-Path $ResultsDirectory 'Frontend'
        if ((Get-Command npm -ErrorAction SilentlyContinue) -and (Test-Path -LiteralPath (Join-Path $repositoryRoot 'frontend/node_modules') -PathType Container)) {
            $frontendExit = Save-CommandResult -Name 'Frontend' -FilePath 'npm' -Arguments @('--prefix', 'frontend', 'test', '--', '--reporter=default', '--reporter=json', "--outputFile=$(Join-Path $frontendResult 'results.json')") -Directory $frontendResult
            $frontendCounters = $null
            $frontendReport = Join-Path $frontendResult 'results.json'
            if (Test-Path -LiteralPath $frontendReport -PathType Leaf) {
                $frontendData = Get-Content -LiteralPath $frontendReport -Raw | ConvertFrom-Json
                $frontendCounters = [ordered]@{ total = $frontendData.numTotalTests; passed = $frontendData.numPassedTests; failed = $frontendData.numFailedTests; notExecuted = $frontendData.numPendingTests }
            }
            if ($null -eq $frontendCounters -and $frontendExit -eq 0) { $frontendExit = 2 }
            $records.Add([ordered]@{ name = 'Frontend'; category = 'Frontend'; status = $(if ($frontendExit -eq 0) { 'Passed' } else { 'Failed' }); exitCode = $frontendExit; counters = $frontendCounters })
            if ($firstFailure -eq 0 -and $frontendExit -ne 0) { $firstFailure = $frontendExit }
        }
        else {
            $reason = 'Instala Node y ejecuta npm ci en frontend antes de la batería completa.'
            $records.Add([ordered]@{ name = 'Frontend'; category = 'Frontend'; status = 'NotRun'; exitCode = 2; reason = $reason; counters = $null })
            if ($firstFailure -eq 0) { $firstFailure = 2 }
        }
    }
}
finally { Pop-Location }

$summary = [ordered]@{
    recordedAtUtc = [DateTime]::UtcNow.ToString('o')
    selection = $Category
    challengeId = $ChallengeId
    deliveryIntegrity = [bool]$Delivery
    exitCode = $firstFailure
    results = $records.ToArray()
}
$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $ResultsDirectory 'summary.json')
$reportLines = [Collections.Generic.List[string]]::new()
$reportLines.Add('# Resultado real de ejecución')
$reportLines.Add('')
$reportLines.Add("Selección: $Category. Salida global: $firstFailure. No se transforma un fallo académico en aprobación.")
$reportLines.Add('')
$reportLines.Add('| Grupo | Estado | Código | Total | Aprobadas | Fallidas | No ejecutadas |')
$reportLines.Add('|---|---|---|---|---|---|---|')
foreach ($record in $records) {
    $counts = $record.counters
    if ($null -eq $counts) { $reportLines.Add("| $($record.name) | $($record.status) | $($record.exitCode) | — | — | — | — |") }
    else { $reportLines.Add("| $($record.name) | $($record.status) | $($record.exitCode) | $($counts.total) | $($counts.passed) | $($counts.failed) | $($counts.notExecuted) |") }
}
$reportLines.Add('')
$reportLines.Add('Los conteos proceden de los reportes generados. Un error de entorno o un fallo sin reporte no acredita los casos académicos. La revisión docente debe confirmar que cada fallo corresponde al DP evaluado.')
$reportLines | Set-Content -LiteralPath (Join-Path $ResultsDirectory 'SUMMARY.md')
Write-Host "Evidencia guardada en $ResultsDirectory"
exit $firstFailure
