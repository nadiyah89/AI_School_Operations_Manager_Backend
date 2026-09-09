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
using SchoolOperations.AI.Tools;
using Google.GenAI;
using SchoolOperations.AI;
using SchoolOperations.AI.Orchestration;

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


var backendBaseUrl =
    builder.Configuration["AI:BackendBaseUrl"]
    ?? throw new InvalidOperationException(
        "AI:BackendBaseUrl is not configured.");

builder.Services.AddHttpClient(
    "AIBackendClient",
    client =>
    {
        client.BaseAddress =
            new Uri(backendBaseUrl);
    });


// =========================================================
// AI Tools
// =========================================================

builder.Services.AddScoped<SearchStudentsTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new SearchStudentsTool(httpClient);
    });


builder.Services.AddScoped<SearchParentsTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new SearchParentsTool(httpClient);
    });


builder.Services.AddScoped<SearchTeachersTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new SearchTeachersTool(httpClient);
    });


builder.Services.AddScoped<GetStudentDetailsTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new GetStudentDetailsTool(httpClient);
    });


builder.Services.AddScoped<GetParentsByStudentTool>(
serviceProvider =>
{
    var httpClientFactory =
        serviceProvider.GetRequiredService<IHttpClientFactory>();

    var httpClient =
        httpClientFactory.CreateClient(
            "AIBackendClient");

    return new GetParentsByStudentTool(httpClient);
});



// =========================================================
// Admissions AI Tools
// =========================================================

builder.Services.AddScoped<AdmissionSummaryTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new AdmissionSummaryTool(httpClient);
    });



builder.Services.AddScoped<AdmissionsByStatusTool>(
serviceProvider =>
{
    var httpClientFactory =
        serviceProvider.GetRequiredService<IHttpClientFactory>();

    var httpClient =
        httpClientFactory.CreateClient(
            "AIBackendClient");

    return new AdmissionsByStatusTool(httpClient);
});


builder.Services.AddScoped<AdmissionDetailsTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new AdmissionDetailsTool(httpClient);
    });


// =========================================================
// Fees AI Tools
// =========================================================

builder.Services.AddScoped<FeeSummaryTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new FeeSummaryTool(httpClient);
    });


builder.Services.AddScoped<OutstandingFeesTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new OutstandingFeesTool(httpClient);
    });


builder.Services.AddScoped<OverdueFeesTool>(
serviceProvider =>
{
    var httpClientFactory =
        serviceProvider.GetRequiredService<IHttpClientFactory>();

    var httpClient =
        httpClientFactory.CreateClient(
            "AIBackendClient");

    return new OverdueFeesTool(httpClient);
});



// =========================================================
// Academic Performance AI Tools
// =========================================================

builder.Services.AddScoped<PoorPerformanceTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new PoorPerformanceTool(httpClient);
    });


builder.Services.AddScoped<DecliningPerformanceTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new DecliningPerformanceTool(httpClient);
    });


// =========================================================
// Attendance AI Tools
// =========================================================

builder.Services.AddScoped<AttendanceTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new AttendanceTool(httpClient);
    });



// =========================================================
// Documents AI Tools
// =========================================================

builder.Services.AddScoped<DocumentCatalogTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new DocumentCatalogTool(httpClient);
    });


builder.Services.AddScoped<ReadDocumentTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new ReadDocumentTool(httpClient);
    });



// =========================================================
// Meetings AI Tools
// =========================================================

builder.Services.AddScoped<GetMyMeetingsTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new GetMyMeetingsTool(httpClient);
    });


builder.Services.AddScoped<GetMeetingDetailsTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new GetMeetingDetailsTool(httpClient);
    });


// =========================================================
// Notifications AI Tools
// =========================================================

builder.Services.AddScoped<CreateNotificationTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new CreateNotificationTool(httpClient);
    });


builder.Services.AddScoped<GetStudentNotificationsTool>(
    serviceProvider =>
    {
        var httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        var httpClient =
            httpClientFactory.CreateClient(
                "AIBackendClient");

        return new GetStudentNotificationsTool(httpClient);
    });



// =========================================================
// AI Tool Interface Registrations
// =========================================================

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<SearchStudentsTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<SearchParentsTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<SearchTeachersTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<GetStudentDetailsTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<GetParentsByStudentTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<AdmissionSummaryTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<AdmissionsByStatusTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<AdmissionDetailsTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<FeeSummaryTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<OutstandingFeesTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<OverdueFeesTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<PoorPerformanceTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<DecliningPerformanceTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<AttendanceTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<DocumentCatalogTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<ReadDocumentTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<GetMyMeetingsTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<GetMeetingDetailsTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<CreateNotificationTool>());

builder.Services.AddScoped<ITool>(
    serviceProvider =>
        serviceProvider.GetRequiredService<GetStudentNotificationsTool>());


// =========================================================
// Gemini AI
// =========================================================

var geminiApiKey =
    Environment.GetEnvironmentVariable(
        "GEMINI_API_KEY");

if (string.IsNullOrWhiteSpace(geminiApiKey))
{
    throw new InvalidOperationException(
        "GEMINI_API_KEY environment variable is not configured.");
}


builder.Services.AddSingleton(
    new Client(apiKey: geminiApiKey));


// =========================================================
// AI Tool Registry
// =========================================================

builder.Services.AddScoped<ToolRegistry>();


// =========================================================
// AI Orchestrator
// =========================================================

builder.Services.AddScoped<AIOrchestrator>();



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

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();

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

app.UseCors("ReactPolicy");

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
