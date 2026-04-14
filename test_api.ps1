$baseUrl = "http://localhost:5065/api"
$email = "test.candidate@testsprite.com"
$password = "P@ssw0rd123!"

Write-Host "Registering candidate..."
$registerBody = @{
    FirstName = "Test"
    LastName = "Candidate"
    Email = $email
    Password = $password
} | ConvertTo-Json
Invoke-RestMethod -Uri "$baseUrl/Auth/register" -Method Post -Body $registerBody -ContentType "application/json" -ErrorAction SilentlyContinue | Out-Null

Write-Host "Logging in..."
$loginBody = @{
    Email = $email
    Password = $password
} | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/Auth/login" -Method Post -Body $loginBody -ContentType "application/json"

$token = $loginResponse.token
$headers = @{
    Authorization = "Bearer $token"
}

Write-Host "Token received. Testing Delete Education with invalid ID..."
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/Education/invalid-id" -Method Delete -Headers $headers
    Write-Host "Result: $($response.StatusCode)"
} catch {
    Write-Host "Result: $_.Exception.Response.StatusCode"
}

Write-Host "Testing Delete Education with non-existent ID (99999)..."
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/Education/99999" -Method Delete -Headers $headers
    Write-Host "Result: $($response.StatusCode)"
} catch {
    Write-Host "Result: $_.Exception.Response.StatusCode"
}

Write-Host "Testing Skill Creation with invalid level (99)..."
$skillBody = @{
    SkillName = "C#"
    Level = 99
} | ConvertTo-Json
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/Skill" -Method Post -Body $skillBody -ContentType "application/json" -Headers $headers
    Write-Host "Result: $($response.StatusCode)"
} catch {
    Write-Host "Result: $_.Exception.Response.StatusCode"
}
