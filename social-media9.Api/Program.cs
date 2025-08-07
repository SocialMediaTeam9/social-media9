
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using social_media9.Api.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using social_media9.Api.Data;
using social_media9.Api.Commands;
using FluentValidation;
using social_media9.Api.Behaviors;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Security.Claims;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Nest;
using social_media9.Api.Services.Interfaces;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Repositories.Implementations;
using social_media9.Api.Services.Implementations;
using social_media9.Api.Configurations;
using social_media9.Api.Infrastructure.ActivityPub.Services;
using social_media9.Api.Domain.ActivityPub.Entities;

using Nest;
using DynamoDbSettings = social_media9.Api.Configurations.DynamoDbSettings;

//using social_media9.Api.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// === Elasticsearch ===
var esSettings = builder.Configuration.GetSection("ElasticsearchSettings").Get<ElasticsearchSettings>();
builder.Services.AddSingleton(esSettings); // Make settings available
var settings = new ConnectionSettings(new Uri(esSettings.Uri))
    .PrettyJson()
    .DefaultIndex(esSettings.UsersIndex); 
builder.Services.AddSingleton<IElasticClient>(new ElasticClient(settings));
builder.Services.AddScoped<ISearchRepository, ElasticsearchRepository>();

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

// === AWS S3 ===
// This registers the AWS S3 client with the DI container.
builder.Services.AddAWSService<IAmazonS3>();
// This registers your custom service for S3 interactions.
builder.Services.AddScoped<IS3StorageService, S3StorageService>();

// === Application Services ===
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddSingleton<ICryptoService, CryptoService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IActorStorageService, DynamoDbActorStorageService>();
builder.Services.AddSingleton<HttpSignatureService>();
builder.Services.AddTransient<WebFingerService>(); 


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

// MediatR registration
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Configure IAmazonDynamoDB based on environment
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
    {
        var config = new AmazonDynamoDBConfig
        {
            ServiceURL = "http://localhost:8000"
        };
        return new AmazonDynamoDBClient(config);
    });
}
else
{
    // Use default AWS credentials & endpoint
    builder.Services.AddAWSService<IAmazonDynamoDB>();
}


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

app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
});

var internalApi = app.MapGroup("/internal/v1");

app.Run();
