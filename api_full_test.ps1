$ErrorActionPreference = 'Stop'
$base = 'http://localhost:5065'
$swagger = Invoke-RestMethod "$base/swagger/v1/swagger.json" -TimeoutSec 30

function Resolve-Schema([object]$schema, [object]$allSchemas) {
  if ($null -eq $schema) { return $null }
  if ($schema.'$ref') {
    $name = ($schema.'$ref' -split '/')[-1]
    return $allSchemas.$name
  }
  return $schema
}

function New-SampleValue([object]$schema, [object]$allSchemas, [int]$depth = 0) {
  if ($depth -gt 4 -or $null -eq $schema) { return $null }
  $schema = Resolve-Schema $schema $allSchemas
  if ($null -eq $schema) { return $null }

  if ($schema.enum -and $schema.enum.Count -gt 0) { return $schema.enum[0] }

  switch ($schema.type) {
    'string' {
      switch ($schema.format) {
        'date-time' { return (Get-Date).AddYears(-25).ToString('o') }
        'date' { return (Get-Date).AddYears(-25).ToString('yyyy-MM-dd') }
        'email' { return "api.$([guid]::NewGuid().ToString('N').Substring(0,8))@example.com" }
        'uuid' { return ([guid]::NewGuid().ToString()) }
        default { return 'sample-text' }
      }
    }
    'integer' { return 1 }
    'number' { return 1 }
    'boolean' { return $true }
    'array' {
      $item = New-SampleValue $schema.items $allSchemas ($depth + 1)
      return @($item)
    }
    'object' {
      $obj = @{}
      if ($schema.properties) {
        foreach ($p in $schema.properties.PSObject.Properties) {
          $obj[$p.Name] = New-SampleValue $p.Value $allSchemas ($depth + 1)
        }
      }
      return $obj
    }
    default {
      if ($schema.properties) {
        $obj = @{}
        foreach ($p in $schema.properties.PSObject.Properties) {
          $obj[$p.Name] = New-SampleValue $p.Value $allSchemas ($depth + 1)
        }
        return $obj
      }
      return 'sample-text'
    }
  }
}

$seed = [guid]::NewGuid().ToString('N').Substring(0,8)
$candidateEmail = "candidate.$seed@example.com"
$candidatePassword = 'Str0ng!Pass123'
$registerPayload = @{
  firstName = 'API'
  lastName = 'Tester'
  email = $candidateEmail
  password = $candidatePassword
  phoneNumber = '+201001112233'
  gender = 'Male'
  dateOfBirth = (Get-Date).AddYears(-24).ToString('o')
  userType = 'Candidate'
}

$regResp = Invoke-RestMethod -Uri "$base/api/Auth/register" -Method Post -ContentType 'application/json' -Body ($registerPayload | ConvertTo-Json)
$token = $regResp.token
$headers = @{ Authorization = "Bearer $token" }

$profile = $null
try {
  $profile = Invoke-RestMethod -Uri "$base/api/Candidates/profile" -Headers $headers -Method Get
} catch {}
$candidateId = if ($profile -and $profile.id) { $profile.id } else { '1' }

$results = New-Object System.Collections.Generic.List[object]

