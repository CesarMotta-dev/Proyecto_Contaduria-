[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://127.0.0.1:5208',
    [string]$Email = 'docente@accountinglab.local',
    [string]$Password = $env:Demo__Password,
    [string]$ResultsPath,
    [switch]$IncludeEducationalRoutes
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($Password)) { throw 'Proporciona -Password o Demo__Password para la cuenta docente local; no se imprime en el informe.' }
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$BaseUrl = $BaseUrl.TrimEnd('/')
if (-not $ResultsPath) {
    $runId = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
    $ResultsPath = Join-Path $repositoryRoot "reports/teacher-delivery/$runId.json"
}
$ResultsPath = [IO.Path]::GetFullPath($ResultsPath)
New-Item -ItemType Directory -Path (Split-Path $ResultsPath -Parent) -Force | Out-Null
$catalog = @(Get-Content -LiteralPath (Join-Path $repositoryRoot 'docs/challenges/catalog.json') -Raw | ConvertFrom-Json -AsHashtable)
$login = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/auth/login" -ContentType 'application/json' -Body (@{ email = $Email; password = $Password } | ConvertTo-Json)
$headers = @{ Authorization = "Bearer $($login.accessToken)" }
$unique = [Guid]::NewGuid().ToString('N')
$company = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/companies" -Headers $headers -ContentType 'application/json' -Body (@{ name = "Revisión docente $unique"; taxId = "TEACHER-$unique"; currency = 'COP' } | ConvertTo-Json)
$headers['X-Company-Id'] = $company.id
$party = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/third-parties" -Headers $headers -ContentType 'application/json' -Body (@{ name = 'Tercero ficticio de revisión'; document = $unique; email = 'teacher-party@example.invalid' } | ConvertTo-Json)
$null = Invoke-RestMethod -Method Post -Uri "$BaseUrl/api/receivables" -Headers $headers -ContentType 'application/json' -Body (@{ thirdPartyId = $party.id; description = 'Obligación que debe conservarse'; amount = '100.0000'; dueDate = '2026-12-31' } | ConvertTo-Json)
$swagger = Invoke-RestMethod -Uri "$BaseUrl/swagger/v1/swagger.json" | ConvertTo-Json -Depth 100 | ConvertFrom-Json -AsHashtable

function New-TransportValue {
    param($Schema, [string]$PropertyName = '', [int]$Depth = 0)
    if ($Depth -gt 12) { throw 'El esquema excede la profundidad admitida para preparar una entrada de transporte.' }
    if ($Schema.Contains('$ref')) {
        $name = ($Schema['$ref'] -split '/')[-1]
        return New-TransportValue -Schema $swagger.components.schemas[$name] -PropertyName $PropertyName -Depth ($Depth + 1)
    }
    if ($Schema.Contains('allOf')) { return New-TransportValue -Schema $Schema.allOf[0] -PropertyName $PropertyName -Depth ($Depth + 1) }
    if ($Schema.Contains('enum') -and $Schema.enum.Count -gt 0) { return $Schema.enum[0] }
    switch ($Schema.type) {
        'array' { return ,@() }
        'boolean' { return $true }
        'integer' { if ($PropertyName -eq 'year') { return 2026 }; return 1 }
        'number' { return 1 }
        'string' {
            switch ($Schema.format) {
                'uuid' { if ($PropertyName -eq 'companyId') { return $company.id }; return [Guid]::NewGuid().ToString() }
                'date' { return '2026-10-23' }
                'date-time' { return '2026-10-23T12:00:00Z' }
                'decimal' { return '100.0000' }
                default { return 'Ficticio' }
            }
        }
        default {
            $value = [ordered]@{}
            if ($Schema.Contains('properties')) {
                foreach ($property in $Schema.properties.GetEnumerator()) {
                    $value[$property.Key] = New-TransportValue -Schema $property.Value -PropertyName $property.Key -Depth ($Depth + 1)
                }
            }
            return $value
        }
    }
}

