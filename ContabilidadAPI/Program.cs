using CapaDatos.ContabilidadAPI;
using CapaDatos.ContabilidadAPI.DAO.Implementation;
using CapaDatos.ContabilidadAPI.DAO.Implementation.Access;
using CapaDatos.ContabilidadAPI.DAO.Implementation.General;
using CapaDatos.ContabilidadAPI.DAO.Interfaces;
using CapaDatos.ContabilidadAPI.DAO.Interfaces.Access;
using CapaDatos.ContabilidadAPI.DAO.Interfaces.General;
using CapaDatos.ContabilidadAPI.Models.Access;
using CapaDatos.ContabilidadAPI.Models.General;
using CapaNegocio.ContabilidadAPI.Models;
using CapaNegocio.ContabilidadAPI.Models.DTO;
using CapaNegocio.ContabilidadAPI.Repository.Implementation;
using CapaNegocio.ContabilidadAPI.Repository.Implementation.Access;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces;
using CapaNegocio.ContabilidadAPI.Repository.Interfaces.Access;
using CapaNegocio.ContabilidadAPI.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IO;
using System;
using System.Text;
using Hangfire;
using Hangfire.SqlServer;
using ContabilidadAPI.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configurar directorio de logs con múltiples fallbacks
var currentDirectory = Directory.GetCurrentDirectory();
var logDirectory = string.Empty;
var logLocationAttempts = new List<string>();

var potentialLogDirectories = new List<string>
{
    Path.Combine(currentDirectory, "Logs"),
    Path.Combine(Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath(), "ContabilidadAPI", "Logs"),
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ContabilidadAPI", "Logs"),
    Path.Combine("C:\\", "Logs", "ContabilidadAPI"),
    Path.Combine(Path.GetTempPath(), "ContabilidadAPI")
};

foreach (var testDir in potentialLogDirectories)
{
    try
    {
        Directory.CreateDirectory(testDir);
        var testFile = Path.Combine(testDir, $"test-{DateTime.Now:yyyyMMddHHmmss}.txt");
        File.WriteAllText(testFile, $"Test log creation at {DateTime.Now} - Directory: {testDir}");
        File.Delete(testFile);

        logDirectory = testDir;
        logLocationAttempts.Add($"✓ SUCCESS: {testDir}");
        Console.WriteLine($"[Startup] Log directory set to: {logDirectory}");
        break;
    }
    catch (Exception ex)
    {
        logLocationAttempts.Add($"✗ FAILED: {testDir} - {ex.Message}");
        Console.WriteLine($"[Startup] Failed to create log directory {testDir}: {ex.Message}");
    }
}

if (string.IsNullOrEmpty(logDirectory))
{
    logDirectory = Path.GetTempPath();
    Console.WriteLine($"[Startup] WARNING: Using system temp as log directory: {logDirectory}");
}

Console.WriteLine($"[Startup] Log directory attempts:");
logLocationAttempts.ForEach(attempt => Console.WriteLine($"[Startup] {attempt}"));

// Configurar Serilog (configuración mínima inicial)
try
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(logDirectory, "startup-.txt"),
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    Console.WriteLine($"[Startup] Serilog configured successfully with directory: {logDirectory}");
}
catch (Exception ex)
{
    Console.WriteLine($"[Startup] Error configuring Serilog: {ex.Message}");
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .CreateLogger();
}

try
{
    Log.Information("=== APPLICATION STARTING ===");
    Log.Information("Current Directory: {CurrentDirectory}", currentDirectory);
    Log.Information("Log Directory: {LogDirectory}", logDirectory);
    Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);

    // Reconfigurar Serilog con configuración completa
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("ApplicationName", "ContabilidadAPI")
        .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(logDirectory, "app-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 20 * 1024 * 1024,
            rollOnFileSizeLimit: true,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1),
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{ApplicationName}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
            buffered: false
        )
        .CreateLogger();

    Log.Information("=== SERILOG RECONFIGURED SUCCESSFULLY ===");
    Log.Information("Final log directory: {LogDirectory}", logDirectory);
}
catch (Exception ex)
{
    Console.WriteLine($"[Startup] Error reconfiguring Serilog: {ex.Message}");
}

// Usar Serilog como provider de logging
builder.Host.UseSerilog();

builder.Services
       .AddDbContext<SvrendicionesContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("BDSVRENDICIONES")));

builder.Services
        .AddDbContext<AccessDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("BDMARCACIONES")));

builder.Services
        .AddDbContext<GeneralDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("BDGNRLMOVITECNICA")));


builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Configuración SUNAT
builder.Services.Configure<SunatConfigurationDto>(
    builder.Configuration.GetSection("SunatConfiguration"));

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(
                                "http://192.168.200.31:8081", 
                                "http://192.168.200.31:48081", // Agregando el puerto correcto
                                "http://192.168.200.31:48080", // Para pruebas locales del API
                                "http://192.168.230.31:48081", // IP que mencionaste en el error
                                "http://localhost:7120", 
                                "https://localhost:7120",
                                "https://192.168.100.34:8081",
                                "https://misviaticos.movitecnica.pe")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials(); // Habilita cookies o tokens en requests
                      });
});


