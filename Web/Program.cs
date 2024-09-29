using Application.Interfaces;
using Application.Services;
using Application.Services.RegistrationService;
using Application.Services.SearchDataService;
using Core.Entities;
using Core.Interfaces;
using Core.Services;
using Infrastructure.Data;
using Infrastructure.Repositorys;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;



// read from Db make sure data is not doublicated //

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // Add this line to register controllers

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IIbanGeneratorService, IbanGeneratorService>();
builder.Services.AddScoped<ISearchUserService, SearchUserService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddUserStore<UserStore<User,Role,ApplicationDbContext,Guid>>()
    .AddRoleStore<RoleStore<Role,ApplicationDbContext,Guid>>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
});

builder.Services.AddTransient<IJwtService, JwtService>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseHttpsRedirection();

// Add routing for controllers
app.UseRouting();
app.UseAuthorization();

// Map controllers
app.MapControllers(); // Add this line to map the controllers

app.Run();