﻿using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace TeduMicroservices.IDP.Extensions
{
    public static class ServiceExtensions
    {
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
            //.AddInMemoryIdentityResources(Config.IdentityResources)
            //.AddInMemoryApiScopes(Config.ApiScopes)
            //.AddInMemoryApiResources(Config.ApiResource)
            //.AddInMemoryClients(Config.Clients)
            //.AddTestUsers(TestUsers.Users)
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
            });
            //.AddAspNetIdentity<User>()
            //.AddProfileService<IdentityProfileService>();
        }

    }
}