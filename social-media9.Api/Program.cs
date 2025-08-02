using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using social_media9.Api.Services;
using social_media9.Api.Services.Interfaces;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Repositories.Implementations;
using social_media9.Api.Services.Implementations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using social_media9.Api.Data;
using social_media9.Api.Commands;
using FluentValidation;
using social_media9.Api.Behaviors;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// === DynamoDB ===
builder.Services.Configure<DynamoDbSettings>(builder.Configuration.GetSection("DynamoDbSettings"));

builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    var config = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<DynamoDbSettings>>().Value;
    var clientConfig = new AmazonDynamoDBConfig
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(config.Region)
    };

    if (!string.IsNullOrEmpty(config.ServiceUrl))
        clientConfig.ServiceURL = config.ServiceUrl;

    return new AmazonDynamoDBClient(clientConfig);
});

builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();
builder.Services.AddScoped<DynamoDbClientFactory>();

// === Application Services ===
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddHttpClient();

// === MediatR & FluentValidation ===
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// === JWT Settings ===
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured.");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience not configured.");

// === Authentication ===
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // for dev
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["GoogleAuthSettings:ClientId"]
        ?? throw new InvalidOperationException("Google ClientId not configured.");
    googleOptions.ClientSecret = builder.Configuration["GoogleAuthSettings:ClientSecret"]
        ?? throw new InvalidOperationException("Google ClientSecret not configured.");
    googleOptions.CallbackPath = "/signin-google";
    googleOptions.SaveTokens = true;

    // Proper claim mapping
    googleOptions.Events.OnCreatingTicket = context =>
    {
        if (context.Identity != null)
        {
            var googleIdClaim = context.Identity.FindFirst(c => c.Type == "sub");
            if (googleIdClaim != null)
            {
                context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, googleIdClaim.Value));
            }
        }
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

// === Controllers & Swagger ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // If serving HTML/CSS/JS

// Protect Swagger from OAuth redirects
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/swagger"), appBuilder =>
{
    appBuilder.UseAuthentication();
    appBuilder.UseAuthorization();
});

app.MapControllers();

app.Run();
