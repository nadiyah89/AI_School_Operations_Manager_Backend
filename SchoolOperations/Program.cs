using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SchoolOperations.Data;
using SchoolOperations.Models;
using SchoolOperations.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<SchoolDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<AttendanceService>();

builder.Services.AddScoped<IAcademicPerformanceService,AcademicPerformanceService>();


// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    // JWT is the default authentication method
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Read JWT configuration from appsettings.json
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Validate that the token was created by our application
        ValidateIssuer = true,

        // Validate that the token is intended for our application
        ValidateAudience = true,

        // Validate the signing key
        ValidateIssuerSigningKey = true,

        // Validate token expiration
        ValidateLifetime = true,

        //JWT Issuer
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        //JWT Audience
        ValidAudience = builder.Configuration["Jwt:Audience"],

        //JWT Signing key
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                builder.Configuration["Jwt:Key"]!)),

        // Tell ASP.NET Core which claim represents the user's role
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // Define JWT Bearer authentication for Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    // Require the JWT token for authorized endpoints
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Seed default application roles and development attendance data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await RoleSeeder.SeedRolesAsync(services);

    var context = services.GetRequiredService<SchoolDbContext>();

    await AttendanceTestDataSeeder.SeedAttendanceAsync(context);
}

app.Run();
