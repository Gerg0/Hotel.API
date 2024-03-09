using System.Text;
using Hotel.API.Configurations;
using Hotel.API.Contracts;
using Hotel.API.Controllers;
using Hotel.API.Data;
using Hotel.API.Middleware;
using Hotel.API.Repository;
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
builder.Services.AddSwaggerGen();



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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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