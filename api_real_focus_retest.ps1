$base='http://localhost:5065'

function WebRes($method,$url,$headers,$body){
  try {
    if($body){
      $r=Invoke-WebRequest -UseBasicParsing -Method $method -Uri $url -Headers $headers -ContentType 'application/json' -Body $body -ErrorAction Stop
    }
    else {
      $r=Invoke-WebRequest -UseBasicParsing -Method $method -Uri $url -Headers $headers -ErrorAction Stop
    }
    return [pscustomobject]@{status=[int]$r.StatusCode; body=$r.Content}
  } catch {
    $s=if($_.Exception.Response){[int]$_.Exception.Response.StatusCode}else{-1}
    $b=''
    if($_.Exception.Response){
      $st=$_.Exception.Response.GetResponseStream()
      if($st){
        $rd=New-Object IO.StreamReader($st)
        $b=$rd.ReadToEnd()
        $rd.Close()
      }
    }
    return [pscustomobject]@{status=$s; body=$b}
  }
}

$seed=[guid]::NewGuid().ToString('N').Substring(0,8)

$cand=@{
  firstName='Skill'
  lastName='Cand'
  email="skillcand.$seed@example.com"
  password='Str0ng!Pass123'
  phoneNumber='+201011111111'
  gender='Male'
  dateOfBirth=(Get-Date).AddYears(-25).ToString('o')
  userType='Candidate'
}

$regC=Invoke-RestMethod -Uri "$base/api/Auth/register" -Method Post -ContentType 'application/json' -Body ($cand|ConvertTo-Json)
$hc=@{Authorization="Bearer $($regC.token)"}

$addSkill=WebRes 'POST' "$base/api/Skill" $hc (@{skillName='GoLang';level=2}|ConvertTo-Json)
$skillObj=$null
try{$skillObj=$addSkill.body|ConvertFrom-Json}catch{}
$skillId = if($skillObj -and $skillObj.id){$skillObj.id}else{'1'}
$getSkill=WebRes 'GET' "$base/api/Skill/$skillId" $hc $null
$getSkillInvalid=WebRes 'GET' "$base/api/Skill/999999" $hc $null

$comp=@{
  firstName='Assess'
  lastName='Owner'
  email="assessco.$seed@example.com"
  password='Str0ng!Pass123'
  phoneNumber='+201022222222'
  gender='Female'
  dateOfBirth=(Get-Date).AddYears(-30).ToString('o')
  companyName="AssessCo-$seed"
  taxNumber="TX-$seed"
  industry='Software'
  website='https://example.com'
}

$regComp=Invoke-RestMethod -Uri "$base/api/Auth/register/company" -Method Post -ContentType 'application/json' -Body ($comp|ConvertTo-Json)
$hr=@{Authorization="Bearer $($regComp.token)"}

$genAssess=WebRes 'POST' "$base/api/Assessments/job/1/generate" $hr '{}'
$assObj=$null
try{$assObj=$genAssess.body|ConvertFrom-Json}catch{}
$assId= if($assObj -and $assObj.id){$assObj.id}else{'1'}
$startAssessByRecruiter=WebRes 'POST' "$base/api/Assessments/$assId/start" $hr '{}'
$submitAssessByRecruiter=WebRes 'POST' "$base/api/Assessments/$assId/submit" $hr (@{answers=@()}|ConvertTo-Json)
$startAssessByCandidate=WebRes 'POST' "$base/api/Assessments/$assId/start" $hc '{}'

Write-Output "skill_add_status=$($addSkill.status) skill_id=$skillId"
Write-Output "skill_get_real_status=$($getSkill.status)"
Write-Output "skill_get_invalid_status=$($getSkillInvalid.status)"
Write-Output "assessment_generate_status=$($genAssess.status) assessment_id=$assId"
Write-Output "assessment_start_recruiter_status=$($startAssessByRecruiter.status)"
Write-Output "assessment_submit_recruiter_status=$($submitAssessByRecruiter.status)"
Write-Output "assessment_start_candidate_status=$($startAssessByCandidate.status)"
if($startAssessByCandidate.status -ge 500 -or $startAssessByRecruiter.status -ge 500){
  $sample = if($startAssessByCandidate.body){$startAssessByCandidate.body}else{$startAssessByRecruiter.body}
  if($sample){
    Write-Output ('assessment_error_sample=' + $sample.Substring(0,[Math]::Min(220,$sample.Length)))
  }
}
