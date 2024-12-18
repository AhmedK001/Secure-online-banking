using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Application.Interfaces;
using Application.Services;
using Application.Validators;
using AspNetCoreRateLimit;
using Core.Entities;
using Core.Interfaces;
using Core.Interfaces.IRepositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using Stripe.BillingPortal;
using BankAccountService = Application.Services.BankAccountService;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("SecureData.json", optional: true, reloadOnChange: true);
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// Services Registration
builder.Services.AddMemoryCache();
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// builder.Services.AddRateLimiter(o =>
// {
//     o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
//     o.AddPolicy("fixed", context =>
//         RateLimitPartition.GetFixedWindowLimiter(
//             partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
//             factory: _ => new FixedWindowRateLimiterOptions
//             {
//                 PermitLimit = 5,
//                 Window = TimeSpan.FromSeconds(60)
//             }));
// });

// builder.Services.AddRateLimiter(o => o.AddFixedWindowLimiter("fixed", o =>
// {
//     o.PermitLimit = 1;
//     o.Window = TimeSpan.FromSeconds(10);
//     o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//     o.QueueLimit = 1;
// }));


// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Enter your valid token to be Authenticated.",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});
builder.Services.AddSwaggerGen(options =>
{
    options.SchemaFilter<EnumSchemaFilter>();
    options.EnableAnnotations();
});


// Database and Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddUserStore<UserStore<User, Role, ApplicationDbContext, Guid>>()
    .AddRoleStore<RoleStore<Role, ApplicationDbContext, Guid>>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.SignIn.RequireConfirmedEmail = true;
});

builder.Services.Configure<IdentityOptions>(o =>
{
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.AllowedForNewUsers = true;
});

// Authentication and Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:ISSUER"],
        ValidAudience = builder.Configuration["Jwt:AUDIENCE"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:KEY"]!))
    };
});

// Dependency Injection
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISearchUserService, SearchUserService>();
builder.Services.AddScoped<IUpdatePassword, UpdatePasswordService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IOperationsRepository, OperationsRepository>();
builder.Services.AddScoped<IOperationService, OperationServices>();
builder.Services.AddScoped<ICardsService, CardsService>();
builder.Services.AddScoped<IClaimsService, ClaimsService>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddHttpClient<IStockService, StockService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IGenerateService, GenerateService>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPaymentsService, PaymentsService>();
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IValidate, Validate>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailBodyBuilder, EmailBodyBuilder>();
builder.Services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IAccountSettingsService, AccountSettingsService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IJwtService, JwtService>();

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();
var app = builder.Build();

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}");
    await next();
});
// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
//app.UseRateLimiter();
app.UseIpRateLimiting();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
