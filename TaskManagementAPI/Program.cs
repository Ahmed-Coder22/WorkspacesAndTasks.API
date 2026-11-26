using Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;
using TaskManagementAPI.Mapping;
using TaskManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ProjectMapping>();
});

//Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("IpRateLimit", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString()
                 ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 80,
                Window = TimeSpan.FromSeconds(30),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });

    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;
});

// Add infrastructure services
builder.Services.infrastructureConfiguration(
    builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();

app.UseMiddleware<ExceptionsMiddleware>();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
