const fs = require('fs');

let fp = 'IES.api/Program.cs';
let content = fs.readFileSync(fp, 'utf-8');

// Ensure swagger is always on
content = content.replace(
    /if \(app\.Environment\.IsDevelopment\(\)\)\s*\{\s*app\.UseSwagger\(\);\s*app\.UseSwaggerUI\(options =>\s*\{\s*options\.SwaggerEndpoint\("\/swagger\/v1\/swagger\.json", "IES API v1"\);\s*options\.RoutePrefix = string\.Empty;\s*\}\);\s*\}/s,
    `// Always enable Swagger for MVP / testing on remote hosts like MonsterASP
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "IES API v1");
                options.RoutePrefix = string.Empty; // Swagger UI at root
            });`
);

// Add forwarded headers for IIS/MonsterASP
if (!content.includes("app.UseForwardedHeaders()")) {
    content = content.replace(
        "app.UseHttpsRedirection();",
        "app.UseForwardedHeaders(new ForwardedHeadersOptions\n            {\n                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto\n            });\n\n            app.UseHttpsRedirection();"
    );
}

fs.writeFileSync(fp, content, 'utf-8');
console.log("Patched Program.cs");
