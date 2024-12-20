using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
                {
                    //options.ListenAnyIP(7004);
                    // Configure Kestrel to listen on port 7004
                    options.ListenAnyIP(7004, listenOptions =>
                    {
                        listenOptions.UseHttps();// Optional: Configure if needed
                    });
                });
                webBuilder.UseStartup<Startup>();
            });
}

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient("client")
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                    {
                        return true;
                        // Trust the self-signed certificate used by the API Gateway
                        //return cert != null && cert.Subject == "CN=localhost";
                    }
                };
                return handler;
            });
        services.AddOcelot();//.AddDelegatingHandler<IgnoreSslCertificateHandler>(); // Add your custom handler

    }

    public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        await app.UseOcelot();
    }
}
/*
public class IgnoreSslCertificateHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Ignore SSL certificate validation for this request
        if (request.RequestUri != null)
        {
            // Set callback only for this request
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            using (var httpClient = new HttpClient(httpClientHandler))
            {
                return await httpClient.SendAsync(request, cancellationToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}*/
