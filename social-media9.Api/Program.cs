using Amazon.DynamoDBv2;
using Microsoft.OpenApi.Models;
using Amazon.Extensions.NETCore.Setup;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// AWS SDK
builder.Services.AddAWSService<IAmazonDynamoDB>();

// Repository
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    var config = new AmazonDynamoDBConfig
    {
        ServiceURL = "http://localhost:8000"
    };
    return new AmazonDynamoDBClient(config);
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
