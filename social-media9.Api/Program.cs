// Program.cs

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
using Amazon.SQS;
using social_media9.Api.Services.DynamoDB;
using Microsoft.AspNetCore.Authorization;
using Nest;
using social_media9.Api.Services.Interfaces;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Repositories.Implementations;
using social_media9.Api.Services.Implementations;
using social_media9.Api.Configurations;
using DynamoDbSettings = social_media9.Api.Configurations.DynamoDbSettings;

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

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          var webappDomain = builder.Configuration["WebAppDomain"];
                          if (string.IsNullOrEmpty(webappDomain))
                          {

                              Console.WriteLine("WARNING: WebAppDomain is not configured. CORS will not be enabled.");
                              return;
                          }


                          policy.WithOrigins(webappDomain).AllowAnyHeader().AllowAnyMethod();
                      });
});


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

builder.Services.AddAWSService<IAmazonSQS>();

builder.Services.AddScoped<IDynamoDBContext>(sp =>
{
    var client = sp.GetRequiredService<IAmazonDynamoDB>();

    var config = new DynamoDBContextConfig
    {
        IgnoreNullValues = true,
    };

    return new DynamoDBContext(client, config);
});
builder.Services.AddScoped<DynamoDbClientFactory>();

builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IS3StorageService, S3StorageService>();

// === Application Services ===
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddSingleton<ICryptoService, CryptoService>();
builder.Services.AddScoped<FollowService>();

builder.Services.AddScoped<ITimelineService, TimelineService>();

builder.Services.AddScoped<PostService>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<DynamoDbService>();
builder.Services.AddScoped<S3Service>();

builder.Services.AddHostedService<SqsWorkerService>();


// === MediatR & FluentValidation ===
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthorizationHandler, InternalApiRequirementHandler>();



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

var gtsHookSecret = builder.Configuration["GTS_HOOK_SECRET"];
if (string.IsNullOrEmpty(gtsHookSecret))
{
    throw new InvalidOperationException("GTS_HOOK_SECRET is not configured. The application cannot start.");
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("InternalApi", policy =>
        policy.Requirements.Add(new InternalApiRequirement(gtsHookSecret)));

});

builder.Services.AddAuthorization();

builder.Services.AddHttpClient("FederationClient", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Peerspace/1.0");
});

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

app.UseMiddleware<HttpSignatureValidationMiddleware>();


// app.UseWhen(
//     context => context.Request.Path.ToString().Contains("/inbox"),
//     appBuilder => appBuilder.UseMiddleware<HttpSignatureValidationMiddleware>()
// );

app.MapControllers();

app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
});

app.Run();
