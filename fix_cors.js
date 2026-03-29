const fs = require('fs');
const path = 'IES.api/Program.cs';

let content = fs.readFileSync(path, 'utf-8');

// The regex matches the specific block for the 'AllowClient' CORS policy
const corsRegex = /options\.AddPolicy\("AllowClient", policy =>\s*\{[\s\S]*?\}\);/m;

const newCors = `options.AddPolicy("AllowClient", policy =>
                {
                    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? new[] { "http://localhost:4200" };

                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Required for SignalR
                });`;

const updatedContent = content.replace(corsRegex, newCors);

// Verify that the replacement actually occurred
if (updatedContent === content) {
    console.error("ERROR: CORS policy regex did not match. File was not modified.");
    console.error("Expected pattern: options.AddPolicy(\"AllowClient\", policy => {...});");
    process.exit(1);
}

fs.writeFileSync(path, updatedContent, 'utf-8');
console.log("✓ CORS updated successfully with AllowedOrigins whitelist");
console.log("✓ SignalR credentials support maintained with explicit origins");

