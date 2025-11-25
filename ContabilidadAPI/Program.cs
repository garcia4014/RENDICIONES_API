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
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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

// OCR Services
builder.Services.AddOcrServices();

// SUNAT Services  
builder.Services.AddSunatServices();


var app = builder.Build();

// Configure the HTTP request pipeline.
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


