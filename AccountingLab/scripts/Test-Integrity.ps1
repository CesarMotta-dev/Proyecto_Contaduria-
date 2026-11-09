[CmdletBinding()]
param([switch]$Delivery)

$ErrorActionPreference = 'Stop'
$repositoryRoot = [IO.Path]::GetFullPath((Split-Path $PSScriptRoot -Parent))
$integrityIssues = [Collections.Generic.List[string]]::new()
$catalogPath = Join-Path $repositoryRoot 'docs/challenges/catalog.json'
$expectedRows = @(
    'DP-01|Creacional|Factory Method|factory-method',
    'DP-02|Creacional|Abstract Factory|abstract-factory',
    'DP-03|Creacional|Builder|builder',
    'DP-04|Creacional|Prototype|prototype',
    'DP-05|Creacional|Singleton|singleton',
    'DP-06|Estructural|Adapter|adapter',
    'DP-07|Estructural|Bridge|bridge',
    'DP-08|Estructural|Composite|composite',
    'DP-09|Estructural|Decorator|decorator',
    'DP-10|Estructural|Facade|facade',
    'DP-11|Estructural|Flyweight|flyweight',
    'DP-12|Estructural|Proxy|proxy',
    'DP-13|Comportamiento|Chain of Responsibility|chain-of-responsibility',
    'DP-14|Comportamiento|Command|command',
    'DP-15|Comportamiento|Iterator|iterator',
    'DP-16|Comportamiento|Mediator|mediator',
    'DP-17|Comportamiento|Memento|memento',
    'DP-18|Comportamiento|Observer|observer',
    'DP-19|Comportamiento|State|state',
    'DP-20|Comportamiento|Strategy|strategy',
    'DP-21|Comportamiento|Template Method|template-method',
    'DP-22|Comportamiento|Visitor|visitor'
)

function Get-ContainedFile {
    param([string]$RelativePath, [string]$Description)
    if ([string]::IsNullOrWhiteSpace($RelativePath) -or [IO.Path]::IsPathRooted($RelativePath)) {
        $integrityIssues.Add("$Description requiere una ruta relativa al repositorio.")
        return $null
    }
    $candidate = [IO.Path]::GetFullPath((Join-Path $repositoryRoot $RelativePath))
    $prefix = $repositoryRoot + [IO.Path]::DirectorySeparatorChar
    if (-not $candidate.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        $integrityIssues.Add("$Description sale del repositorio.")
        return $null
    }
    if (-not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
        $integrityIssues.Add("$Description no existe: $RelativePath")
        return $null
    }
    return $candidate
}

$requiredFiles = @(
    'AGENTS.md', 'README.md', 'AccountingLab.sln',
    'docs/ACTIVIDAD_ESTUDIANTES.md', 'docs/ARQUITECTURA.md', 'docs/CONTRATOS_API.md',
    'docs/CATALOGO_22_PATRONES.md', 'docs/MATRIZ_TRAZABILIDAD.md',
    'docs/CRITERIOS_EVALUACION.md', 'docs/POLITICA_USO_IA.md', 'docs/ENTREGA.md',
    'Dockerfile.api', 'docker-compose.yml', '.env.example', 'frontend/Dockerfile',
    'src/AccountingLab.Domain/AccountingLab.Domain.csproj',
    'src/AccountingLab.Application/AccountingLab.Application.csproj',
    'src/AccountingLab.Infrastructure/AccountingLab.Infrastructure.csproj',
    'src/AccountingLab.Api/AccountingLab.Api.csproj',
    'tests/AccountingLab.UnitTests/AccountingLab.UnitTests.csproj',
    'tests/AccountingLab.IntegrationTests/AccountingLab.IntegrationTests.csproj',
    'tests/AccountingLab.ArchitectureTests/AccountingLab.ArchitectureTests.csproj',
    'tests/AccountingLab.PatternChallenges/AccountingLab.PatternChallenges.csproj'
)
foreach ($requiredFile in $requiredFiles) { $null = Get-ContainedFile -RelativePath $requiredFile -Description $requiredFile }

if (-not (Test-Path -LiteralPath $catalogPath -PathType Leaf)) {
    $integrityIssues.Add('No existe docs/challenges/catalog.json.')
}
else {
    $entries = @(Get-Content -LiteralPath $catalogPath -Raw | ConvertFrom-Json)
    if ($entries.Count -ne 22) { $integrityIssues.Add("El catálogo contiene $($entries.Count) entradas; se requieren 22.") }
    foreach ($group in ($entries | Group-Object id | Where-Object Count -ne 1)) { $integrityIssues.Add("ID duplicado: $($group.Name).") }
    foreach ($row in $expectedRows) {
        $expected = $row -split '\|'
        $matching = @($entries | Where-Object id -eq $expected[0])
        if ($matching.Count -ne 1) { $integrityIssues.Add("$($expected[0]): debe existir exactamente una entrada."); continue }
        $entry = $matching[0]
        if ($entry.family -ne $expected[1] -or $entry.patternName -ne $expected[2]) { $integrityIssues.Add("$($entry.id): patrón o familia no corresponden al inventario.") }
        $expectedReference = "https://refactoring.guru/es/design-patterns/$($expected[3])"
        if ($entry.referenceUrl -ne $expectedReference) { $integrityIssues.Add("$($entry.id): referencia distinta de la individual requerida.") }
        $source = Get-ContainedFile -RelativePath $entry.sourcePath -Description "$($entry.id) fuente"
        $test = Get-ContainedFile -RelativePath $entry.testPath -Description "$($entry.id) pruebas"
        $guide = Get-ContainedFile -RelativePath $entry.guidePath -Description "$($entry.id) guía"
        if ($source -and $Delivery) {
            $sourceText = Get-Content -LiteralPath $source -Raw
            if (-not $sourceText.Contains("TODO(STUDENT): $($entry.id)")) { $integrityIssues.Add("$($entry.id): falta el marcador docente inicial.") }
        }
        if ($test) {
            $testText = Get-Content -LiteralPath $test -Raw
            if (-not $testText.Contains('ChallengeId') -or -not $testText.Contains($entry.id)) { $integrityIssues.Add("$($entry.id): falta asociación explícita de pruebas mediante ChallengeId.") }
        }
        if ($guide) {
            $guideText = Get-Content -LiteralPath $guide -Raw
            if (-not $guideText.Contains($entry.id) -or -not $guideText.Contains($entry.referenceUrl) -or -not $guideText.Contains('AGENTS.md')) { $integrityIssues.Add("$($entry.id): la guía debe identificar el DP, la referencia y AGENTS.md.") }
        }
    }
    foreach ($family in @(@('Creacional', 5), @('Estructural', 7), @('Comportamiento', 10))) {
        $actualCount = @($entries | Where-Object family -eq $family[0]).Count
        if ($actualCount -ne $family[1]) { $integrityIssues.Add("La familia $($family[0]) tiene $actualCount entradas; se requieren $($family[1]).") }
    }
}

if ($integrityIssues.Count -gt 0) {
    Write-Host 'Integridad documental: INCUMPLIMIENTOS'
    foreach ($issue in $integrityIssues) { Write-Host "- $issue" }
    exit 1
}
Write-Host "Integridad documental: comprobada. Modo entrega docente: $([bool]$Delivery)."
Write-Host 'Esta comprobación no ejecuta pruebas ni acredita ausencia de soluciones, corrección funcional o comprensión. La revisión docente sigue siendo necesaria.'
exit 0
