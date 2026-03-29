const fs = require('fs');

// Fix JobApplicationController
let fp = 'IES.api/Controllers/JobApplicationController.cs';
let content = fs.readFileSync(fp, 'utf-8');
content = content.replace(
    /var userRole = User\.FindFirst\(ClaimTypes\.Role\)\?\.Value;\s*\/\/\s*Candidates can only view their own applications\s*if \(userRole == "Candidate" && currentUserId != candidateId\)/g,
    `// Candidates can only view their own applications\n            if (User.IsInRole("Candidate") && currentUserId != candidateId)`
);
fs.writeFileSync(fp, content, 'utf-8');

// Fix InterviewController
fp = 'IES.api/Controllers/InterviewController.cs';
content = fs.readFileSync(fp, 'utf-8');
content = content.replace(
    /var userRole = User\.FindFirst\(ClaimTypes\.Role\)\?\.Value;\s*\/\/\s*Candidates can only view their own interviews\s*if \(userRole == "Candidate" && currentUserId != candidateId\)/g,
    `// Candidates can only view their own interviews\n            if (User.IsInRole("Candidate") && currentUserId != candidateId)`
);
fs.writeFileSync(fp, content, 'utf-8');

console.log("Patched roles");
