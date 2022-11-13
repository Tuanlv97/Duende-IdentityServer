using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using TeduMicroservices.IDP.Common;
using TeduMicroservices.IDP.Infrastructure.Entities;
using TeduMicroservices.IDP.Infrastructure.Persistence;

namespace TeduMicroservices.IDP.Extensions
{
    public static class ServiceExtensions
    {

        internal static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection(nameof(SMTPEmailSetting))
                .Get<SMTPEmailSetting>();
            services.AddSingleton(emailSettings);

            return services;
        }

        internal static void AddAppConfiguration(this ConfigureHostBuilder host)
        {
            host.ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
            });
        }

        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            });
        }

        public static void ConfigureSerilog(this ConfigureHostBuilder host)
        {
            host.UseSerilog((context, configuration) =>
            {
                var elasticUri = context.Configuration.GetValue<string>("ElasticConfiguration:Uri");
                var username = context.Configuration.GetValue<string>("ElasticConfiguration:Username");
                var password = context.Configuration.GetValue<string>("ElasticConfiguration:Password");
                var applicationName = context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-");

                if (string.IsNullOrEmpty(elasticUri))
                    throw new Exception("ElasticConfiguration Uri is not configured.");

                configuration
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .WriteTo.Debug()
                    .WriteTo.Console().ReadFrom.Configuration(context.Configuration)
                    .WriteTo.Elasticsearch(
                        new ElasticsearchSinkOptions(new Uri(elasticUri))
                        {
                            IndexFormat =
                                $"{applicationName}-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                            AutoRegisterTemplate = true,
                            NumberOfShards = 2,
                            NumberOfReplicas = 1,
                            ModifyConnectionSettings = x => x.BasicAuthentication(username, password)
                        })
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.WithProperty("Application", applicationName)
                    .ReadFrom.Configuration(context.Configuration);
            });
        }

        public static void ConfigureIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("IdentitySqlConnection");

            services.AddIdentityServer(options =>
            {
                // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
            })
            // not recomended for production  - you need to store your key material  somewhere secure
            .AddDeveloperSigningCredential()
            .AddConfigurationStore(opt =>
            {
                opt.ConfigureDbContext = c => c.UseSqlServer(connectionString,
                    buider => buider.MigrationsAssembly("TeduMicroservices.IDP"));
            })
            .AddOperationalStore(opt =>
            {
                opt.ConfigureDbContext = c => c.UseSqlServer(
                    connectionString,
                    buider => buider.MigrationsAssembly("TeduMicroservices.IDP"));
            })
            .AddAspNetIdentity<User>()
            .AddProfileService<IdentityProfileService>();
        }

        public static void ConfigureIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("IdentitySqlConnection");
            services
                .AddDbContext<TeduIdentityContext>(options => options
                    .UseSqlServer(connectionString))
                .AddIdentity<User, IdentityRole>(opt =>
                {
                    opt.Password.RequireDigit = false;
                    opt.Password.RequiredLength = 6;
                    opt.Password.RequireUppercase = false;
                    opt.Password.RequireLowercase = false;
                    opt.User.RequireUniqueEmail = true;
                    opt.Lockout.AllowedForNewUsers = true;
                    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    opt.Lockout.MaxFailedAccessAttempts = 3;
                })
                .AddEntityFrameworkStores<TeduIdentityContext>()
                .AddUserStore<IdentityUserStore>()
                .AddDefaultTokenProviders();
        }

        public static void ConfigureSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Tedu Identity Server API",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Name = "Tedu Identity Service",
                        Email = "kietpham.dev@gmail.com",
                        Url = new Uri("https://kietpham.dev")
                    }
                });
                var identityServerBaseUrl = configuration.GetSection("IdentityServer:BaseUrl").Value;
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{identityServerBaseUrl}/connect/authorize"),
                            Scopes = new Dictionary<string, string>
                        {
                            { "tedu_microservices_api.read", "Tedu Microservices API Read Scope" },
                            { "tedu_microservices_api.write", "Tedu Microservices API Write Scope" }
                        }
                        }
                    }
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    new List<string>
                    {
                        "tedu_microservices_api.read",
                        "tedu_microservices_api.write"
                    }
                }
            });
            });
        }

        public static void ConfigureAuthentication(this IServiceCollection services)
        {
            services
                .AddAuthentication()
                .AddLocalApi("Bearer", option =>
                {
                    option.ExpectedScope = "tedu_microservices_api.read";
                });
        }

        public static void ConfigureAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(
                options =>
                {
                    options.AddPolicy("Bearer", policy =>
                    {
                        policy.AddAuthenticationSchemes("Bearer");
                        policy.RequireAuthenticatedUser();
                    });
                });
        }

    }
}
