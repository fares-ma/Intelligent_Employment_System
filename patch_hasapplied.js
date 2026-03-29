const fs = require('fs');

const filePath = 'IES.api/Controllers/JobApplicationController.cs';
let content = fs.readFileSync(filePath, 'utf-8');

const newMethod = `    public async Task<ActionResult<bool>> HasApplied(string candidateId, int jobPostId)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isRecruiter = User.IsInRole("Recruiter") || User.IsInRole("Admin");

            // Only allow candidates to check their own status, unless they are a recruiter/admin
            if (!isRecruiter && currentUserId != candidateId)
            {
                return Forbid(); // returning Forbid() is safer for auth issues
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

const regex = /    public async Task<ActionResult<bool>> HasApplied[\s\S]*?    }/m;
content = content.replace(regex, newMethod);

fs.writeFileSync(filePath, content, 'utf-8');
console.log("Patched JobApplicationController.cs");
