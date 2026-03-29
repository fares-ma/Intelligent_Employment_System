const fs = require('fs');

const filePath = 'IES.api/Controllers/JobApplicationController.cs';
let content = fs.readFileSync(filePath, 'utf-8');

// Fix 1: HasApplied
const oldHasApplied = `    public async Task<ActionResult<bool>> HasApplied(string candidateId, int jobPostId)
    {
        try
        {
            var result = await _service.HasAppliedAsync(candidateId, jobPostId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HasApplied");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }`;

const newHasApplied = `    public async Task<ActionResult<bool>> HasApplied(string candidateId, int jobPostId)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isRecruiter = User.IsInRole("Recruiter") || User.IsInRole("Admin");

            if (!isRecruiter && currentUserId != candidateId)
            {
                return Forbid();
            }

            var result = await _service.HasAppliedAsync(candidateId, jobPostId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HasApplied");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }`;

content = content.replace(oldHasApplied, newHasApplied);

// Fix 2: Roles (Candidate)
content = content.replace(
    /var userRole = User\.FindFirst\(ClaimTypes\.Role\)\?\.Value;\s*\/\/\s*Candidates can only view their own applications\s*if \(userRole == "Candidate" && currentUserId != candidateId\)/g,
    `// Candidates can only view their own applications\n            if (User.IsInRole("Candidate") && currentUserId != candidateId)`
);

fs.writeFileSync(filePath, content, 'utf-8');
console.log("Patched JobApplicationController.cs securely");
