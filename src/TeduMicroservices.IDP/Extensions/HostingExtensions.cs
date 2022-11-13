using Microsoft.AspNetCore.Mvc;
using Serilog;
using TeduMicroservices.IDP.Infrastructure.Domains;
using TeduMicroservices.IDP.Infrastructure.Repositories;
using TeduMicroservices.IDP.Presentation;
using TeduMicroservices.IDP.Services.EmailService;

namespace TeduMicroservices.IDP.Extensions;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // uncomment if you want to add a UI
        builder.Services.AddConfigurationSettings(builder.Configuration);
        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddRazorPages();

        // Add services to the container
        builder.Services.AddScoped<IEmailSender, SmtpMailService>();
        builder.Services.ConfigureCookiePolicy();
        builder.Services.ConfigureCors();

        builder.Services.ConfigureIdentity(builder.Configuration);
        builder.Services.ConfigureIdentityServer(builder.Configuration);

        builder.Services.AddTransient(typeof(IUnitOfWork),
         typeof(UnitOfWork));
        builder.Services.AddTransient(typeof(IRepositoryBase<,>),
            typeof(RepositoryBase<,>));
        builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();
        builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();

        builder.Services.AddControllers(config =>
        {
            config.RespectBrowserAcceptHeader = true;
            config.ReturnHttpNotAcceptable = true;
            config.Filters.Add(new ProducesAttribute("application/json", "text/plain", "text/json"));
        }).AddApplicationPart(typeof(AssemblyReference).Assembly);


        builder.Services.ConfigureAuthentication();
        builder.Services.ConfigureAuthorization();
        builder.Services.ConfigureSwagger(builder.Configuration);

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseCors("CorsPolicy");
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.OAuthClientId("tedu_microservices_swagger");
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tedu Identity API");
            c.DisplayRequestDuration();
        });
        app.UseRouting();
        app.UseMiddleware<ErrorWrappingMiddleware>();
        app.UseIdentityServer();

        //set cookie policy before authentication/authorization setup
        app.UseCookiePolicy();

        // uncomment if you want to add a UI
        app.UseAuthorization();

        app.UseEndpoints(enpoints =>
        {
            enpoints.MapDefaultControllerRoute().RequireAuthorization("Bearer");
            enpoints.MapRazorPages().RequireAuthorization();
        });

        return app;
    }
}