builder.Services.AddControllersWithViews();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

var jwtSettings = builder.Configuration.GetSection("JwtSecurityToken");
var audiences = jwtSettings["audience"]?.Split(','); // M�ltiples audiencias

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["JwtSecurityToken:issuer"],
                //ValidAudience = builder.Configuration["JwtSecurityToken:audience"],
                ValidAudiences = audiences,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSecurityToken:key"]))
            };

        });

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Contabilidad API", Version = "v1" });
        
        c.AddSecurityDefinition("bearerAuth", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization using JWT bearer security scheme"

        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type=ReferenceType.SecurityScheme,
                        Id="bearerAuth",
                    },
                },
                new string[]{}
            }
        });
    });
builder.Services.AddAuthorization();

//DAO
builder.Services.AddScoped<ITipoGasto, TipoGastoImpl>();
builder.Services.AddScoped<IPoliticaTipoGastoPersona, PoliticaTipoGastoPersonaImpl>();
builder.Services.AddScoped<IAccess, IAccessImpl>();
builder.Services.AddScoped<IEmppla, IEmpplaImpl>();
builder.Services.AddScoped<ISviatico, SviaticoImpl>();
builder.Services.AddScoped<IComprobantePago, ComprobantePagoImpl>();
builder.Services.AddScoped<INotificacionDao, NotificacionDaoImpl>();
builder.Services.AddScoped<IPersonalDao, PersonalDaoImpl>();
builder.Services.AddScoped<IUsuarioTipoPersonaDao, UsuarioTipoPersonaDaoImpl>();

//REPOSITORY
builder.Services.AddScoped<ITipoGastoServices, TipoGastoServicesImpl>();
builder.Services.AddScoped<IAccessService, AccessServiceImpl>();
builder.Services.AddScoped<IGeneralService, GeneralServiceImpl>();
builder.Services.AddScoped<ISviaticoService, SviaticoServiceImpl>();
builder.Services.AddScoped<INotificacionService, NotificacionServiceImpl>();
builder.Services.AddScoped<IComprobantePagoService, ComprobantePagoServiceImpl>();
builder.Services.AddScoped<INotificacionesService, NotificacionesServiceImpl>();
builder.Services.AddScoped<IPersonalService, PersonalServiceImpl>();
builder.Services.AddScoped<IUsuarioTipoPersonaService, UsuarioTipoPersonaServiceImpl>();

// Configurar Hangfire con SQL Server
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("BDSVRENDICIONES"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Agregar el servidor de Hangfire
builder.Services.AddHangfireServer();

// OCR Services
builder.Services.AddOcrServices();

// SUNAT Services  
builder.Services.AddSunatServices();


var app = builder.Build();

// Configure the HTTP request pipeline.
// Serilog request logging (short info) and detailed request/response logging middleware
app.UseSerilogRequestLogging();

// Middleware to log request and response bodies for debugging (buffers streams)
app.Use(async (context, next) =>
{
    // Ignorar logs de polling de Hangfire Dashboard
    if (context.Request.Path.StartsWithSegments("/hangfire"))
    {
        await next();
        return;
    }

    var requestId = Guid.NewGuid().ToString("N")[0..8];

    Log.Information("[REQ-{RequestId}] {Method} {Path} from {RemoteIp}",
        requestId,
        context.Request.Method,
        context.Request.Path + context.Request.QueryString,
        context.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

    // Log headers
    Log.Information("[REQ-{RequestId}] Headers: {Headers}", requestId, context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));

    // Read and log request body (if any)
    string requestBody = string.Empty;
    try
    {
        context.Request.EnableBuffering();
        if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }
        if (!string.IsNullOrWhiteSpace(requestBody))
            Log.Information("[REQ-{RequestId}] Body: {RequestBody}", requestId, requestBody);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "[REQ-{RequestId}] Failed to read request body", requestId);
    }

    // Capture the response
    var originalBodyStream = context.Response.Body;
    await using var responseBody = new MemoryStream();
    context.Response.Body = responseBody;

    try
    {
        await next();

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        Log.Information("[REQ-{RequestId}] Response {StatusCode}: {ResponseBody}", requestId, context.Response.StatusCode, responseText);

        await responseBody.CopyToAsync(originalBodyStream);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "[REQ-{RequestId}] Exception in request pipeline", requestId);
        throw;
    }
    finally
    {
        context.Response.Body = originalBodyStream;
    }
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // En producción, habilitar Swagger solo si es necesario
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ContabilidadAPI v1");
        c.RoutePrefix = "swagger"; // Swagger en /swagger en lugar de raíz
    });
}

// Habilitar Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    DashboardTitle = "Contabilidad API - Background Jobs",
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Middleware para manejo de errores en producción
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// IMPORTANTE: El orden de middlewares es crucial
app.UseCors(MyAllowSpecificOrigins); // CORS debe ir antes que Authentication

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


