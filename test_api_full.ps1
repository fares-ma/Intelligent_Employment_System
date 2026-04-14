$baseUrl = "http://localhost:5065/api"
$email = "test.candidate3@testsprite.com"
$password = "P@ssw0rd__123XYZ!"

try {
    Write-Host "Registering candidate..."
    $registerBody = @{
        FirstName = "Test"
        LastName = "Candidate"
        Email = $email
        Password = $password
        PhoneNumber = "01000000000"
        Gender = "Male"
        DateOfBirth = "1990-01-01"
        UserType = "Candidate"
    } | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/Auth/register" -Method Post -Body $registerBody -ContentType "application/json" | Out-Null
    
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
    
    Write-Host "`nTesting Delete Education with non-existent ID (99999)..."
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl/Education/99999" -Method Delete -Headers $headers
        Write-Host "Result (Should be 404): $($response.StatusCode)"
    } catch {
        $status = $_.Exception.Response.StatusCode
        Write-Host "Result (Should be 404): $status"
    }

    Write-Host "`nTesting Delete Experience with non-existent ID (99999)..."
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl/Experience/99999" -Method Delete -Headers $headers
        Write-Host "Result (Should be 404): $($response.StatusCode)"
    } catch {
        $status = $_.Exception.Response.StatusCode
        Write-Host "Result (Should be 404): $status"
    }

    Write-Host "`nTesting Skill Creation with invalid level (99)..."
    $skillBody = @{
        SkillName = "C#"
        Level = 99
    } | ConvertTo-Json
    try {
        $response = Invoke-WebRequest -Uri "$baseUrl/Skill" -Method Post -Body $skillBody -ContentType "application/json" -Headers $headers
        Write-Host "Result (Should be 400): $($response.StatusCode)"
    } catch {
        $status = $_.Exception.Response.StatusCode
        Write-Host "Result (Should be 400): $status"
    }
    
} catch {
    Write-Host "Failed setup:"
    $streamReader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
    $ErrResp = $streamReader.ReadToEnd()
    Write-Host $ErrResp
}
