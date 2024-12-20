using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using IdentityProviderMicroservice.Services;
using IdentityProviderMicroservice.User;
using Grpc.Net.Client;
using System;
using Grpc.Core;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
                {
                   
                    options.ListenAnyIP(7002, listenOptions =>
                    {
                        // Ensure to configure the certificate here if needed
                        listenOptions.UseHttps("/https/Phonebook.pfx", "luka004leho1"); // HTTPS
                    });
                });
                webBuilder.ConfigureServices((context, services) =>
                {
                    services.AddHealthChecks();

                    var configuration = context.Configuration;

                    // Add services to the container.
                    services.AddControllers();

                    // Configure gRPC client for User microservice
                    services.AddGrpcClient<UserGrpc.UserGrpcClient>(options =>
                    {
                        var grpcAddress = Environment.GetEnvironmentVariable("GRPC_USER_SERVICE_ADDRESS");
                        options.Address = new Uri(grpcAddress);
                        // Use secure channel options for production
                        options.ChannelOptionsActions.Add(channelOptions =>
                        {
                            channelOptions.Credentials = ChannelCredentials.SecureSsl; // Use secure credentials
                        });
                    });

                    services.AddGrpc();

                    // Register the IdentityProvider service
                    services.AddScoped<IIdentityProviderService, IdentityProviderService>();
                    services.AddScoped<JwtTokenService>();
                    services.AddEndpointsApiExplorer();

                    // Configure CORS
                    services.AddCors(options =>
                    {
                        options.AddPolicy("AllowAllOrigins",
                            builder => builder.AllowAnyOrigin()
                                              .AllowAnyMethod()
                                              .AllowAnyHeader());
                    });

                    // Add Swagger services
                    services.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityProvider API", Version = "v1" });
                    });
                });

                webBuilder.Configure((context, app) =>
                {
                    app.UseHealthChecks("/health");

                    var env = context.HostingEnvironment;

                    if (env.IsDevelopment())
                    {
                        app.UseSwagger();
                        app.UseSwaggerUI();
                    }

                    app.UseHttpsRedirection();
                    app.UseRouting();
                    app.UseCors("AllowAllOrigins");
                    app.UseAuthorization();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                        endpoints.MapGrpcService<IdentityProviderService>();
                        endpoints.MapGet("/", async context =>
                        {
                            await context.Response.WriteAsync("Welcome to the IdentityProvider API.");
                        });
                    });

                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityProvider API V1");
                        c.RoutePrefix = string.Empty;
                    });
                });
            });
}
