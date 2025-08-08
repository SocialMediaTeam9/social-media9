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
using social_media9.Api.Services.Interfaces;
using social_media9.Api.Repositories.Interfaces;
using social_media9.Api.Repositories.Implementations;
using social_media9.Api.Services.Implementations;
using social_media9.Api.Configurations;
using social_media9.Api.Infrastructure.ActivityPub.Services;
using social_media9.Api.Domain.ActivityPub.Entities;

using Nest;
using DynamoDbSettings = social_media9.Api.Configurations.DynamoDbSettings;
using Amazon.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowPeerspace", policy =>
    {
        policy.WithOrigins("https://peerspace.online")
              .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
              .WithHeaders("Authorization", "Content-Type", "X-Requested-With", "Accept", "Origin")
              .AllowCredentials();
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
builder.Services.AddScoped<IFederationService, FederationService>();
// === Application Services ===
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<DynamoDbContext>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFollowRepository, FollowRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtGenerator, JwtGenerator>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddTransient<CommentService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddTransient<ICommentService, CommentService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddSingleton<ICryptoService, CryptoService>();
builder.Services.AddScoped<FollowService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IFederationService, FederationService>();
builder.Services.AddScoped<HttpSignatureService>();

builder.Services.AddScoped<ITimelineService, TimelineService>();

builder.Services.AddScoped<PostService>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<DynamoDbService>();
builder.Services.AddScoped<S3Service>();
builder.Services.AddScoped<ActivityPubService>();

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
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
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

builder.Services.AddHttpClient("FederationClient", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", $"Peerspace/1.0 (+https://{builder.Configuration["DomainName"]})");
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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = ctx =>
    {
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        ctx.Response.Redirect(ctx.RedirectUri);
        return Task.CompletedTask;
    };
});



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

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["Vary"] = "Origin";
        return Task.CompletedTask;
    });

    await next();
});
app.UseStaticFiles();
app.UseCors("AllowPeerspace");
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/swagger"), appBuilder =>
{
    appBuilder.UseAuthentication();
    appBuilder.UseAuthorization();
});

app.UseMiddleware<HttpSignatureValidationMiddleware>();

app.MapControllers();

app.MapGet("/health", () =>
{
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
});

app.Use(async (context, next) =>
{
    if (context.Request.Method == HttpMethods.Options)
    {
        context.Response.StatusCode = 204;
        await context.Response.CompleteAsync();
        return;
    }

    await next();
});

app.Run();
