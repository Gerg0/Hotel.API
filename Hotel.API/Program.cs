using System.Text;
using Hotel.API.Core.Configurations;
using Hotel.API.Core.Contracts;
using Hotel.API.Controllers;
using Hotel.API.Data;
using Hotel.API.Core.Middleware;
using Hotel.API.Core.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.OData;
using Microsoft.OpenApi.Models;
using System.ComponentModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("HotelDbConnection");

builder.Services.AddDbContext<HotelDbContext>(options => {
    options.UseSqlServer(connectionString);
});

// Configure microsoft identity
builder.Services.AddIdentityCore<ApiUser>()
.AddRoles<IdentityRole>()
.AddTokenProvider<DataProtectorTokenProvider<ApiUser>>("HotelApi")
.AddEntityFrameworkStores<HotelDbContext>().AddDefaultTokenProviders();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options=>{
    options.SwaggerDoc("v1", new OpenApiInfo{
        Title="Hotel Listing API", Version = "v1"
    });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme{
        Description=@"JWT Authorization header using the Bearer scheme.
        Enter 'Bearer' [space] and then your token in the text input below.
        Example: 'Bearer 123456abcdef",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme{
                Reference =new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme,
                },
                Scheme = "Oauth2",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()  
        }
    });
});



// Allow access for third party applications
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", b => b.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
});

// Configure api versioning
builder.Services.AddApiVersioning(options => {
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1,0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
    );
}).AddApiExplorer(options => {
    options.GroupNameFormat ="'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Configure serilog
builder.Host.UseSerilog((ctx, lc) =>lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

// Configure automapper
builder.Services.AddAutoMapper(typeof(MapperConfig));

// Inject services
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IHotelsRepository, HotelsRepository>();
builder.Services.AddScoped<IAuthManager, AuthManager>();

// Configure jwt authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // "Bearer"
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options=>{
    options.TokenValidationParameters = new TokenValidationParameters{
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
    };
});

// Configure caching
builder.Services.AddResponseCaching(options=>{
    options.MaximumBodySize = 1024;
    options.UseCaseSensitivePaths = true;
});

builder.Services.AddControllers().AddOData(options=>{
    options.Select().Filter().OrderBy();
});

builder.Services.AddHealthChecks()
.AddCheck<CustomHealthCheck>("Custom Health Check",
failureStatus:  HealthStatus.Degraded,
tags: new[] { "custom" }
).AddSqlServer(connectionString, tags: new[] { "database" }).AddDbContextCheck<HotelDbContext>(tags: new[] { "database" });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/healthcheck", new HealthCheckOptions{
    Predicate = healthcheck => healthcheck.Tags.Contains("custom"),
    ResultStatusCodes = {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,

    },
    ResponseWriter = WriteResponse
});
app.MapHealthChecks("/databasehealthcheck", new HealthCheckOptions{
    Predicate = healthcheck => healthcheck.Tags.Contains("database"),
    ResultStatusCodes = {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,

    },
    ResponseWriter = WriteResponse
});
static Task WriteResponse(HttpContext context, HealthReport healthReport){
    context.Response.ContentType ="application/json; charset=utf-8";
    var options = new JsonWriterOptions{Indented = true};
    using var memoryStream = new MemoryStream();
    using(var jsonWriter = new Utf8JsonWriter(memoryStream, options)){
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("status", healthReport.Status.ToString());
        jsonWriter.WriteStartObject("result");

        foreach (var healtReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject(healtReportEntry.Key);
            jsonWriter.WriteString("status", healtReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("description", healtReportEntry.Value.Description);
            jsonWriter.WriteStartObject("data");
            foreach (var item in healtReportEntry.Value.Data)
            {
                jsonWriter.WritePropertyName(item.Key);
                JsonSerializer.Serialize(jsonWriter, item.Value,item.Value?.GetType() ?? typeof(object));
            }   
            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }
        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();

    }
    return context.Response.WriteAsync(Encoding.UTF8.GetString(memoryStream.ToArray()));
}
app.MapHealthChecks("/health");
// Middleware for global exception handeling
app.UseMiddleware<ExceptionMiddleware>();

// Configure serilog to log all incoming requests and responses
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseResponseCaching();

app.Use(async (context, next) =>{
    context.Response.GetTypedHeaders().CacheControl =
        new CacheControlHeaderValue(){
            Public = true,
            MaxAge = TimeSpan.FromSeconds(10)
        };

    context.Response.Headers[HeaderNames.Vary] = 
        new string[] { "Accept-Encoding" };

    await next();
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

public class CustomHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var isHealty = true;
        if (isHealty)
        {
            return Task.FromResult(HealthCheckResult.Healthy("All systems are working"));
        }
        return Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus, "System unhealty"));
    }
}