foreach ($pathProp in $swagger.paths.PSObject.Properties) {
  $rawPath = $pathProp.Name

  foreach ($opProp in $pathProp.Value.PSObject.Properties) {
    $method = $opProp.Name.ToUpper()
    $op = $opProp.Value

    $urlPath = $rawPath
    $urlPath = $urlPath -replace '\{id\}', '1'
    $urlPath = $urlPath -replace '\{candidateId\}', [uri]::EscapeDataString("$candidateId")
    $urlPath = $urlPath -replace '\{companyId\}', '1'
    $urlPath = $urlPath -replace '\{jobPostId\}', '1'
    $urlPath = $urlPath -replace '\{jobId\}', '1'
    $urlPath = $urlPath -replace '\{assessmentId\}', '1'
    $urlPath = $urlPath -replace '\{interviewId\}', '1'
    $urlPath = $urlPath -replace '\{applicationId\}', '1'
    $urlPath = $urlPath -replace '\{resumeId\}', '1'
    $url = "$base$urlPath"

    $status = -1
    $message = ''

    try {
      $callHeaders = @{}
      if ($rawPath -notlike '/api/Auth/*' -or $rawPath -eq '/api/Auth/change-password') {
        $callHeaders = $headers
      }

      $hasBody = $method -in @('POST','PUT','PATCH')
      $contentType = $null
      $jsonBody = $null

      if ($hasBody -and $op.requestBody -and $op.requestBody.content) {
        if ($op.requestBody.content.'application/json') {
          $contentType = 'application/json'
          $schema = $op.requestBody.content.'application/json'.schema
          $sample = New-SampleValue $schema $swagger.components.schemas

          if ($rawPath -eq '/api/Auth/login') {
            $sample = @{ email = $candidateEmail; password = $candidatePassword }
          } elseif ($rawPath -eq '/api/Auth/register') {
            $sample = $registerPayload
          } elseif ($rawPath -eq '/api/Auth/register/company') {
            $sample = @{
              firstName = 'Comp'
              lastName = 'Owner'
              email = "company.$seed@example.com"
              password = 'Str0ng!Pass123'
              phoneNumber = '+201001112244'
              gender = 'Male'
              dateOfBirth = (Get-Date).AddYears(-30).ToString('o')
              companyName = "Test Company $seed"
              taxNumber = "TX-$seed"
              industry = 'Software'
              website = 'https://example.com'
            }
          } elseif ($rawPath -eq '/api/Auth/register/recruiter') {
            $sample = @{
              firstName = 'Rec'
              lastName = 'User'
              email = "recruiter.$seed@example.com"
              password = 'Str0ng!Pass123'
              phoneNumber = '+201001112255'
              gender = 'Female'
              dateOfBirth = (Get-Date).AddYears(-29).ToString('o')
              inviteCode = 'INVALID-CODE'
            }
          }

          $jsonBody = ($sample | ConvertTo-Json -Depth 12 -Compress)
        } elseif ($op.requestBody.content.'multipart/form-data') {
          $contentType = 'multipart/form-data'
        }
      }

      if ($method -in @('POST','PUT','PATCH') -and $contentType -eq 'multipart/form-data') {
        $tmpFile = Join-Path $env:TEMP "sample-resume-$seed.txt"
        Set-Content -Path $tmpFile -Value 'sample resume content for api test'
        $form = @{ file = Get-Item $tmpFile }
        $null = Invoke-RestMethod -Uri $url -Method $method -Headers $callHeaders -Form $form -TimeoutSec 20
        $status = 200
        $message = 'multipart success'
      } elseif ($method -in @('POST','PUT','PATCH') -and $jsonBody) {
        $resp = Invoke-WebRequest -UseBasicParsing -Uri $url -Method $method -Headers $callHeaders -ContentType 'application/json' -Body $jsonBody -TimeoutSec 20 -ErrorAction Stop
        $status = [int]$resp.StatusCode
        $message = if ($resp.Content) { $resp.Content.ToString().Substring(0, [Math]::Min(180, $resp.Content.Length)) } else { '' }
      } else {
        $resp = Invoke-WebRequest -UseBasicParsing -Uri $url -Method $method -Headers $callHeaders -TimeoutSec 20 -ErrorAction Stop
        $status = [int]$resp.StatusCode
        $message = if ($resp.Content) { $resp.Content.ToString().Substring(0, [Math]::Min(180, $resp.Content.Length)) } else { '' }
      }
    } catch {
      if ($_.Exception.Response) {
        $status = [int]$_.Exception.Response.StatusCode
      } else {
        $status = -1
      }
      $message = $_.Exception.Message
    }

    $kind = if ($status -ge 200 -and $status -lt 300) { 'PASS' } elseif ($status -eq 401 -or $status -eq 403) { 'AUTH' } elseif ($status -in 400,404,409,415) { 'BUSINESS' } elseif ($status -ge 500) { 'SERVER_FAIL' } else { 'OTHER' }

    $results.Add([pscustomobject]@{
      method = $method
      path = $rawPath
      status = $status
      kind = $kind
      note = $message
    }) | Out-Null
  }
}

$reportPath = Join-Path $env:TEMP "ies_api_full_report_$seed.json"
$results | ConvertTo-Json -Depth 6 | Set-Content -Path $reportPath

Write-Output "candidate_email=$candidateEmail"
Write-Output "total_operations=$($results.Count)"
$results | Group-Object kind | Sort-Object Name | ForEach-Object { Write-Output ("kind_{0}={1}" -f $_.Name, $_.Count) }
$results | Group-Object status | Sort-Object Name | ForEach-Object { Write-Output ("status_{0}={1}" -f $_.Name, $_.Count) }
Write-Output "report_path=$reportPath"
Write-Output "--- non-pass sample ---"
$results | Where-Object { $_.kind -ne 'PASS' } | Select-Object -First 30 | Format-Table -AutoSize | Out-String -Width 220
