using System.Text;
using MakerspaceFablabPlatform.Data;
using MakerspaceFablabPlatform.Data.Interfaces;
using MakerspaceFablabPlatform.Data.Repositories;
using MakerspaceFablabPlatform.Services;
using MakerspaceFablabPlatform.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using MakerspaceFablabPlatform.Entities;
using MakerspaceFablabPlatform.Data.CachedRepositoties;
using MakerspaceFablabPlatform.Events;
using MakerspaceFablabPlatform.Events.Handlers;
using MakerspaceFablabPlatform.Handlers;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MakerspaceFablabPlatform.Entities.Enums;
using MakerspaceFablabPlatform.Helpers;
using MakerspaceFablabPlatform.Notifications;
using MakerspaceFablabPlatform.States.AnnouncementStates;
using MakerspaceFablabPlatform.Strategies.MembershipStrategies;
using Microsoft.Extensions.Caching.Memory;


namespace MakerspaceFablabPlatform;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        
        //Smtp Config 
        builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
        builder.Services.AddTransient<SmtpService>();
        
        // Add services to the container.
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        
        // Appdbcontext'i somut kullanMAmak için
        builder.Services.AddScoped<IApplicationDbContext>(sp => 
            sp.GetRequiredService<AppDbContext>());
        
        // For repository pattern 

        // Unit Of Work Pattern
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


        
        // Repositories
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<CategoryRepository>();
        builder.Services.AddScoped<ICategoryRepository>(sp =>
        {
            var inner = sp.GetRequiredService<CategoryRepository>();
            var cache = sp.GetRequiredService<IMemoryCache>();
            var logger = sp.GetRequiredService<ILogger<CachedCategoryRepository>>();
            var dbContext = sp.GetRequiredService<AppDbContext>();
            return new CachedCategoryRepository(inner, cache, logger, dbContext);
        });
        
        //builder.Services.AddScoped<IEventRepository, EventRepository>();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        
        
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        builder.Services.AddScoped<IEquipmentRentalRepository, EquipmentRentalRepository>();
        builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
        
        builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

        builder.Services.AddScoped<INotificationChannel, InAppNotificationChannel>();
        builder.Services.AddScoped<INotificationChannel, SmtpEmailChannel>();
        builder.Services.AddScoped<INotificationChannelFactory, NotificationChannelFactory>();
        
        builder.Services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        //Events
        builder.Services.AddScoped<IDomainEventHandler<AnnouncementPublishedEvent>, AnnouncementPublishedNotificationHandler>();
        builder.Services.AddScoped<IDomainEventHandler<EquipmentRelasedEvent>, EquipmentRelasedNotificationHandler>();

        // Auto Mapper for updaterequest => entity transaction
        builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

        // Services
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
        builder.Services.AddScoped<IEquipmentService, EquipmentService>();
        builder.Services.AddScoped<IPaymentService, PaymentService>();
        builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        

        //builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddScoped<TokenService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        builder.Services.AddScoped<IUserService, UserService>();


        // fluent Validation
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        // Factories
        builder.Services.AddScoped<IStateFactory, StateFactory>();
        builder.Services.AddScoped<States.EquipmentStates.IStateFactory, States.EquipmentStates.StateFactory>();
        

    
        // JWT
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

        //Strategy
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IMembershipStrategyFactory, MembershipStrategyFactory>();

        // Token sahibinin stratejisi. Başka bir kullanıcı adına işlem yapan
        // servisler bunu değil, IMembershipStrategyFactory'yi kullanmalı.
        builder.Services.AddScoped<IMembershipStrategy>(provider =>
        {
            var httpContext = provider.GetRequiredService<IHttpContextAccessor>();
            var userRepository = provider.GetRequiredService<IUserRepository>();
            var factory = provider.GetRequiredService<IMembershipStrategyFactory>();

            // Token'dan user ID al
            var userId = httpContext.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null && Guid.TryParse(userId, out var guidId))
            {
                // Database'den çek (Sync - .Result kullan)
                var user = userRepository.GetByIdAsync(guidId).Result;

                return factory.Create(user?.Status ?? MembershipStatus.Unknown);
            }

            return factory.Create(MembershipStatus.Unknown);
        });

        
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
        await using (var scope = app.Services.CreateAsyncScope())
        {
            var UoW = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            await SeedData.EnsureAdminAsync(UoW, hasher, app.Configuration);
        }

        await app.RunAsync();
    }
}