function Get-BusinessSnapshot {
    $state = [ordered]@{}
    foreach ($resource in @('accounts', 'periods', 'third-parties', 'cost-centers', 'tax-rules', 'journal-entries', 'receivables', 'payables', 'payments', 'invoices', 'bank-statements', 'bank-reconciliation', 'budgets', 'approvals', 'notifications', 'audit')) {
        $state[$resource] = Invoke-RestMethod -Uri "$BaseUrl/api/$($resource)?pageSize=100" -Headers $headers
    }
    $state['summary'] = Invoke-RestMethod -Uri "$BaseUrl/api/reports/summary" -Headers $headers
    return ($state | ConvertTo-Json -Depth 50 -Compress)
}

function Get-StateHash {
    param([string]$State)
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($State)))
}

$results = [Collections.Generic.List[object]]::new()
$beforeState = Get-BusinessSnapshot
$beforeHash = Get-StateHash -State $beforeState
$checks = [Collections.Generic.List[object]]::new()
foreach ($entry in $catalog) {
    $operation = $swagger.paths[$entry.endpoint][$entry.httpMethod.ToLowerInvariant()]
    if ($null -eq $operation) { throw "OpenAPI no describe $($entry.httpMethod) $($entry.endpoint)." }
    $payload = [ordered]@{}
    if ($operation.Contains('requestBody')) {
        $content = $operation.requestBody.content
        $mediaType = if ($content.Contains('application/json')) { 'application/json' } else { @($content.Keys)[0] }
        $payload = New-TransportValue -Schema $content[$mediaType].schema
    }
    $checks.Add(@{ Entry = $entry; Route = $entry.endpoint; Payload = $payload; Kind = 'Module' })
    if ($IncludeEducationalRoutes) { $checks.Add(@{ Entry = $entry; Route = "/api/challenges/$($entry.id)/execute"; Payload = @{}; Kind = 'Educational' }) }
}

try {
    foreach ($check in $checks) {
        $response = Invoke-WebRequest -Method Post -Uri "$BaseUrl$($check.Route)" -Headers $headers -ContentType 'application/json' -Body (ConvertTo-Json -InputObject $check.Payload -Depth 50 -Compress) -SkipHttpErrorCheck
        $contentText = if ($response.Content -is [byte[]]) { [Text.Encoding]::UTF8.GetString($response.Content) } else { [string]$response.Content }
        $body = ConvertFrom-Json -InputObject $contentText -AsHashtable
        $afterState = Get-BusinessSnapshot
        $afterHash = Get-StateHash -State $afterState
        $unchanged = $afterHash -eq $beforeHash
        $accepted = [int]$response.StatusCode -eq 501 -and $body.challengeId -eq $check.Entry.id -and $body.patternName -eq $check.Entry.patternName -and $body.referenceUrl -eq $check.Entry.referenceUrl -and $unchanged
        $results.Add([ordered]@{
            id = $check.Entry.id
            kind = $check.Kind
            route = $check.Route
            status = [int]$response.StatusCode
            challengeId = $body.challengeId
            correlationId = $body.correlationId
            stateBefore = $beforeHash
            stateAfter = $afterHash
            businessStateUnchanged = $unchanged
            initialDeliveryCheckPassed = [bool]$accepted
            response = $body
        })
        Write-Host "$($check.Entry.id) $($check.Kind): HTTP $([int]$response.StatusCode), estado conservado=$unchanged, comprobación=$accepted"
    }
}
finally {
    [ordered]@{
        recordedAtUtc = [DateTime]::UtcNow.ToString('o')
        purpose = 'Comprobación exclusiva de la entrega docente inicial; no es una prueba funcional del alumno ni acredita los 66 casos académicos.'
        companyId = $company.id
        baseUrl = $BaseUrl
        businessStateBefore = ($beforeState | ConvertFrom-Json -AsHashtable)
        expectedChecks = $checks.Count
        completedChecks = $results.Count
        results = $results.ToArray()
    } | ConvertTo-Json -Depth 70 | Set-Content -LiteralPath $ResultsPath
    Write-Host "Informe docente: $ResultsPath"
}
if ($results.Count -ne $checks.Count -or @($results | Where-Object { -not $_.initialDeliveryCheckPassed }).Count -gt 0) { exit 1 }
exit 0
