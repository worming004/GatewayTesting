using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Gateway.Tests;

public class UnitTest1 
{
    [Fact]
    public async Task ObserveHttpMock()
    {
        var gateway = Program.GetApp(new string[0]);
        var cancelAllHosts = new CancellationTokenSource();
        var gatewayIsReady = false;
        gateway.Lifetime.ApplicationStarted.Register(() => gatewayIsReady = true);
        gateway.RunAsync(cancelAllHosts.Token);
        while (!gatewayIsReady)
        {
            await Task.Delay(1000);
        }

        var expectedString = "Yo mama, i am the request from the mock http server";
        var testHost = BuildHttpServer(async (ctx, next) =>
        {
            await ctx.Response.WriteAsync(expectedString);
        });
        var testHostIsReady = false;
        var testHostApplicationLifeTime = testHost.Services.GetService(typeof(IHostApplicationLifetime)) as IHostApplicationLifetime;
        testHostApplicationLifeTime.ApplicationStarted.Register(() => testHostIsReady = true);
        testHost.RunAsync(cancelAllHosts.Token);
        while (!gatewayIsReady)
        {
            await Task.Delay(1000);
        }

        var client = new HttpClient ();
//        var client = new HttpClient {BaseAddress = new Uri("http://localhost:5289")};
        var response = await client.GetAsync("http://localhost:5289/Repeat");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Equal(expectedString, responseBody);

        await testHost.StopAsync();
        await gateway.StopAsync();
    }

    private IHost? BuildHttpServer(Func<HttpContext, RequestDelegate, Task> middleware)
    {
        var builder = Host.CreateDefaultBuilder(new string[0]);
        builder.ConfigureWebHostDefaults(bui =>
        {
            bui.UseUrls("http://localhost:3000");
            bui.Configure((ctx, app) =>
            {
                app.Use(middleware);
            });
        });
            
        var app = builder.Build();
        return app;
    }
}
