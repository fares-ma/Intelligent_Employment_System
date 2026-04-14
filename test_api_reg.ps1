$baseUrl = "http://localhost:5065/api"
$email = "test.candidate2@testsprite.com"
$password = "P@ssw0rd__123XYZ!"

try {
    Write-Host "Registering candidate..."
    $registerBody = @{
        FirstName = "Test"
        LastName = "Candidate"
        Email = $email
        Password = $password
    } | ConvertTo-Json
    Invoke-RestMethod -Uri "$baseUrl/Auth/register" -Method Post -Body $registerBody -ContentType "application/json"
} catch {
    Write-Host "Register failed:"
    $streamReader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
    $ErrResp = $streamReader.ReadToEnd()
    Write-Host $ErrResp
}
