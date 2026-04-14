$base = 'http://localhost:5065'

function Invoke-Detailed {
  param(
    [string]$Method,
    [string]$Url,
    [hashtable]$Headers = $null,
    [string]$Body = $null
  )

  try {
    if ($Body) {
      $r = Invoke-WebRequest -UseBasicParsing -Method $Method -Uri $Url -Headers $Headers -ContentType 'application/json' -Body $Body -ErrorAction Stop
    } else {
      $r = Invoke-WebRequest -UseBasicParsing -Method $Method -Uri $Url -Headers $Headers -ErrorAction Stop
    }

    return [pscustomobject]@{
      status = [int]$r.StatusCode
      body = ($r.Content | Out-String)
    }
  }
  catch {
    $status = if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { -1 }
    $respBody = ''

    if ($_.Exception.Response) {
      $stream = $_.Exception.Response.GetResponseStream()
      if ($stream) {
        $reader = New-Object System.IO.StreamReader($stream)
        $respBody = $reader.ReadToEnd()
        $reader.Close()
      }
    }

    return [pscustomobject]@{
      status = $status
      body = $respBody
    }
  }
}

$seed = [guid]::NewGuid().ToString('N').Substring(0,8)
$email = "cand.$seed@example.com"
$pass = 'Str0ng!Pass123'

$reg = @{
  firstName = 'Api'
  lastName = 'User'
  email = $email
  password = $pass
  phoneNumber = '+201009998877'
  gender = 'Male'
  dateOfBirth = (Get-Date).AddYears(-26).ToString('o')
  userType = 'Candidate'
}

$regRes = Invoke-RestMethod -Uri "$base/api/Auth/register" -Method Post -ContentType 'application/json' -Body ($reg | ConvertTo-Json)
$h = @{ Authorization = "Bearer $($regRes.token)" }
$profile = Invoke-RestMethod -Uri "$base/api/Candidates/profile" -Headers $h -Method Get
$candidateId = $profile.id

$edu = @{ degree = 'BSc'; fieldOfStudy = 'Computer Science'; institution = 'Cairo University'; graduationYear = 2020 }
$exp = @{ jobTitle = 'Software Engineer'; company = 'Acme'; description = 'Backend APIs'; startDate = (Get-Date).AddYears(-3).ToString('o'); endDate = (Get-Date).AddYears(-1).ToString('o') }
$skill = @{ skillName = 'CSharp'; level = 2 }
$msg = @{ receiverId = $candidateId; content = 'test message'; jobApplicationId = $null }

$tests = @(
  @{ m = 'POST'; u = "$base/api/Education"; b = ($edu | ConvertTo-Json) },
  @{ m = 'GET'; u = "$base/api/Education"; b = $null },
  @{ m = 'POST'; u = "$base/api/Experience"; b = ($exp | ConvertTo-Json) },
  @{ m = 'GET'; u = "$base/api/Experience"; b = $null },
  @{ m = 'POST'; u = "$base/api/Skill"; b = ($skill | ConvertTo-Json) },
  @{ m = 'GET'; u = "$base/api/Skill"; b = $null },
  @{ m = 'POST'; u = "$base/api/Messages/send"; b = ($msg | ConvertTo-Json) },
  @{ m = 'DELETE'; u = "$base/api/JobApplication/1/withdraw"; b = $null }
)

Write-Output "candidate_email=$email"

foreach ($t in $tests) {
  $res = Invoke-Detailed -Method $t.m -Url $t.u -Headers $h -Body $t.b
  $snippet = if ($res.body) { $res.body.Substring(0, [Math]::Min(240, $res.body.Length)).Replace("`r", ' ').Replace("`n", ' ') } else { '' }
  Write-Output ("{0} {1} => {2} | {3}" -f $t.m, ($t.u -replace $base, ''), $res.status, $snippet)
}
