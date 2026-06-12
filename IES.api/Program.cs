using System.Text;
using Microsoft.AspNetCore.Authentication;
using Domain.Contracts;
using Domain.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence.Data;
using Persistence.Repositories;
using Presentation.Middleware;
using IES.api.Authentication;
using IES.api.Extensions;
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

            builder.Services.Configure<AnonymousApiAuthOptions>(
                builder.Configuration.GetSection("AnonymousApi"));

            var allowAnonymousApi = builder.Configuration.GetValue("AllowAnonymousApi", false);

            // ── Authentication: JWT (normal) or anonymous bypass (demo / open API) ──
            if (allowAnonymousApi)
            {
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = AnonymousApiAuthenticationHandler.SchemeName;
                    options.DefaultChallengeScheme = AnonymousApiAuthenticationHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, AnonymousApiAuthenticationHandler>(
                    AnonymousApiAuthenticationHandler.SchemeName, _ => { });
            }
            else
            {
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
            }

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
                    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? new[] { "http://localhost:4200" };

                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Required for SignalR
                });
            });

            // ── SignalR ──
            builder.Services.AddSignalR();

            // ── Background Services ──
            builder.Services.AddHostedService<Services.Background.JobExpiryBackgroundService>();
            builder.Services.AddHostedService<Services.Background.TalentXScoringPollingService>();
            builder.Services.AddTalentXIntegration(builder.Configuration);

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
            builder.Services.AddScoped<IInviteCodeService, InviteCodeService>();
            builder.Services.AddScoped<IEducationService, EducationService>();
            builder.Services.AddScoped<IExperienceService, ExperienceService>();
            builder.Services.AddScoped<ISkillService, SkillService>();
            builder.Services.AddScoped<IJobPostingService, JobPostingService>();
            builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
            builder.Services.AddScoped<IInterviewService, InterviewService>();
            builder.Services.AddScoped<IAssessmentService, AssessmentService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<INotificationPusher, Infrastructure.Presentation.Services.SignalRNotificationPusher>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IMessageService, Core.Services.MessageService>();
            builder.Services.AddScoped<IActivityLogService, Core.Services.ActivityLogService>();
            builder.Services.AddScoped<IAnalyticsService, Core.Services.AnalyticsService>();

            // ── Configuration for FileStorage ──
            builder.Services.Configure<Shared.Configuration.FileStorageSettings>(
                builder.Configuration.GetSection("FileStorage"));

            // ── AutoMapper ──
            builder.Services.AddAutoMapper(_ => { }, typeof(AuthMappingProfile).Assembly);

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

          
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "IES API v1");
                    options.RoutePrefix = string.Empty; // Swagger UI at root
                });
          

            app.UseMiddleware<GlobalExceptionHandler>();

            // Serve profile pictures at the root path so the frontend can access them directly by filename
            var profilePicturesPath = Path.Combine(builder.Environment.ContentRootPath, "Uploads", "profile-pictures");
            if (!Directory.Exists(profilePicturesPath))
            {
                Directory.CreateDirectory(profilePicturesPath);
            }
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(profilePicturesPath),
                RequestPath = "" // Map to root
            });

            // Configure ForwardedHeaders for proxy environments (IIS, load balancers, etc.)
            var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
            var knownProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>();
            var forwardedHeadersOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            };

            var hasValidKnownProxy = false;

            if (knownProxies != null && knownProxies.Length > 0)
            {
                foreach (var proxy in knownProxies)
                {
                    if (System.Net.IPAddress.TryParse(proxy, out var ipAddress))
                    {
                        forwardedHeadersOptions.KnownProxies.Add(ipAddress);
                        hasValidKnownProxy = true;
                    }
                    else
                    {
                        startupLogger.LogWarning("Invalid forwarded proxy IP configured: {Proxy}", proxy);
                    }
                }
            }

            if (!hasValidKnownProxy)
            {
                startupLogger.LogWarning("No valid forwarded proxies configured. Falling back to loopback addresses only.");
                forwardedHeadersOptions.KnownProxies.Add(System.Net.IPAddress.Loopback);
                forwardedHeadersOptions.KnownProxies.Add(System.Net.IPAddress.IPv6Loopback);
            }

            app.UseForwardedHeaders(forwardedHeadersOptions);

            // HTTPS redirection disabled — TalentX AI (Python) sends POST over HTTP
            // and most clients don't follow 307 redirects for POST, causing ConnectionResetError.
            // app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            // ── Health check endpoint (keep app warm on free hosting) ──
            app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
                .AllowAnonymous();

            // ── SignalR Hub endpoints ──
            // Uncomment when hub classes are created:
            app.MapHub<Presentation.Hubs.InterviewHub>("/hubs/interview");
            app.MapHub<Presentation.Hubs.NotificationHub>("/hubs/notifications");
            app.MapHub<Presentation.Hubs.ChatHub>("/hubs/chat");

            app.Run();
        }
    }
}
