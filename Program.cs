using System.Text;
using MarkerspaceFablabPlatform.Data;
using MarkerspaceFablabPlatform.Services;
using MarkerspaceFablabPlatform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using MarkerspaceFablabPlatform.Entitys;
using MarkerspaceFablabPlatform.Handlers;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.AspNetCore.Identity;
using MarkerspaceFablabPlatform.Helpers;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;


namespace MarkerspaceFablabPlatform;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            
            options.CreateSchemaReferenceId = typeInfo =>
                OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo) is null
                    ? null
                    : typeInfo.Type.FullName!.Replace("+", ".");
        });

        
        // Db Connections
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    
        // Services
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
        builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddScoped<TokenService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IUserService, UserService>();


        // fluent Validation
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        

    
        // JWT
        
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,             // iss claim'i bizim mi?
                    ValidateAudience = true,           // aud claim'i bizim mi?
                    ValidateLifetime = true,           // exp geçmiş mi?
                    ValidateIssuerSigningKey = true,   // imza anahtarla tutuyor mu?
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                    ClockSkew = TimeSpan.Zero,          // default 5 dk tolerans var, sıfırla
                    NameClaimType = "sub", 
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                };
            });
        
        builder.Services.AddAuthorization();



        
        // Exception 
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails(); // hadnler dönemze fallback fortmal

        builder.Host.UseSerilog((context, config) => config
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14));
        
        var app = builder.Build();

        app.UseSerilogRequestLogging();   
        app.UseExceptionHandler();

        
        app.UseAuthentication();   // ÖNCE kimlik  — sıra önemli!
        app.UseAuthorization();    
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();               
            app.MapScalarApiReference();    
        }

        app.MapControllers();

        // base admin oluşturma
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            SeedData.EnsureAdmin(db, hasher, app.Configuration);
        }

        app.Run();
    }
}