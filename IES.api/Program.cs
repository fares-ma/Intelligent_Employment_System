using System.Text;
using Domain.Contracts;
using Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence.Data;
using Persistence.Repositories;
using Presentation.Middleware;
using Services;
using Services.Abstractions;
using Services.Mapping;

namespace IES.api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── EF Core + SQL Server ──
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
                
                // Suppress warnings that cause migrations to fail
                options.ConfigureWarnings(w =>
                    w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

            // ── ASP.NET Identity ──
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // ── JWT Authentication ──
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"]!;

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero // No extra tolerance — token expires exactly at exp
                };

                options.Events = new JwtBearerEvents
                {
                    // ── jti blacklist check for logout invalidation ──
                    OnTokenValidated = context =>
                    {
                        var cache = context.HttpContext.RequestServices
                            .GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();

                        var jti = context.Principal?.FindFirst("jti")?.Value;
                        if (jti is not null && cache.TryGetValue($"blacklist_{jti}", out _))
                        {
                            context.Fail("Token has been revoked.");
                        }
                        return Task.CompletedTask;
                    },

                    // ── SignalR: extract token from query string ──
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // ── In-memory cache (for JWT blacklist) ──
            builder.Services.AddMemoryCache();

            // ── CORS ──
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });

                options.AddPolicy("AllowClient", policy =>
                {
                    policy.WithOrigins(
                              builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                              ?? ["http://localhost:4200"])
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Required for SignalR
                });
            });

            // ── SignalR ──
            builder.Services.AddSignalR();

            // ── Repository & UnitOfWork DI ──
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
            builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
            builder.Services.AddScoped<ISavedJobRepository, SavedJobRepository>();
            builder.Services.AddScoped<ISkillRepository, SkillRepository>();
            builder.Services.AddScoped<ICompanyInviteCodeRepository, CompanyInviteCodeRepository>();
            builder.Services.AddScoped<ICandidateEducationRepository, CandidateEducationRepository>();
            builder.Services.AddScoped<ICandidateExperienceRepository, CandidateExperienceRepository>();

            // ── Services DI ──
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ICompanyService, CompanyService>();
            builder.Services.AddScoped<ICandidateService, CandidateService>();
            builder.Services.AddScoped<IFileStorageService, FileStorageService>();
            builder.Services.AddScoped<IAiServiceClient, AiServiceClient>();
            builder.Services.AddScoped<IInviteCodeService, InviteCodeService>();
            builder.Services.AddScoped<IEducationService, EducationService>();
            builder.Services.AddScoped<IExperienceService, ExperienceService>();
            builder.Services.AddScoped<ISkillService, SkillService>();

            // ── Configuration for FileStorage ──
            builder.Services.Configure<Shared.Configuration.FileStorageSettings>(
                builder.Configuration.GetSection("FileStorage"));

            // ── AutoMapper ──
            builder.Services.AddAutoMapper(typeof(AuthMappingProfile), typeof(CandidateMappingProfile));

            // ── Controllers ──
            builder.Services.AddControllers()
                .AddApplicationPart(typeof(Presentation.Middleware.GlobalExceptionHandler).Assembly);

            // ── Swagger / OpenAPI ──
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
                {
                    Title = "IES API",
                    Version = "v1",
                    Description = "Intelligent Employment System — RESTful API"
                });

                // JWT Bearer token support in Swagger UI
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.ParameterLocation.Header,
                    Description = "Enter your JWT token"
                });

                options.AddSecurityRequirement(doc =>
                {
                    return new Microsoft.OpenApi.OpenApiSecurityRequirement
                    {
                        {
                            new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer"),
                            new List<string>()
                        }
                    };
                });
            });

            // ── File upload limits ──
            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 11_000_000; // ~10.5 MB
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 11_000_000;
            });

            var app = builder.Build();

            // ── Middleware pipeline ──

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "IES API v1");
                    options.RoutePrefix = string.Empty; // Swagger UI at root
                });
            }

            app.UseMiddleware<GlobalExceptionHandler>();

            app.UseHttpsRedirection();

            app.UseCors("AllowClient");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // ── SignalR Hub endpoints ──
            // Uncomment when hub classes are created:
            // app.MapHub<Presentation.Hubs.InterviewHub>("/hubs/interview");
            // app.MapHub<Presentation.Hubs.NotificationHub>("/hubs/notifications");

            app.Run();
        }
    }
}
