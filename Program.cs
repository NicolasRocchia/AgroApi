using APIAgroConnect.Application.Interfaces;
using APIAgroConnect.Application.Services;
using APIAgroConnect.Common.Middleware;
using APIAgroConnect.Common.Security;
using APIAgroConnect.Infrastructure.Data;
using APIAgroConnect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "APIAgroConnect", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Peg�: Bearer {token}"
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

AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Console.WriteLine("UnhandledException: " + e.ExceptionObject);
};

TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    Console.WriteLine("UnobservedTaskException: " + e.Exception);
    e.SetObserved();
};


builder.Services.AddDbContext<AgroDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IPdfTextExtractor, PdfPigTextExtractor>();
builder.Services.AddScoped<IRecipePdfParser, RecipePdfParser>();
builder.Services.AddScoped<IRecipeImportService, RecipeImportService>();
builder.Services.AddScoped<PdfLotsExtractor>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IInsightsService, InsightsService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

// Validar que la clave JWT esté configurada (vía variable de entorno Jwt__Key o appsettings)
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.StartsWith("CHANGE_ME"))
    throw new InvalidOperationException(
        "La clave JWT no está configurada. Seteá la variable de entorno 'Jwt__Key' con una clave segura de al menos 32 caracteres.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = "unique_name",
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Rate limiting: proteger endpoints de auth contra fuerza bruta
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,                     // máx 10 intentos
                Window = TimeSpan.FromMinutes(5),     // cada 5 minutos
                QueueLimit = 0
            }));
});

// CORS: leer orígenes permitidos desde configuración (o variable de entorno Cors__Origins)
var allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? new[] { "https://localhost:7100" }; // fallback solo para desarrollo local

builder.Services.AddCors(o =>
{
    o.AddPolicy("default", p =>
        p.WithOrigins(allowedOrigins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});



builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50_000_000; // 50 MB
    o.ValueLengthLimit = int.MaxValue;
    o.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50_000_000; // 50 MB
});

var app = builder.Build();

app.UseCors("default");

// Middleware global de excepciones (atrapa errores no manejados en controllers)
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

await app.RunAsync();
