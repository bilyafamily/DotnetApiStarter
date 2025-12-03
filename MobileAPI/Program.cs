using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MobileAPI.Data;
using MobileAPI.Helpers;
using MobileAPI.Models;
using MobileAPI.Repositories;
using MobileAPI.Repositories.IRepository;
using MobileAPI.Services;
using MobileAPI.Services.IService;
using MobileAPI.Utility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at 7
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin());
});

// DbContexts
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// JWT Settings for local Identity accounts
var configuration = builder.Configuration.GetSection("JwtSettings");

builder.Services.AddAuthentication()
    // 1. Local JWT (Scheme = "Bearer")
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtValidIssuer"],
            ValidAudience = configuration["JwtValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JwtSecret"] ?? ""))
        };
    })
    // 2. Azure AD (Custom Scheme = "BearerAAD")
    .AddMicrosoftIdentityWebApi(
        options =>
        {
            builder.Configuration.Bind(Constant.AZUREAD, options);
        },
        identityOptions =>
        {
            builder.Configuration.Bind(Constant.AZUREAD, identityOptions);
        },
        Constant.AZUREAD
    );


builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ISectorRepository, SectorRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddTransient<RoleSeederService>();

// Authorization
// builder.Services.AddAuthorization();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AnyAuthenticated", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "AzureAD")
            .RequireAuthenticatedUser();
    });
    
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Controllers
builder.Services.AddControllers().AddNewtonsoftJson();

// IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
// builder.Services.AddSingleton(mapper);
// builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddSingleton<IMapper>(provider =>
{
    var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
    var config = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<MappingConfig>();
    }, loggerFactory);
    
    config.AssertConfigurationIsValid();
    return config.CreateMapper();
});


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the bearer Authorization stirng as following 'Bearer token'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            }, new string[]{}
        }
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        },

    });
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1.0",//to override version display in the swagger,
        Contact = new OpenApiContact
        {
            Name = "BILYA API",
            Url = new Uri("https://support.bilya.gov.ng"),
            Email = "support@bilya.gov.ng"
        },
        TermsOfService = new Uri("https://support.bilya.gov.ng"),
        License = new OpenApiLicense
        {
            Name = "BILYA License",
            Url = new Uri("https://license.bilya.gov.ng")
        },
        Description = "BILYA API",
        Title = "BILYA API"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var serviceProvider = app.Services.CreateScope().ServiceProvider;
try
{
    var roleSeeder = serviceProvider.GetRequiredService<RoleSeederService>();
    await roleSeeder.SeedRolesAsync();
    
    // You can get these from configuration or use defaults
    var adminEmail = builder.Configuration["Admin:Email"] ?? "admin@mail.com";
    var adminPassword = builder.Configuration["Admin:Password"] ?? "Admin123!";
    
    await roleSeeder.SeedAdminUserAsync(adminEmail, adminPassword);
}
catch (Exception ex)
{
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while seeding roles and admin user.");
}

app.Run();


