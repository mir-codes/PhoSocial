using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PhoSocial.API.Hubs;
using PhoSocial.API.Repositories;
using PhoSocial.API.Services;
using PhoSocial.API.Middleware;
using PhoSocial.API.Utilities;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.SignalR;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services
builder.Services.AddControllers();

// Configure SignalR with custom user ID provider
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// DI - DB factory & repositories & services
builder.Services.AddSingleton<IDbConnectionFactory, SqlDbConnectionFactory>();
builder.Services.AddSingleton<ISanitizationService, HtmlSanitizationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IFeedRepository, FeedRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChatService, ChatService>();
// New v2 repositories and services (Dapper -> Stored Procedures / optimized schema)
builder.Services.AddScoped<PhoSocial.API.Repositories.V2.IPostRepositoryV2, PhoSocial.API.Repositories.V2.PostRepositoryV2>();
builder.Services.AddScoped<PhoSocial.API.Services.IPostServiceV2, PhoSocial.API.Services.PostServiceV2>();
// Register v2 chat
builder.Services.AddScoped<PhoSocial.API.Repositories.V2.IChatRepositoryV2, PhoSocial.API.Repositories.V2.ChatRepositoryV2>();
builder.Services.AddScoped<PhoSocial.API.Services.IChatServiceV2, PhoSocial.API.Services.ChatServiceV2>();
// Register v2 profile
builder.Services.AddScoped<PhoSocial.API.Repositories.V2.IProfileRepositoryV2, PhoSocial.API.Repositories.V2.ProfileRepositoryV2>();
builder.Services.AddScoped<PhoSocial.API.Services.IProfileServiceV2, PhoSocial.API.Services.ProfileServiceV2>();
// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PhoSocial API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token.\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
// JWT config
var jwtSection = config.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection.GetValue<string>("Key"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection.GetValue<string>("Issuer"),
        ValidAudience = jwtSection.GetValue<string>("Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
    // Allow token in query string for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"].FirstOrDefault();
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Add rate limiting for auth endpoints (5 attempts per minute per IP)
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*:/api/auth/*",
            Period = "1m",
            Limit = 5
        }
    };
});
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Optional: only if you're using cookies or auth headers
        });
});

// Hosted services
builder.Services.AddHostedService<PhoSocial.API.HostedServices.ExpireStoriesService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseIpRateLimiting();
app.UseRouting();
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowAngularClient");
app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger UI in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
