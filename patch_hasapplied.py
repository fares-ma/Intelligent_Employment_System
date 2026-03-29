import re

file_path = 'IES.api/Controllers/JobApplicationController.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

new_method = """    public async Task<ActionResult<bool>> HasApplied(string candidateId, int jobPostId)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isRecruiter = User.IsInRole("Recruiter") || User.IsInRole("Admin");

            // Only allow candidates to check their own status, unless they are a recruiter/admin
            if (!isRecruiter && currentUserId != candidateId)
            {
                return Forbid("You can only check your own application status");
            }

            var result = await _service.HasAppliedAsync(candidateId, jobPostId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HasApplied");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }"""

# regex to replace the existing HasApplied method
pattern = re.compile(r'    public async Task<ActionResult<bool>> HasApplied.*?    }', re.DOTALL)
content = pattern.sub(new_method, content, count=1)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)

