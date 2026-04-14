$base = 'http://localhost:5065'
$seed = [guid]::NewGuid().ToString('N').Substring(0,8)

$companyPayload = @{
  firstName = 'Comp'
  lastName = 'Admin'
  email = "companyadmin.$seed@example.com"
  password = 'Str0ng!Pass123'
  phoneNumber = '+201022223333'
  gender = 'Male'
  dateOfBirth = (Get-Date).AddYears(-30).ToString('o')
  companyName = "RealCo-$seed"
  taxNumber = "TX-$seed"
  industry = 'Software'
  website = 'https://example.com'
}

$reg = Invoke-RestMethod -Uri "$base/api/Auth/register/company" -Method Post -ContentType 'application/json' -Body ($companyPayload | ConvertTo-Json)
$token = $reg.token
$h = @{ Authorization = "Bearer $token" }

Write-Output "company_email=$($companyPayload.email)"

$authEndpoints = @(
  @{ m = 'POST'; p = '/api/Ai/extract-skills'; b = '{"text":"C# .NET"}' },
  @{ m = 'POST'; p = '/api/Ai/generate-assessment'; b = '{"jobTitle":"Backend Engineer","skills":["C#"]}' },
  @{ m = 'POST'; p = '/api/Assessments'; b = '{"jobPostId":1,"title":"Sample","description":"desc","durationMinutes":30}' },
  @{ m = 'POST'; p = '/api/Assessments/job/1/generate'; b = '{}' },
  @{ m = 'DELETE'; p = '/api/Candidates/resume/1'; b = $null },
  @{ m = 'POST'; p = '/api/Candidates/resume/1/generate-cv'; b = '{}' },
  @{ m = 'PUT'; p = '/api/Company/1'; b = '{"name":"Updated Co","industry":"Software","taxNumber":"TX-1"}' },
  @{ m = 'GET'; p = '/api/Company/1/active-invitations-count'; b = $null },
  @{ m = 'POST'; p = '/api/Company/1/transfer-admin'; b = '{"newAdminRecruiterId":"00000000-0000-0000-0000-000000000000"}' },
  @{ m = 'GET'; p = '/api/Dashboards/company/1'; b = $null },
  @{ m = 'PUT'; p = '/api/Interview/1'; b = '{"status":"Scheduled"}' },
  @{ m = 'DELETE'; p = '/api/Interview/1/cancel'; b = $null },
  @{ m = 'GET'; p = '/api/Interview/recruiter/interviews'; b = $null },
  @{ m = 'POST'; p = '/api/Interview/schedule'; b = '{"jobApplicationId":1,"scheduledAt":"2026-05-01T10:00:00Z","meetingLink":"https://meet.example.com/1"}' },
  @{ m = 'GET'; p = '/api/Interview/status/Scheduled'; b = $null },
  @{ m = 'PUT'; p = '/api/JobApplication/1/status'; b = '{"status":"Reviewed"}' },
  @{ m = 'GET'; p = '/api/JobApplication/job/1'; b = $null },
  @{ m = 'GET'; p = '/api/JobApplication/job/1/export'; b = $null },
  @{ m = 'GET'; p = '/api/JobApplication/job/1/status/Pending'; b = $null },
  @{ m = 'DELETE'; p = '/api/JobPosting/1'; b = $null }
)

foreach ($e in $authEndpoints) {
  try {
    if ($e.b) {
      $r = Invoke-WebRequest -UseBasicParsing -Uri ($base + $e.p) -Method $e.m -Headers $h -ContentType 'application/json' -Body $e.b -ErrorAction Stop
    } else {
      $r = Invoke-WebRequest -UseBasicParsing -Uri ($base + $e.p) -Method $e.m -Headers $h -ErrorAction Stop
    }
    $s = [int]$r.StatusCode
  }
  catch {
    $s = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { -1 }
  }

  Write-Output ("{0} {1} => {2}" -f $e.m, $e.p, $s)
}
