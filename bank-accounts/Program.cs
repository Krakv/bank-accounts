using bank_accounts.Features.Transactions;
using bank_accounts.Infrastructure.Repository;
using bank_accounts.PipelineBehaviors;
using bank_accounts.Services.CurrencyService;
using bank_accounts.Services.AccrueInterestService;
using bank_accounts.Services.VerificationService;
using FluentValidation;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using bank_accounts;


var builder = WebApplication.CreateBuilder(args);

var keycloakConfig = builder.Configuration.GetSection("Keycloak");

builder.Services.AddControllers();
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = $"{keycloakConfig["server-url"]}realms/{keycloakConfig["realm"]}";
            options.Audience = keycloakConfig["resource"];
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        });
}
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Account Service API",
        Version = "v1",
        Description = """
                      Данные для авторизации:<br><br>
                      Логин - user<br>
                      Пароль - password<br>
                      Client secret - "aNDf8kbjdZsV0r82a4TsD8PoKYDAE1aQ"<br>
                      Client ID - account
                      """
    });

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{keycloakConfig["auth-server-url"]}realms/{keycloakConfig["realm"]}/protocol/openid-connect/auth"),
                TokenUrl = new Uri($"{keycloakConfig["auth-server-url"]}realms/{keycloakConfig["realm"]}/protocol/openid-connect/token")
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            ["openid", "profile", "email"]
        }
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddOpenApi();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAccrueInterestService, AccrueInterestService>();
builder.Services.AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddSingleton<IVerificationService, StubVerificationService>();
builder.Services.AddSingleton<ICurrencyService, StubCurrencyService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(options =>
    {
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireConnection"));
    }));

builder.Services.AddHangfireServer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(keycloakConfig["resource"]);
        options.OAuthClientSecret(keycloakConfig["credentials:secret"]);
        options.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new List<IDashboardAuthorizationFilter>
    {
        new AllowAllDashboardAuthorizationFilter()
    }
});

RecurringJob.AddOrUpdate<IAccrueInterestService>("accrue-deposit-interest", 
    x => x.AccrueInterestForAllAccountsAsync(),
    "0 0 * * *");

await app.RunAsync();

public partial class Program;