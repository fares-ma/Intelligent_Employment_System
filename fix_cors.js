const fs = require('fs');
const path = 'IES.api/Program.cs';

let content = fs.readFileSync(path, 'utf-8');

// The regex matches the specific block for the 'AllowClient' CORS policy
const corsRegex = /options\.AddPolicy\("AllowClient", policy =>\s*\{[\s\S]*?\}\);/m;

const newCors = `options.AddPolicy("AllowClient", policy =>
                {
                    // Using SetIsOriginAllowed(origin => true) instead of AllowAnyOrigin()
                    // This is a magic trick that allows ANY frontend URL while still allowing 
                    // SignalR credentials (which crashes if you just use AllowAnyOrigin).
                    policy.SetIsOriginAllowed(origin => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Required for SignalR
                });`;

content = content.replace(corsRegex, newCors);

fs.writeFileSync(path, content, 'utf-8');
console.log("CORS updated successfully for MVP deployment!");
